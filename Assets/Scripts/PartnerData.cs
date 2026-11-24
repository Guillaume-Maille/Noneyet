using UnityEngine;


[CreateAssetMenu(fileName = "New Partner", menuName = "Partner")]
public class ObjInteraction : ScriptableObject
{
    // data we need
    public string partnerName;
    public string role;
    public string relationship;
    public string attendance;

    // game elements
    public GameObject model;


}
