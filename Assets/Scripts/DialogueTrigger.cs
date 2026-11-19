using UnityEngine;

// Put this on any object that should start a conversation.
public class DialogueTrigger : MonoBehaviour
{
    [Header("What conversation to play")]
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private DialogueManager dialogueManager;

    [Header("When should it start?")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool playOnTriggerEnter = false;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlyPlayOnce = false;

    private bool hasPlayed = false;

    void Start()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
        }

        if (playOnStart)
        {
            TriggerDialogue();
        }
    }

    // Also usable from a Button's OnClick
    public void TriggerDialogue()
    {
        if (onlyPlayOnce && hasPlayed) return;
        if (DialogueManager.IsDialogueOpen) return;

        dialogueManager.StartDialogue(dialogue);
        hasPlayed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playOnTriggerEnter && other.CompareTag(playerTag))
        {
            TriggerDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playOnTriggerEnter && other.CompareTag(playerTag))
        {
            TriggerDialogue();
        }
    }
}
