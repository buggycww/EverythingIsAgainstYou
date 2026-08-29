using Cainos.PixelArtTopDown_Basic;
using System.Collections;
using UnityEngine;

public class Rock : BaseInteractable
{
    private Animator animator;

    [SerializeField] private Vector3 offset = Vector3.zero;
    private bool followPlayer;
    private PlayerController playerController;

    [SerializeField] private int moreFertilizerDialogueIndex;
    [SerializeField] private float killPlayerCountdown = 3f;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followPlayer)
        {
            transform.position = playerController.transform.position + offset;
        }
    }

    public override bool IsInteractable()
    {
        if (followPlayer)
        {
            return false;
        }

        return base.IsInteractable();
    }

    public override void OnInteract(GameObject interactor)
    {
        base.OnInteract(interactor);

        playerController = interactor.GetComponent<PlayerController>();
        followPlayer = true;
        HideInteractionPrompt();
        StartCoroutine(RockFall());
    }

    private IEnumerator RockFall()
    {
        yield return new WaitForSeconds(killPlayerCountdown);

        playerController.Stop();
        animator.Play("GrowAndFall");

        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySFX("Rock");

        playerController.Die();
        DialogueSystem.StartDialogue(dialogues[moreFertilizerDialogueIndex]);
    }
}
