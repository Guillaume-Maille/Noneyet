using TMPro;
using UnityEngine;

public class FieldInputUIGrabber : MonoBehaviour
{

    [Header("The value we got from the input field")]
    [SerializeField] private string inputText;

    [Header("Showing the output to the player")]
    [SerializeField] private GameObject reactionGroup;
    [SerializeField] private TMP_Text reactionTextBox;


    public void GrabInputField(string input)
    {
        inputText = input;
        DisplayReactionToInput();
    }

    private void DisplayReactionToInput()
    {
        reactionTextBox.text = inputText;
        reactionGroup.SetActive(true);
    }
}