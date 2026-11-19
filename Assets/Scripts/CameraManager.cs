using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform target;          // the spaceship
    [SerializeField] private Camera cam;

    [Header("Isometric angle")]
    [SerializeField] private float pitch = 35.264f;     // true isometric
    [SerializeField] private float yaw = 45f;

    [Header("Projection")]
    [SerializeField] private bool useOrthographic = false; // false = Option A (recommended on HDRP)
    [SerializeField] private float telephotoFov = 8f;      // Option A
    [SerializeField] private float orthoSize = 20f;        // Option B

    [Header("Controls")]
    [SerializeField] private float distance = 200f;
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float rotateSpeed = 90f;

    void Start()
    {
        if (cam == null) cam = GetComponent<Camera>();
        cam.orthographic = useOrthographic;
        if (useOrthographic) cam.orthographicSize = orthoSize;
        else cam.fieldOfView = telephotoFov;
    }

    void LateUpdate()
    {
        // Orbit around the ship (Z/X aren't used elsewhere in your project)
        if (Input.GetKey(KeyCode.Z)) yaw += rotateSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.X)) yaw -= rotateSpeed * Time.deltaTime;

        // Zoom with the scroll wheel
        float scroll = Input.mouseScrollDelta.y;
        if (useOrthographic)
            cam.orthographicSize = Mathf.Max(1f, cam.orthographicSize - scroll * zoomSpeed * 0.1f);
        else
            distance = Mathf.Max(10f, distance - scroll * zoomSpeed);

        // Place the camera on a fixed offset from the ship and look at it
        Quaternion angle = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target.position - angle * Vector3.forward * distance;
        transform.LookAt(target);
    }
}