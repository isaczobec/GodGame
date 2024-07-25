using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CopyCameraSettings : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    private Camera thisCamera;
    private bool copyTransform;


    private void Awake() {
        thisCamera = GetComponent<Camera>();
        if (thisCamera == null) {
            Debug.LogError("No Camera component found on this object");
        }
    }
    private void Update() {
        if (copyTransform) {
            transform.position = targetCamera.transform.position;
            transform.rotation = targetCamera.transform.rotation;
        }
        thisCamera.orthographic = targetCamera.orthographic;
        thisCamera.orthographicSize = targetCamera.orthographicSize;
        thisCamera.fieldOfView = targetCamera.fieldOfView;
        thisCamera.nearClipPlane = targetCamera.nearClipPlane;
        thisCamera.farClipPlane = targetCamera.farClipPlane;
        

    }
}
