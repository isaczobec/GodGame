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


    [SerializeField] private LayerMask npcSelectionHitBoxLayerMask;


    private Vector3 orbitPoint;
    private bool isOrbiting = false;


    private bool cameraControlModeEnabled = false;
    public void SetCameraControlMode(bool enabled) {
        cameraControlModeEnabled = enabled;
    }


    public static PlayerCamera Instance { get; private set; }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }


    private void Start() {
        Cursor.lockState = CursorLockMode.Confined;
        PlayerInputHandler.Instance.OnMouse2 += Mouse2Click;
        PlayerInputHandler.Instance.OnMouse2Released += Mouse2Released;

    }


    private void Update() {
        HandleCamerMovenemt();
        HandleOrbiting();
    }

    private void HandleCamerMovenemt() {

        if (isOrbiting && cameraControlModeEnabled) return;

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
        if (!isOrbiting || !cameraControlModeEnabled) return;

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




    public WorldClickData GetClickedTerrainPosition() {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            return new WorldClickData { hitPoint = hit.point, hit = true };
        } else {
            return new WorldClickData { hit = false };
        }
    }



    /// <summary>
    /// Gets the npc that is currently hovered by the mouse. Returns null if no npc is hovered.
    /// </summary>
    /// <returns></returns>
    public NPC GetHoveredNPC(bool prioritizeMercenaries = false) {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.RaycastAll(ray, 10000f, npcSelectionHitBoxLayerMask).Length > 0) { // if we hit something

            float largestDotProduct = float.MinValue;
            NPCSelectionHitBox bestHitBox = null;
            NPCSelectionHitBox bestMercenaryHitBox = null;

            RaycastHit[] hits = Physics.RaycastAll(ray, 10000f, npcSelectionHitBoxLayerMask);
            if (hits.Length == 0) return null; // nothing hit
            for (int i = 0; i < hits.Length; i++) {

                NPCSelectionHitBox selectionHitBox = hits[i].collider.GetComponent<NPCSelectionHitBox>();
                float dotProduct = Vector3.Dot(ray.direction, (selectionHitBox.GetMiddlePoint().position - Camera.main.transform.position).normalized);
                if (dotProduct > largestDotProduct) {
                    largestDotProduct = dotProduct;
                    bestHitBox = selectionHitBox;
                    if (selectionHitBox.GetIsMercenary() && prioritizeMercenaries) {
                        bestMercenaryHitBox = selectionHitBox;
                    }
                }
                
            }

            NPCSelectionHitBox definitiveBestHitbox;
            if (prioritizeMercenaries) {
                if (bestMercenaryHitBox != null) {
                    definitiveBestHitbox = bestMercenaryHitBox;
                } else {
                    definitiveBestHitbox = bestHitBox;
                }
            } else {
                definitiveBestHitbox = bestHitBox;
            }

            NPC npc = definitiveBestHitbox.GetNPC();
            if (npc != null) {
                return npc;
            }
        }

        return null;
    }
}

public class WorldClickData {
    public Vector3 hitPoint;
    public bool hit;
}