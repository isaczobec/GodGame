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
                _mainSelectedNPC.npcVisual.OnNPCSelectedChanged(false);
            }
            if (value != null) {
                value.npcVisual.OnNPCSelectedChanged(true);
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

        NpcManager.instance.OnNPCSpawned += OnNPCSpawned;
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
                
                bool shiftPressed = playerInputHandler.GetShiftButtonPressed(); // if shift was pressed, multi select 

                foreach (NPC npc in selectedNpcs) {
                    if (npc.nPCSO.isMercenary && npc.isOwnedByPlayer) {
                        if (npc.npcBehaviour is NPCBehaviourMercenary mercBehaviour) {
                            if (!shiftPressed) mercBehaviour.ClearTargettedEnemyNPCs(); // single selected npc if shift is not pressed
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
        
        // try and select an npc
        NPC npc = playerCamera.GetHoveredNPC();

        if (mainSelectedNPC != npc) {
            selectedNpcs.Add(npc);
        }

        // no npc was clicked. Deselect all npcs
        if (npc == null) {
            mainSelectedNPC = null;
            selectedNpcs.Clear();
        }

        mainSelectedNPC = npc; // can be null if no npc is hovered, ie deselecting. Dont need to set it if it already is the same.
    }

    void Update()
    {
        HandlePlayerCameraCommunication();
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
}
