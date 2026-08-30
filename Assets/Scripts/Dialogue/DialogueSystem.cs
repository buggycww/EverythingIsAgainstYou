using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    // Public static Instance for access from other scripts
    public static DialogueSystem Instance { get; private set; }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image portrait;
    [SerializeField] private Button[] options;

    [SerializeField] private Animator animator;

    [Header("Dialogue Settings")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private AudioClip defaultVoice;

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

    private AudioClip currentAudioClip;
    private float currentPitch;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dialogueQueue = new Queue<string>();
        autoProgressLines = new Queue<bool>();

        // Get animator if not assigned
        if (animator == null && dialoguePanel != null)
        {
            animator = dialoguePanel.GetComponent<Animator>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // Clean up events
        OnDialogueComplete = null;
        OnOption1Clicked = null;
        OnOption2Clicked = null;
    }

    public static void StartDialogue(Dialogue dialogue)
    {
        // Null check for Instance
        if (Instance == null)
        {
            Debug.LogError("DialogueSystem Instance is null! Cannot start dialogue.");
            return;
        }

        if (dialogue == null)
        {
            Debug.LogError("Dialogue is null! Cannot start dialogue.");
            return;
        }

        Instance.hasOptions = false;

        foreach (var option in Instance.options)
        {
            if (option != null)
                option.gameObject.SetActive(false);
        }

        if (Instance.animator != null)
            Instance.animator.Play("SlideIn");
        else
            Debug.LogWarning("DialogueSystem animator is null!");

        Instance.dialogueQueue.Clear();
        Instance.autoProgressLines.Clear();

        for (int i = 0; i < dialogue.dialogueLines.Length; i++)
        {
            Instance.dialogueQueue.Enqueue(dialogue.dialogueLines[i]);
            Instance.autoProgressLines.Enqueue(dialogue.autoProgressLines[i]);
        }

        if (dialogue.options.Length > 0)
        {
            Instance.hasOptions = true;
            for (int i = 0; i < dialogue.options.Length; i++)
            {
                if (i < Instance.options.Length && Instance.options[i] != null)
                {
                    var textComponent = Instance.options[i].GetComponentInChildren<TMP_Text>();
                    if (textComponent != null)
                        textComponent.text = dialogue.options[i];
                    Instance.options[i].gameObject.SetActive(true);
                }
            }
        }

        if (Instance.portrait != null)
        {
            Instance.portrait.sprite = dialogue.Portrait;
            Instance.portrait.color = dialogue.PortraitTint;
        }

        Instance.typeSpeed = dialogue.typingSpeed;

        if (dialogue.voiceSound != null)
        {
            Instance.currentAudioClip = dialogue.voiceSound;
        }
        else
        {
            Instance.currentAudioClip = Instance.defaultVoice;
        }

        Instance.currentPitch = dialogue.voicePitch;
        Instance.ShowNextLine();

        Instance.isDialogueActive = true;
        Instance.isSkipping = false;
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
            if (dialogueText != null)
                dialogueText.text = currentLineText;
            isSkipping = false;
        }

        if (dialogueQueue.Count == 0 && !hasOptions)
        {
            if (animator != null)
                animator.Play("SlideOut");
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

        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char c in text)
        {
            // Check if space was pressed to skip
            if (isSkipping)
            {
                // Skip to the end of the line
                if (dialogueText != null)
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

            if (currentAudioClip != null)
            {
                SoundManager.Instance.PlayVoice(currentAudioClip, currentPitch);
            }

            if (dialogueText != null)
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
                isSkipping = true;
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    public void Option1Clicked()
    {
        OnOption1Clicked?.Invoke();
        if (animator != null)
            animator.Play("SlideOut");
        isDialogueActive = false;

        // Stop any ongoing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        OnDialogueComplete?.Invoke();
        OnOption1Clicked = null;
        OnOption2Clicked = null;
    }

    public void Option2Clicked()
    {
        OnOption2Clicked?.Invoke();
        if (animator != null)
            animator.Play("SlideOut");
        isDialogueActive = false;

        // Stop any ongoing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        OnDialogueComplete?.Invoke();
        OnOption1Clicked = null;
        OnOption2Clicked = null;
    }
}