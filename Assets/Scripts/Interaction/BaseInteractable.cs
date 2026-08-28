using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] protected Dialogue[] dialogues;
    protected bool hasInteracted;
    [SerializeField] private bool canInteractAgain = false;
    [SerializeField] private InteractType interactType;
    [SerializeField] protected TextMeshProUGUI interactText;
    [SerializeField] private GameObject interactionPrompt;

    protected bool isShowingText = false;

    public virtual void Awake()
    {
        DialogueSystem.OnDialogueComplete += HandleDialogueComplete;
        interactionPrompt.GetComponent<Button>().onClick.AddListener(ShowInteractionText);
        HideInteractionPrompt();
    }

    #region IInteractable
    public virtual bool IsInteractable()
    {
        return canInteractAgain ? true : !hasInteracted;
    }

    public virtual void OnInteract(GameObject interactor)
    {
        if (!isShowingText)
            return;

        hasInteracted = true;
    }

    public void ShowInteractionPrompt()
    {
        if (!interactionPrompt.gameObject.activeSelf)
        {
            interactionPrompt.SetActive(true);
        }
    }

    public void HideInteractionPrompt()
    {
        Debug.Log("Hide prompt");
        isShowingText = false; // Reset flag
        interactText.text = "";
        interactionPrompt.SetActive(false);
    }
    #endregion

    #region Interaction UI
    public virtual string GetInteractionPrompt()
    {
        if (hasInteracted && !canInteractAgain)
        {
            return "";
        }

        string actionText = interactType.GetDisplayName();
        return hasInteracted ? $"[E] {actionText} Again" : $"[E] {actionText}";
    }

    public void ShowInteractionText()
    {
        Debug.Log("show interaction text");
        isShowingText = true;
        interactionPrompt.SetActive(false);
        interactText.text = GetInteractionPrompt();
    }
    #endregion

    #region Dialogue
    public string GetPlayerInteractionDialogue() => dialogues[0].playerInteractionDialogueLines[0];

    public virtual void HandleDialogueComplete()
    {

    }
    #endregion
}