using UnityEngine;

public class MousePosition : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layerMask;

    public Vector3 planetPlacement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //print(ray);   debugging planet position stuck at 0,0,0
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            //print('a');   debugging planet position stuck at 0,0,0
            transform.position = raycastHit.point;
            planetPlacement = raycastHit.point;
        }
    }
}
