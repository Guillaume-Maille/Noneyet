using UnityEngine;

// Pins a 3D object to a spot on the screen, like UI, but it stays a real,
// lit 3D model. Put this on an object that is a DIRECT CHILD of the camera.
//
// It keeps the same screen position and size when the camera moves, zooms,
// switches between perspective and orthographic, or the window is resized.
//
// [ExecuteAlways] makes it also run outside Play mode, so you can see changes
// to Screen Position, Screen Height and Depth straight away in the editor.
// Note: this script controls the object's position, scale and rotation. Use
// the fields on this component to change them, not the Transform tools.
[ExecuteAlways]
[DefaultExecutionOrder(200)] // after IsometricCamera has moved and zoomed this frame
public class HudAnchor : MonoBehaviour
{
    // Which side of the model is its front (the side with the buttons, screen, etc.).
    public enum FrontAxis { AsIs, PlusZ, MinusZ, PlusY, MinusY, PlusX, MinusX }

    [SerializeField] private Camera cam;

    [Tooltip("Show position and size changes in the editor. Turn off to edit the Transform by hand outside Play mode.")]
    [SerializeField] private bool previewInEditor = true;

    [Tooltip("Where on screen the object's centre goes. (0,0) = bottom-left, (1,1) = top-right.")]
    [SerializeField] private Vector2 screenPosition = new Vector2(0.5f, 0.15f);

    [Header("Size")]
    [Tooltip("On: size the object by Screen Height, so it looks the same size however the camera zooms. " +
             "Off: use Scale exactly like the Transform's Scale.")]
    [SerializeField] private bool fitToScreenHeight = true;

    [Tooltip("How tall the object should look, as a fraction of the screen height (0.2 = a fifth). Only used when Fit To Screen Height is on.")]
    [SerializeField] private float screenHeight = 0.2f;

    [Tooltip("Fit on: multiplies the fitted size (1,1,1 = unchanged, (2,1,1) = twice as wide). " +
             "Fit off: the object's scale, same as the Transform's Scale field.")]
    [SerializeField] private Vector3 scale = Vector3.one;

    [Tooltip("Distance in front of the camera, in world units. Keep it small so the world stays behind it, " +
             "but bigger than the camera's Near clipping plane.")]
    [SerializeField] private float depth = 1.5f;

    [Header("Rotation")]
    [Tooltip("Which side of the model is its front. Set this and Rotation (0,0,0) means: front facing the camera, top pointing up.\n" +
             "As Is = no correction (old behaviour). Flat panels built lying face-up are usually Plus Y.\n" +
             "If you see the back, pick the opposite (Plus <-> Minus).")]
    [SerializeField] private FrontAxis front = FrontAxis.AsIs;

    [Tooltip("Rotation relative to the camera, in degrees. Z spins the object on the screen (like turning a steering wheel), " +
             "X tips it toward/away from you, Y turns it left/right.")]
    [SerializeField] private Vector3 rotation = Vector3.zero;

    // The first time this runs on an object, copy whatever rotation it already had,
    // so adding the Rotation field doesn't reset rotations you've already set up.
    [SerializeField, HideInInspector] private bool rotationCaptured = false;

    // The model's bounding box in its own local space, at scale 1.
    private Vector3 modelSize = Vector3.one;
    private Vector3 modelCentre = Vector3.zero;

    // So each warning only shows once instead of every frame in the editor.
    private bool warnedNoMesh;
    private bool warnedNearClip;

    void Start()
    {
        FindCamera();
        MeasureModel();
    }

    void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            if (!previewInEditor) return;

            // In the editor you might swap the model or add child meshes at any
            // time, so measure again each update. (Cheap: it only looks at the
            // corners of each mesh's box.)
            FindCamera();
            MeasureModel();
        }

        if (cam == null) return;

        CheckNearClip();

        // Rotation first: the size and centring below depend on which way the model faces.
        CaptureExistingRotation();
        Quaternion relativeToCamera = Quaternion.Euler(rotation) * FrontCorrection(front);
        transform.rotation = cam.transform.rotation * relativeToCamera;

        Vector3 finalScale;

        if (fitToScreenHeight)
        {
            // How many world units tall the screen is, at this object's distance.
            float visibleHeight = cam.orthographic
                ? cam.orthographicSize * 2f
                : 2f * depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

            // How tall the model is ON SCREEN (at scale 1), whichever way it's turned.
            // Before, this used the model's own Y size, which for a flat panel is its
            // thickness: that made flat panels scale up hugely.
            float heightOnScreen = MeasuredHeightAlong(Quaternion.Inverse(relativeToCamera) * Vector3.up);

            // The uniform size that makes the model cover the wanted fraction of that height...
            float fit = visibleHeight * screenHeight / heightOnScreen;

            // ...then your Scale on top, so you can stretch or shrink it per axis.
            finalScale = scale * fit;
        }
        else
        {
            finalScale = scale;
        }

        // Skip broken values, e.g. a 0 while you're still typing a number.
        if (!IsUsable(finalScale)) return;
        transform.localScale = finalScale;

        // Turn the screen spot into a point in the world, 'depth' units in front of the camera...
        Vector3 anchor = cam.ViewportToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));

        // ...and put the model's visual centre there (its pivot might not be in the middle).
        transform.position = anchor - transform.rotation * Vector3.Scale(modelCentre, finalScale);
    }

