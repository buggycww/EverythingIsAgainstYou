using UnityEngine;
using Cainos.PixelArtTopDown_Basic;

public class WickedWind : BaseInteractable
{
    private bool hasPlayerTouchedWind;
    [SerializeField] private int killedPlayerIndex;
    [SerializeField] private int clearedWindIndex;

    public void KilledPlayer()
    {
        DialogueSystem.StartDialogue(dialogues[killedPlayerIndex]);
    }

    public void PlayerClearedWind()
    {
        DialogueSystem.StartDialogue(dialogues[clearedWindIndex]);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            hasPlayerTouchedWind = true;
        }
    }
}
