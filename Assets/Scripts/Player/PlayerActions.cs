using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{


    // --- field references ---

    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerCamera playerCamera;

    /// <summary>
    /// The champion npcs controlled by the player
    /// </summary>
    private List<NPC> playerMercenaries = new List<NPC>();
    private List<NPC> playerNPCs = new List<NPC>();
    private List<NPC> selectedNpcs = new List<NPC>();

    
    private NPC _mainSelectedNPC;
    private NPC mainSelectedNPC {
        get => _mainSelectedNPC;
        set {
            if (_mainSelectedNPC != null) {
                _mainSelectedNPC.npcVisual.OnMainSelectedChanged(false);
            }
            if (value != null) {
                value.npcVisual.OnMainSelectedChanged(true);
            }
            _mainSelectedNPC = value;
        }
    }


    private bool isInCameraControlMode = false;

    void Start()
    {
        // subscribe to events

        playerInputHandler.OnMouse1 += Mouse1Click;
        playerInputHandler.OnMouse2 += Mouse2Click;

        playerInputHandler.OnAbilityReleased += TryCastAbility;

        NpcManager.instance.OnNPCSpawned += OnNPCSpawned;
    }

    void Update()
    {
        HandlePlayerCameraCommunication();
    }

    private void Mouse2Click(object sender, InputAction.CallbackContext e)
    {

        NPC clickedNpc = playerCamera.GetHoveredNPC();

        if (clickedNpc == null) { // if the player clicked the ground
            // set the movement destination of the selected npc to the clicked position
            if (!isInCameraControlMode && mainSelectedNPC != null) {
                foreach (NPC npc in selectedNpcs) {
                    TrySetNpcMovementDestinationToClickedPosition(npc); // add some space between the npcs later
                }
            }

        } else {
            // if the player right clicked on an npc

            if (!clickedNpc.isOwnedByPlayer) { // if an enemy was right clicked
                
                bool shiftPressed = playerInputHandler.GetShiftButtonPressed(); // if shift was pressed, multi target the enemies

                foreach (NPC npc in selectedNpcs) {
                    if (npc.nPCSO.isMercenary && npc.isOwnedByPlayer) {
                        if (npc.npcBehaviour is NPCBehaviourMercenary mercBehaviour) {
                            if (!shiftPressed) mercBehaviour.ClearPlayerTargettedEnemyNPCs(); // single selected npc if shift is not pressed
                            mercBehaviour.ClearNaturallyTargettedEnemyNPCs(); // prioritize the player targetted npcs
                            mercBehaviour.AddPlayerAttackTarget(clickedNpc);
                        }
                    }
                }
            }
        }
    }


    private void Mouse1Click(object sender, InputAction.CallbackContext e)
    {
        if (!isInCameraControlMode)
        {
            SetSelectedNPC();
        }
    }

    private void SetSelectedNPC()
    {
        
        // try and select an npc, prioritize mercenaries
        NPC hoveredNPC = playerCamera.GetHoveredNPC(prioritizeMercenaries: true);

        if (!playerInputHandler.GetShiftButtonPressed()) RemoveAllSelectedNPCs(); // if shift is not pressed, clear the selected npcs
        AddSelectedNPC(hoveredNPC);

        // no npc was clicked. Deselect all npcs
        if (hoveredNPC == null) {
            mainSelectedNPC = null;
            RemoveAllSelectedNPCs();
        }

        mainSelectedNPC = hoveredNPC; // can be null if no npc is hovered, ie deselecting. Dont need to set it if it already is the same.
    }

    private void AddSelectedNPC(NPC npc) {
        if (!selectedNpcs.Contains(npc) && npc != null) {
            selectedNpcs.Add(npc);
            npc.npcVisual.OnNPCSelectedChanged(true);
        }
    }

    private void RemoveSelectedNPC(NPC npc) {
        if (selectedNpcs.Contains(npc)) {
            selectedNpcs.Remove(npc);
            npc?.npcVisual.OnNPCSelectedChanged(false);
        }
    }

    private void RemoveAllSelectedNPCs() {
        foreach (NPC npc in selectedNpcs) {
            npc?.npcVisual.OnNPCSelectedChanged(false);
        }
        selectedNpcs.Clear();
    }


    private void OnNPCSpawned(object sender, NPC npc)
    {
        if (npc.isOwnedByPlayer)
        {
            playerNPCs.Add(npc);
            if (npc.nPCSO.isMercenary) playerMercenaries.Add(npc);
        }
    }


    /// <summary>
    /// Updates the cameras control mode.
    /// </summary>
    private void HandlePlayerCameraCommunication() {
        isInCameraControlMode = playerInputHandler.GetCameraControlButtonPressed();
        playerCamera.SetCameraControlMode(isInCameraControlMode);
    }

    /// <summary>
    /// Sets the movement destination of some player controlled npcs to the clicked position.
    /// </summary>
    private void TrySetNpcMovementDestinationToClickedPosition(NPC npc)
    {

        if (npc.nPCSO.isMercenary && npc.isOwnedByPlayer) {

            WorldClickData data = playerCamera.GetClickedTerrainPosition();
            if (data.hit)
            {
                Vector2Int coords = WorldDataGenerator.instance.GetWorldCoordinates(data.hitPoint);
                if (npc.npcBehaviour is NPCBehaviourMercenary mercBehaviour) {
                    // call the overridable method in the mercenary behaviour
                    mercBehaviour.OnMovementTargetSetByPlayer(coords);
                }
            }
        }

    }




    // ---------------- ABILITIES ----------------

    private void TryCastAbility(object sender, int abilityIndex) {
        if (mainSelectedNPC != null && mainSelectedNPC.isOwnedByPlayer && mainSelectedNPC.nPCSO.isMercenary) {
            bool succeded = mainSelectedNPC.abilityList.TryCastAbilityAtIndex(abilityIndex);
        }
    }
}
