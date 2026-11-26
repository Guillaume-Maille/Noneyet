using TMPro;
using UnityEngine;

public class Manager : MonoBehaviour
{
    [SerializeField] GameObject guidePlanet;
    [SerializeField] GameObject newPlanet;
    [SerializeField] GameObject newGap;
    [SerializeField] private GameObject creationGroup;


    private bool placingPlanet = false;
    private MousePosition mousePositionScript;
    private Vector3 position;
    public bool IsPlacing => placingPlanet;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mousePositionScript = guidePlanet.GetComponent<MousePosition>();
    }

    public void CloseCreateMenu()
    {
        creationGroup.SetActive(false);
    }

    public void PlacePlanet()
    {
        creationGroup.SetActive(false);
        guidePlanet.SetActive(true);
        placingPlanet = true;
    }

        // Update is called once per frame
        void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlacePlanet();
        }

        if (placingPlanet && (Input.GetKeyDown(KeyCode.Mouse0)))
        {
            position = mousePositionScript.planetPlacement;

            Instantiate(newPlanet, position, Quaternion.identity);

            creationGroup.SetActive(true);

            placingPlanet = false;
            guidePlanet.SetActive(false);

        }

        if (placingPlanet && (Input.GetKeyDown(KeyCode.Q)))
        {
            position = mousePositionScript.planetPlacement;

            Instantiate(newGap, position, Quaternion.identity);

            creationGroup.SetActive(true);

            placingPlanet = false;
            guidePlanet.SetActive(false);

        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            creationGroup.SetActive(false);

            if (placingPlanet)
            {
                placingPlanet = false;
                guidePlanet.SetActive(false);
            }
        }    



    }
}
