using UnityEngine;

public class MousePosition : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;

    public Vector3 planetPlacement;

    // Lets other scripts (like Manager) reuse the same placement layer.
    public LayerMask PlacementMask => layerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            transform.position = raycastHit.point;
            planetPlacement = raycastHit.point;
        }
    }
}
