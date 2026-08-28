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
        foreach (var option in instance.options)
        {
            option.gameObject.SetActive(false);
        }

        instance.animator.Play("SlideIn");

        instance.dialogueQueue.Clear();

        for (int i = 0; i < dialogue.dialogueLines.Length; i++)
        {
            instance.dialogueQueue.Enqueue(dialogue.dialogueLines[i]);
            instance.autoProgressLines.Enqueue(dialogue.autoProgressLines[i]);
        }

        if (dialogue.options.Length > 0)
        {
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
    }

    public void ShowNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            instance.animator.Play("SlideOut");
            isDialogueActive = false;
            OnDialogueComplete?.Invoke();
            return;
        }

        var line = dialogueQueue.Dequeue();
        bool autoProgress = autoProgressLines.Dequeue();
        StartCoroutine(TypeText(line, autoProgress));
    }

    private IEnumerator TypeText(string text, bool autoProgress)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;

        if (autoProgress)
        {
            yield return new WaitForSeconds(1.5f);
            ShowNextLine();
        }
    }

    private void Update()
    {
        if (isDialogueActive && !isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextLine();
        }
    }

    public void Option1Clicked()
    {
        OnOption1Clicked?.Invoke();
        instance.animator.Play("SlideOut");
        isDialogueActive = false;
        OnDialogueComplete?.Invoke();
    }

    public void Option2Clicked()
    {
        OnOption2Clicked?.Invoke();
        instance.animator.Play("SlideOut");
        isDialogueActive = false;
        OnDialogueComplete?.Invoke();
    }
}