#if UNITY_EDITOR
    // Called when you change a value in the Inspector. Ask Unity to update
    // right away so the Scene and Game views refresh without clicking anything.
    void OnValidate()
    {
        warnedNearClip = false;   // re-check with the new Depth
        if (!Application.isPlaying)
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
    }
#endif

    private void CaptureExistingRotation()
    {
        if (rotationCaptured) return;
        rotation = (Quaternion.Inverse(cam.transform.rotation) * transform.rotation).eulerAngles;
        rotationCaptured = true;
#if UNITY_EDITOR
        // Make sure the editor saves the copied rotation with the scene.
        if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // Turns the model so its chosen front points at the camera and its top points up.
    // The camera looks along its +Z, so "toward the camera" is -Z in camera space.
    private static Quaternion FrontCorrection(FrontAxis axis)
    {
        Vector3 modelFront, modelUp;   // in the model's own space
        switch (axis)
        {
            case FrontAxis.PlusZ:  modelFront = Vector3.forward; modelUp = Vector3.up;      break;
            case FrontAxis.MinusZ: modelFront = Vector3.back;    modelUp = Vector3.up;      break;
            case FrontAxis.PlusY:  modelFront = Vector3.up;      modelUp = Vector3.forward; break; // lying face-up
            case FrontAxis.MinusY: modelFront = Vector3.down;    modelUp = Vector3.forward; break; // lying face-down
            case FrontAxis.PlusX:  modelFront = Vector3.right;   modelUp = Vector3.up;      break;
            case FrontAxis.MinusX: modelFront = Vector3.left;    modelUp = Vector3.up;      break;
            default:               return Quaternion.identity;                                     // As Is
        }

        Quaternion wanted = Quaternion.LookRotation(Vector3.back, Vector3.up);
        Quaternion model = Quaternion.LookRotation(modelFront, modelUp);

        // Undo the model's own orientation, then apply the one we want.
        return wanted * Quaternion.Inverse(model);
    }

    // How long the model's bounding box is along a direction (in the model's own space).
    private float MeasuredHeightAlong(Vector3 direction)
    {
        float h = Mathf.Abs(direction.x) * modelSize.x
                + Mathf.Abs(direction.y) * modelSize.y
                + Mathf.Abs(direction.z) * modelSize.z;
        return Mathf.Max(h, 0.0001f);
    }

    private static bool IsUsable(Vector3 s)
    {
        return s.x > 0f && s.y > 0f && s.z > 0f &&
               !float.IsNaN(s.x) && !float.IsNaN(s.y) && !float.IsNaN(s.z) &&
               !float.IsInfinity(s.x) && !float.IsInfinity(s.y) && !float.IsInfinity(s.z);
    }

    private void FindCamera()
    {
        if (cam == null) cam = GetComponentInParent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    private void CheckNearClip()
    {
        if (warnedNearClip || depth > cam.nearClipPlane) return;
        Debug.LogWarning($"HudAnchor on {name}: Depth ({depth}) is inside the camera's Near clipping plane ({cam.nearClipPlane}), so it won't be visible.", this);
        warnedNearClip = true;
    }

    // Finds the model's bounding box in this object's local space, including child meshes.
    private void MeasureModel()
    {
        MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
        bool first = true;
        Bounds local = new Bounds();

        foreach (MeshFilter mf in meshes)
        {
            if (mf.sharedMesh == null) continue;
            Bounds mb = mf.sharedMesh.bounds;

            // Check all 8 corners of each mesh's box.
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = mb.center + Vector3.Scale(mb.extents, new Vector3(
                    (i & 1) == 0 ? -1 : 1,
                    (i & 2) == 0 ? -1 : 1,
                    (i & 4) == 0 ? -1 : 1));

                // mesh space -> world -> this object's local space.
                // InverseTransformPoint also removes this object's own scale and
                // rotation, so this measures the model as if it were at scale 1 and
                // unrotated. That's why measuring again after the script has resized
                // or turned it gives the same answer.
                Vector3 p = transform.InverseTransformPoint(mf.transform.TransformPoint(corner));

                if (first) { local = new Bounds(p, Vector3.zero); first = false; }
                else local.Encapsulate(p);
            }
        }

        if (!first && local.size.sqrMagnitude > 0.00000001f)
        {
            modelSize = local.size;
            modelCentre = local.center;
            warnedNoMesh = false;
        }
        else if (!warnedNoMesh)
        {
            Debug.LogWarning($"HudAnchor on {name}: couldn't find a mesh to measure, assuming size 1.", this);
            warnedNoMesh = true;
        }
    }
}
