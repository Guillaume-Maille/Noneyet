using UnityEngine;

// Runs after default scripts (including IsometricCamera), so it always
// uses the camera's final rotation for this frame.
[DefaultExecutionOrder(100)]
public class Billboard : MonoBehaviour
{
    public enum Mode
    {
        MatchCamera,   // Face the screen fully. Best for isometric/orthographic views.
        UprightOnly    // Turn left/right only, stay vertical (like a signpost).
    }

    [SerializeField] private Mode mode = Mode.MatchCamera;
    [SerializeField] private Camera targetCamera;   // Leave empty to use the Main Camera.

    void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 forward = targetCamera.transform.forward;

        if (mode == Mode.UprightOnly)
        {
            forward.y = 0f;                        // Ignore the camera's downward tilt.
            if (forward.sqrMagnitude < 0.0001f) return; // Camera looking straight down.
        }

        // Point the quad the same way the camera points. A Unity Quad's visible
        // side faces its -Z, so matching the camera's forward shows that side
        // to the camera.
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}
