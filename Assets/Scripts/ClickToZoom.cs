using UnityEngine;
using UnityEngine.EventSystems;

// Click any object with a collider to glide the camera over and zoom in.
// Click empty space (or press Escape) to glide back to the ship.
//
// Runs before Manager.cs, so on the click that places a planet,
// Manager is still in "placing" mode and this script ignores the click.
[DefaultExecutionOrder(-50)]
public class ClickToZoom : MonoBehaviour
{
    [SerializeField] private IsometricCamera isoCamera;
    [SerializeField] private Camera cam;
    [SerializeField] private Manager manager;

    [Tooltip("Layers you can click to zoom. Untick the placement plane's layer here.")]
    [SerializeField] private LayerMask clickableLayers = ~0;

    [Tooltip("1 = object fills the screen height, 2 = object takes up half.")]
    [SerializeField] private float padding = 1.5f;

    [SerializeField] private KeyCode backKey = KeyCode.Escape;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (isoCamera == null) isoCamera = cam.GetComponent<IsometricCamera>();
        if (manager == null) manager = FindFirstObjectByType<Manager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(backKey))
        {
            isoCamera.ReturnHome();
            return;
        }

        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;

        // Leave clicks alone when something else owns them.
        if (DialogueManager.IsDialogueOpen) return;
        if (manager != null && manager.IsPlacing) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // clicked on UI

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, clickableLayers))
        {
            isoCamera.FocusOn(hit.collider.transform, hit.collider.bounds, padding);
        }
        else if (isoCamera.IsFocusedOnObject)
        {
            isoCamera.ReturnHome();   // clicked empty space
        }
    }
}
