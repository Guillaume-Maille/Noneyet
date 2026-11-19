using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // Other scripts (like Manager.cs) can check this to ignore clicks while someone is talking
    public static bool IsDialogueOpen { get; private set; }

    [Header("The whole dialogue box (turned on and off)")]
    [SerializeField] private GameObject dialogueGroup;

    [Header("Character faces")]
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;

    [Header("Text boxes")]
    [SerializeField] private TMP_Text nameTextBox;
    [SerializeField] private TMP_Text dialogueTextBox;
    [SerializeField] private GameObject continueIndicator; // optional little arrow / "press space"

    [Header("Typing settings")]
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;
    [SerializeField] private KeyCode alternateContinueKey = KeyCode.Return;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isTyping = false;
    private Coroutine typingRoutine;

    void Start()
    {
        dialogueGroup.SetActive(false);
        IsDialogueOpen = false;
    }

    // Call this from a button, a trigger, or another script
    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines.Length == 0) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;

        dialogueGroup.SetActive(true);
        IsDialogueOpen = true;
        onDialogueStart.Invoke();

        ShowLine(currentDialogue.lines[currentLineIndex]);
    }

    // Hook this to a "Next" button too, if you want one
    public void ContinueDialogue()
    {
        if (!IsDialogueOpen) return;

        // First press while typing: finish the line instantly
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine(currentDialogue.lines[currentLineIndex]);
        }
    }

    public void EndDialogue()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);

        isTyping = false;
        dialogueGroup.SetActive(false);
        IsDialogueOpen = false;
        currentDialogue = null;
        onDialogueEnd.Invoke();
    }

    private void ShowLine(DialogueLine line)
    {
        nameTextBox.text = line.speakerName;
        ShowPortrait(line);

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(line.text));
    }

    private void ShowPortrait(DialogueLine line)
    {
        bool onLeft = line.side == DialogueLine.PortraitSide.Left;
        Image activePortrait = onLeft ? leftPortrait : rightPortrait;
        Image otherPortrait = onLeft ? rightPortrait : leftPortrait;

        if (otherPortrait != null) otherPortrait.gameObject.SetActive(false);

        if (activePortrait != null)
        {
            activePortrait.sprite = line.portrait;
            activePortrait.gameObject.SetActive(line.portrait != null);
        }
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        if (continueIndicator != null) continueIndicator.SetActive(false);

        // Using maxVisibleCharacters means rich text tags like <b> or <color> still work
        dialogueTextBox.text = text;
        dialogueTextBox.maxVisibleCharacters = 0;
        dialogueTextBox.ForceMeshUpdate();

        int totalCharacters = dialogueTextBox.textInfo.characterCount;
        float delay = 1f / Mathf.Max(charactersPerSecond, 1f);

        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueTextBox.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);

        dialogueTextBox.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        if (continueIndicator != null) continueIndicator.SetActive(true);
    }

    void Update()
    {
        if (!IsDialogueOpen) return;

        if (Input.GetKeyDown(continueKey) || Input.GetKeyDown(alternateContinueKey))
        {
            ContinueDialogue();
        }
    }
}
