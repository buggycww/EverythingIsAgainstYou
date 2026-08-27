using UnityEngine;

public class NPC : BaseInteractable
{
    public bool isDead;

    public override bool IsInteractable()
    {
        if (isDead)
        {
            return false;
        }

        return base.IsInteractable();
    }

    public virtual void Run()
    {

    }

    public virtual void Die()
    {
        this.HideInteractionPrompt();
    }
}
