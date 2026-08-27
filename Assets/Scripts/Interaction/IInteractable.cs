using UnityEngine;

public interface IInteractable
{
    public void ShowInteractionPrompt();

    public void HideInteractionPrompt();

    void OnInteract(GameObject interactor);

    bool IsInteractable(); // Can be used for cooldowns, conditions, etc.

    string GetPlayerInteractionDialogue(); // returns player dialogue when seeing interaction
}

public enum InteractType
{
    Talk,
    Push,
    PickUp,
    Poke,
    Pull,
    Open,
    Investigate
}

// Create a separate file for this
public static class InteractTypeExtensions
{
    public static string GetDisplayName(this InteractType type)
    {
        return type switch
        {
            InteractType.Talk => "Talk",
            InteractType.PickUp => "Pick Up",
            InteractType.Open => "Open",
            InteractType.Push => "Push",
            InteractType.Investigate => "Investigate",
            InteractType.Poke => "Poke",
            InteractType.Pull => "Pull",
            _ => type.ToString()
        };
    }
}