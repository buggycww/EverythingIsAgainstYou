using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

public class Lever : BaseInteractable
{
    [SerializeField] private Sprite pulledIcon;
    [SerializeField] private Gate gate;
    [SerializeField] private GameObject[] traps;
    [SerializeField] private int killPlayerIndex;
    [SerializeField] private int clearedIndex;

    public override void OnInteract(GameObject interactor)
    {
        SoundManager.Instance.PlaySFX("Lever");

        base.OnInteract(interactor);

        HideInteractionPrompt();
        ActivateTrap(interactor);
    }

    private void ActivateTrap(GameObject player)
    {
        var playerController = player.GetComponent<PlayerController>();
        playerController.Stop();
        playerController.enabled = false;
        transform.GetComponent<SpriteRenderer>().sprite = pulledIcon;
        int trapIndex = Random.Range(0, traps.Length);
        traps[trapIndex].transform.position = player.transform.position;
        traps[trapIndex].SetActive(true);
        gate.Open();
    }

    public void KilledPlayer()
    {
        DialogueSystem.StartDialogue(dialogues[killPlayerIndex]);
    }
    public void PuzzleCleared()
    {
        DialogueSystem.StartDialogue(dialogues[clearedIndex]);
    }
}
