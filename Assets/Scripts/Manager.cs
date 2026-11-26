using UnityEngine;
using UnityEngine.EventSystems;

public class Manager : MonoBehaviour
{
    // What the player is placing right now.
    private enum PlacingMode { None, Planet, Asteroid }

    [Header("Planets")]
    [SerializeField] GameObject guidePlanet;
    [SerializeField] GameObject newPlanet;
    [SerializeField] GameObject newGap;
    [SerializeField] private GameObject creationGroup;

    [Header("Asteroids")]
    [Tooltip("One is picked at random each time. Give each a collider if you want click-to-zoom on it.")]
    [SerializeField] private GameObject[] asteroidPrefabs;
    [Tooltip("A see-through preview that follows the mouse. Must NOT have a collider.")]
    [SerializeField] private GameObject guideAsteroid;
    [Tooltip("Optional: an empty object to put spawned asteroids under, to keep the Hierarchy tidy.")]
    [SerializeField] private Transform asteroidParent;
    [Tooltip("The centre of the ring asteroids are placed in. Leave empty to use (0,0,0).")]
    [SerializeField] private Transform centre;
    [SerializeField] private float minRadius = 3f;
    [SerializeField] private float maxRadius = 15f;
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.7f, 1.3f);
    [Tooltip("Hold this while clicking to keep placing more asteroids.")]
    [SerializeField] private KeyCode keepPlacingKey = KeyCode.LeftShift;

    [Header("Mouse aiming for asteroids")]
    [SerializeField] private Camera cam;
    [Tooltip("The same layer your placement plane is on (the one MousePosition uses).")]
    [SerializeField] private LayerMask placementLayer;

    private PlacingMode mode = PlacingMode.None;
    private MousePosition mousePositionScript;
    private Vector3 position;

    private Vector3 asteroidPlacement;
    private bool hasAsteroidPlacement;

    // Other scripts (like ClickToZoom) check this to leave clicks alone while placing.
    public bool IsPlacing => mode != PlacingMode.None;

    void Start()
    {
        mousePositionScript = guidePlanet.GetComponent<MousePosition>();
        if (cam == null) cam = Camera.main;
        if (guideAsteroid != null) guideAsteroid.SetActive(false);
    }

    public void CloseCreateMenu()
    {
        creationGroup.SetActive(false);
    }

    // Starts planet placement (C key, or a button).
    public void PlacePlanet()
    {
        StopPlacing();
        creationGroup.SetActive(false);
        guidePlanet.SetActive(true);
        mode = PlacingMode.Planet;
    }

    // Starts asteroid placement. Hook this to your UI Button's OnClick.
    public void PlaceAsteroid()
    {
        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0)
        {
            Debug.LogWarning("Manager: no asteroid prefabs assigned.", this);
            return;
        }

        StopPlacing();
        creationGroup.SetActive(false);
        mode = PlacingMode.Asteroid;
        hasAsteroidPlacement = false;
    }

    // Cancels whatever is being placed and hides the previews.
    public void StopPlacing()
    {
        mode = PlacingMode.None;
        hasAsteroidPlacement = false;
        guidePlanet.SetActive(false);
        if (guideAsteroid != null) guideAsteroid.SetActive(false);
    }

    void Update()
    {
        // Ignore input while someone is talking.
        if (DialogueManager.IsDialogueOpen) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            PlacePlanet();
        }

        // Right-click closes the menu and cancels any placing.
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            creationGroup.SetActive(false);
            StopPlacing();
            return;
        }

        // Escape cancels asteroid placing too.
        if (mode == PlacingMode.Asteroid && Input.GetKeyDown(KeyCode.Escape))
        {
            StopPlacing();
            return;
        }

        if (mode == PlacingMode.Planet) UpdatePlanetPlacing();
        else if (mode == PlacingMode.Asteroid) UpdateAsteroidPlacing();
    }

    // ---------------- Planets (same behaviour as before) ----------------

    private void UpdatePlanetPlacing()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !PointerOverUI())
        {
            PlaceAtMouse(newPlanet);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            PlaceAtMouse(newGap);
        }
    }

    private void PlaceAtMouse(GameObject prefab)
    {
        position = mousePositionScript.planetPlacement;
        Instantiate(prefab, position, Quaternion.identity);

        creationGroup.SetActive(true);
        StopPlacing();
    }

    // ---------------- Asteroids ----------------

    private void UpdateAsteroidPlacing()
    {
        UpdateAsteroidAim();

        if (Input.GetKeyDown(KeyCode.Mouse0) && hasAsteroidPlacement && !PointerOverUI())
        {
            SpawnAsteroid();

            // Holding Shift keeps placing; otherwise we're done.
            if (!Input.GetKey(keepPlacingKey)) StopPlacing();
        }
    }

    // Find where the mouse points on the placement plane, then pull that point into the ring.
    private void UpdateAsteroidAim()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, placementLayer))
        {
            hasAsteroidPlacement = false;
            if (guideAsteroid != null) guideAsteroid.SetActive(false);
            return;
        }

        Vector3 c = centre != null ? centre.position : Vector3.zero;

        // Use the flat (horizontal) distance from the centre, and keep the plane's height.
        Vector3 offset = hit.point - c;
        float height = offset.y;
        offset.y = 0f;

        float distance = offset.magnitude;
        if (distance < 0.0001f)
            offset = Vector3.forward * minRadius;                    // mouse exactly on the centre
        else
            offset = offset / distance * Mathf.Clamp(distance, minRadius, maxRadius);

        offset.y = height;
        asteroidPlacement = c + offset;
        hasAsteroidPlacement = true;

        if (guideAsteroid != null)
        {
            guideAsteroid.SetActive(true);
            guideAsteroid.transform.position = asteroidPlacement;
        }
    }

    private void SpawnAsteroid()
    {
        GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
        if (prefab == null) return;

        Quaternion rotation = randomRotation ? Random.rotation : Quaternion.identity;
        GameObject asteroid = Instantiate(prefab, asteroidPlacement, rotation, asteroidParent);
        asteroid.transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);
    }

    // ---------------- Helpers ----------------

    private bool PointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // Shows the asteroid ring in the Scene view when this object is selected.
    void OnDrawGizmosSelected()
    {
        Vector3 c = centre != null ? centre.position : Vector3.zero;
        Gizmos.color = new Color(1f, 0.6f, 0f);
        Gizmos.DrawWireSphere(c, minRadius);
        Gizmos.DrawWireSphere(c, maxRadius);
    }
}
