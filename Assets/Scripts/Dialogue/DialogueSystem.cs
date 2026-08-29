using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image portrait;
    [SerializeField] private Button[] options;

    private Animator animator;

    [Header("Dialogue Settings")]
    [SerializeField] private float typeSpeed = 0.05f;

    private static DialogueSystem instance;
    private Queue<string> dialogueQueue;
    private Queue<bool> autoProgressLines;
    private bool isDialogueActive;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private string currentLineText;
    private bool isSkipping;
    private bool hasOptions;

    public static event Action OnDialogueComplete;
    public static event Action OnOption1Clicked;
    public static event Action OnOption2Clicked;

    private void Awake()
    {
        instance = this;
        animator = dialoguePanel.GetComponent<Animator>();
        dialogueQueue = new Queue<string>();
        autoProgressLines = new Queue<bool>();
    }

    public static void StartDialogue(Dialogue dialogue)
    {
        instance.hasOptions = false;

        foreach (var option in instance.options)
        {
            option.gameObject.SetActive(false);
        }

        instance.animator.Play("SlideIn");

        instance.dialogueQueue.Clear();
        instance.autoProgressLines.Clear();

        for (int i = 0; i < dialogue.dialogueLines.Length; i++)
        {
            instance.dialogueQueue.Enqueue(dialogue.dialogueLines[i]);
            instance.autoProgressLines.Enqueue(dialogue.autoProgressLines[i]);
        }

        if (dialogue.options.Length > 0)
        {
            instance.hasOptions = true;
            for (int i = 0; i < dialogue.options.Length; i++)
            {
                instance.options[i].transform.GetComponentInChildren<TMP_Text>().text = dialogue.options[i];
                instance.options[i].gameObject.SetActive(true);
            }
        }

        instance.portrait.sprite = dialogue.Portrait;
        instance.portrait.color = dialogue.PortraitTint;
        instance.typeSpeed = dialogue.typingSpeed;
        instance.ShowNextLine();

        instance.isDialogueActive = true;
        instance.isSkipping = false;
    }

    public void ShowNextLine()
    {
        // Stop any ongoing typing coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // If there was a skipped line, make sure we show the full text
        if (isSkipping && !string.IsNullOrEmpty(currentLineText))
        {
            dialogueText.text = currentLineText;
            isSkipping = false;
        }

        if (dialogueQueue.Count == 0 && !hasOptions)
        {
            instance.animator.Play("SlideOut");
            isDialogueActive = false;
            OnDialogueComplete?.Invoke();
            return;
        }
        else if (dialogueQueue.Count == 0 && hasOptions)
        {
            return;
        }

        var line = dialogueQueue.Dequeue();
        bool autoProgress = autoProgressLines.Dequeue();
        typingCoroutine = StartCoroutine(TypeText(line, autoProgress));
    }

    private IEnumerator TypeText(string text, bool autoProgress)
    {
        isTyping = true;
        isSkipping = false;
        currentLineText = text;
        dialogueText.text = "";

        foreach (char c in text)
        {
            // Check if space was pressed to skip
            if (isSkipping)
            {
                // Skip to the end of the line
                dialogueText.text = text;
                isTyping = false;
                isSkipping = false;
                currentLineText = "";

                // If auto-progress, wait, then continue
                if (autoProgress)
                {
                    yield return new WaitForSeconds(1.5f);
                    ShowNextLine();
                }
                yield break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        currentLineText = "";

        if (autoProgress)
        {
            yield return new WaitForSeconds(1.5f);
            ShowNextLine();
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        // Skip typing with Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Skip the current typing animation
                isSkipping = true;
            }
            else
            {
                // Go to next line if not typing
                ShowNextLine();
            }
        }
    }

    public void Option1Clicked()
    {
        OnOption1Clicked?.Invoke();
        instance.animator.Play("SlideOut");
        isDialogueActive = false;

        // Stop any ongoing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        OnDialogueComplete?.Invoke();
    }

    public void Option2Clicked()
    {
        OnOption2Clicked?.Invoke();
        instance.animator.Play("SlideOut");
        isDialogueActive = false;

        // Stop any ongoing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        OnDialogueComplete?.Invoke();
    }
}