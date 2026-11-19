using UnityEngine;

// One line of dialogue: who is talking, their face, and what they say.
[System.Serializable]
public class DialogueLine
{
    public enum PortraitSide { Left, Right }

    [Header("Speaker")]
    public string speakerName;
    public Sprite portrait;
    public PortraitSide side = PortraitSide.Left;

    [Header("Text")]
    [TextArea(3, 6)]
    public string text;
}

// A full conversation you can create in the Project window:
// Right click > Create > Dialogue > Conversation
[CreateAssetMenu(fileName = "NewConversation", menuName = "Dialogue/Conversation")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}
