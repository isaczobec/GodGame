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
        if (!isInCameraControlMode && mainSelectedNPC != null) SetNpcMovementDestinationToClickedPosition(mainSelectedNPC);
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
        if (mainSelectedNPC != npc) mainSelectedNPC = npc; // can be null if no npc is hovered, ie deselecting. Dont need to set it if it already is the same.
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
    private void SetNpcMovementDestinationToClickedPosition(NPC npc)
    {
        WorldClickData data = playerCamera.GetClickedTerrainPosition();
        if (data.hit)
        {
            Vector2Int coords = WorldDataGenerator.instance.GetWorldCoordinates(data.hitPoint);
            npc.SetMovementTarget(coords);
            npc.MoveToNextTileInQueue();
        }
    }
}
