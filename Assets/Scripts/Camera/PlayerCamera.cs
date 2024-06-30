using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{

    /// <summary>
    /// In which margin close to the screen the camera should move
    /// </summary>
    [SerializeField] private float screenMoveMargin = 0.1f;

    [SerializeField] private float cameraMoveSpeed = 700f;


    [SerializeField] private float orbitSpeed = 10f;


    private Vector3 orbitPoint;
    private bool isOrbiting = false;


    private void Start() {
        Cursor.lockState = CursorLockMode.Confined;
        PlayerInputHandler.Instance.OnMouse2 += Mouse2Click;
        PlayerInputHandler.Instance.OnMouse2Released += Mouse2Released;

        PlayerInputHandler.Instance.OnMouse1 += Mouse1Click;
    }


    private void Update() {
        HandleCamerMovenemt();
        HandleOrbiting();
    }

    private void HandleCamerMovenemt() {

        if (isOrbiting) return;

        Vector3 mousePosition = Input.mousePosition;

        Vector2 inputMove = new Vector2(0, 0);

        float pixelsMarginX = Screen.width * screenMoveMargin;
        float pixelsMarginY = Screen.height * screenMoveMargin;

        if (mousePosition.x < pixelsMarginX) {
            inputMove.x = -1 + (mousePosition.x / pixelsMarginX);
        } else if (mousePosition.x > Screen.width - pixelsMarginX) { 
            inputMove.x = 1 - ((Screen.width - mousePosition.x) / pixelsMarginX);
        }

        if (mousePosition.y < pixelsMarginY) {
            inputMove.y = -1 + (mousePosition.y / pixelsMarginY);
        } else if (mousePosition.y > Screen.height - pixelsMarginY) {
            inputMove.y = 1 - ((Screen.height - mousePosition.y) / pixelsMarginY);
        }

        inputMove.x = Mathf.Clamp(inputMove.x, -1, 1);
        inputMove.y = Mathf.Clamp(inputMove.y, -1, 1);

        float moveYFactor = Mathf.Sin(transform.eulerAngles.x * Mathf.Deg2Rad);
        Vector3 yMoveVector = new Vector3(transform.forward.x, 0, transform.forward.z).normalized * moveYFactor;


        Vector3 move = (transform.right * inputMove.x + yMoveVector * inputMove.y) * cameraMoveSpeed * Time.deltaTime;
        

        transform.position += move;
    }

    private void HandleOrbiting() {
        if (!isOrbiting) return;

        float horizontalInput = Input.GetAxis("Mouse X");
        float angle = horizontalInput * orbitSpeed;
        transform.RotateAround(orbitPoint, Vector3.up, angle);

    }


    private void Mouse2Click(object sender, InputAction.CallbackContext e)
    {
        // check if we clicked on terrain and get the point that was clicked to enable orbiting
        WorldClickData orbitData = GetClickedTerrainPosition();
        if (orbitData.hit) {
            orbitPoint = orbitData.hitPoint;
            isOrbiting = true;
        }
    }

    private void Mouse2Released(object sender, InputAction.CallbackContext e)
    {
        isOrbiting = false;
    }


    private class WorldClickData {
        public Vector3 hitPoint;
        public bool hit;
    }


    private WorldClickData GetClickedTerrainPosition() {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            return new WorldClickData { hitPoint = hit.point, hit = true };
        } else {
            return new WorldClickData { hit = false };
        }
    }
    private void Mouse1Click(object sender, InputAction.CallbackContext e)
    {
        WorldClickData data = GetClickedTerrainPosition();
        if (data.hit) {
            Vector2Int coords = WorldDataGenerator.instance.GetWorldCoordinates(data.hitPoint);
            foreach (NPC npc in NpcManager.instance.npcs) {
                npc.SetMovementTarget(coords);
                npc.MoveToNextTileInQueue();
            }
        }
    }
}
