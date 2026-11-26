using UnityEngine;

// Isometric camera that orbits a focus point. By default the focus is the
// spaceship ("home"). Other scripts can call FocusOn() to glide over to an
// object and zoom in, and ReturnHome() to glide back.
//
// [ExecuteAlways] makes it also run outside Play mode, so the Scene and Game
// views show the isometric angle, projection and zoom while you edit.
[ExecuteAlways]
public class IsometricCamera : MonoBehaviour
{
    [SerializeField] private Transform target;          // the spaceship (home focus)
    [SerializeField] private Camera cam;

    [Tooltip("Show the camera's angle and zoom in the editor. Turn off if you want to move the camera by hand outside Play mode.")]
    [SerializeField] private bool previewInEditor = true;

    [Header("Isometric angle")]
    [SerializeField] private float pitch = 35.264f;     // true isometric
    [SerializeField] private float yaw = 45f;

    [Header("Projection")]
    [SerializeField] private bool useOrthographic = false; // false = telephoto perspective (recommended on HDRP)
    [SerializeField] private float telephotoFov = 8f;
    [SerializeField] private float orthoSize = 20f;

    [Header("Controls")]
    [SerializeField] private float distance = 200f;     // home distance (perspective) / fixed distance (orthographic)
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Smoothing")]
    [Tooltip("Roughly how many seconds the glide takes. Smaller = snappier.")]
    [SerializeField] private float smoothTime = 0.4f;

    // "Zoom" means camera distance in perspective mode, and orthographic Size in orthographic mode.
    private float homeZoom;

    // Where the camera is right now (smoothed)...
    private Vector3 focusPoint;
    private float currentZoom;

    // ...and where it's heading.
    private Transform focusObject;      // null = focus on the ship
    private bool hasFocus;              // true while we *mean* to be on an object (catches deleted objects)
    private Vector3 focusOffset;        // from the object's pivot to the centre of its bounds
    private float targetZoom;

    // SmoothDamp keeps track of speed in these.
    private Vector3 focusVelocity;
    private float zoomVelocity;

    public bool IsFocusedOnObject => focusObject != null;

    void Start()
    {
        if (!Application.isPlaying) return;     // edit mode is handled by EditorPreview()

        if (cam == null) cam = GetComponent<Camera>();
        if (target == null)
        {
            Debug.LogError("IsometricCamera: no Target assigned (drag your WorldCentre or ship in).", this);
            enabled = false;
            return;
        }

        ApplyProjection();

        homeZoom = useOrthographic ? orthoSize : distance;
        currentZoom = targetZoom = homeZoom;
        focusPoint = target.position;
    }

    private void ApplyProjection()
    {
        cam.orthographic = useOrthographic;
        if (useOrthographic) cam.orthographicSize = orthoSize;
        else cam.fieldOfView = telephotoFov;
    }

    // Outside Play mode: put the camera straight at its home position, no smoothing or input.
    private void EditorPreview()
    {
        if (!previewInEditor || target == null) return;
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        ApplyProjection();

        Quaternion angle = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = angle;
        transform.position = target.position - angle * Vector3.forward * distance;
    }

#if UNITY_EDITOR
    // Called when you change a value in the Inspector. Ask Unity to run an
    // update straight away so the views refresh without having to click anything.
    void OnValidate()
    {
        if (!Application.isPlaying)
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
    }
#endif

    // Glide to an object and zoom so its bounds fill the view.
    // padding 1.5 = the object takes up about two thirds of the screen height.
    public void FocusOn(Transform obj, Bounds bounds, float padding = 1.5f)
    {
        focusObject = obj;
        hasFocus = true;
        focusOffset = bounds.center - obj.position;

        float radius = Mathf.Max(bounds.extents.magnitude, 0.01f);

        if (useOrthographic)
        {
            // Orthographic Size is half the screen height in world units.
            targetZoom = radius * padding;
        }
        else
        {
            // Distance at which a sphere of this radius fits the vertical field of view.
            float halfFov = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            targetZoom = radius * padding / Mathf.Sin(halfFov);
        }

        targetZoom = Mathf.Max(targetZoom, minZoom);
    }

    // Glide back to the ship at the starting zoom.
    public void ReturnHome()
    {
        focusObject = null;
        hasFocus = false;
        targetZoom = homeZoom;
    }

    void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            EditorPreview();
            return;
        }

        // Orbit (Z/X aren't used elsewhere in the project)
        if (Input.GetKey(KeyCode.Z)) yaw += rotateSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.X)) yaw -= rotateSpeed * Time.deltaTime;

        // Scroll still works while focused: it adjusts the target, and the smoothing does the rest.
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f)
        {
            float step = useOrthographic ? zoomSpeed * 0.1f : zoomSpeed;
            // Scale the step with the current zoom so it feels the same close up and far away.
            step *= targetZoom / homeZoom;
            targetZoom = Mathf.Max(minZoom, targetZoom - scroll * step);
        }

        // If the object we were focused on got deleted, go home.
        if (hasFocus && focusObject == null) ReturnHome();

        Vector3 targetFocus = focusObject != null
            ? focusObject.position + focusOffset
            : target.position;

        // The smooth part: ease the current values toward the targets every frame.
        focusPoint = Vector3.SmoothDamp(focusPoint, targetFocus, ref focusVelocity, smoothTime);
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, smoothTime);

        float camDistance;
        if (useOrthographic)
        {
            cam.orthographicSize = currentZoom;
            camDistance = distance;     // ortho zoom doesn't need the camera to move closer
        }
        else
        {
            camDistance = currentZoom;
        }

        Quaternion angle = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = angle;     // same as LookAt(focusPoint), since we sit on that line
        transform.position = focusPoint - angle * Vector3.forward * camDistance;
    }
}
