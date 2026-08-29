using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

public class Escape : BaseInteractable
{
    [SerializeField] private GameObject playerEscapeAnimObject;
    [SerializeField] private GameObject hidePlayer;
    [SerializeField] private Animator moveEscapeCoverAnimator;

    private bool hasMovedCover;
    private bool canEscape;
    private bool hasEscaped;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hidePlayer.SetActive(false);
        playerEscapeAnimObject.SetActive(false);
    }

    public override bool IsInteractable()
    {
        if (hasMovedCover && !canEscape)
        {
            return false;
        }

        if (hasEscaped)
        {
            return false;
        }

        return base.IsInteractable();
    }

    public override string GetInteractionPrompt()
    {
        return !hasMovedCover ? "[E] Investigate" : "[E] Escape";
    }

    public override void OnInteract(GameObject interactor)
    {
        base.OnInteract(interactor);

        HideInteractionPrompt();

        if (!hasMovedCover)
        {
            hasMovedCover = true;
            moveEscapeCoverAnimator.Play("MoveCoverAside");
        }
        else
        {
            hasEscaped = true;
            var playerController = interactor.GetComponent<PlayerController>();
            playerController.MoveToLocation(playerEscapeAnimObject.transform.position);
            playerController.OnMoveToLocationComplete += OnPlayerFinishedMoving;
        }
    }

    public void OnCoverFinishedMoving()
    {
        canEscape = true;
    }

    public void OnPlayerFinishedMoving()
    {
        hidePlayer.SetActive(true);
        playerEscapeAnimObject.SetActive(true);
    }
}
