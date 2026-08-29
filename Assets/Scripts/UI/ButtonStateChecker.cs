using UnityEngine;
using UnityEngine.UI;

public class ButtonStateChecker : MonoBehaviour
{
    private Button targetButton;
    private bool wasHovered = false;
    private bool wasPressed = false;

    private void Start()
    {
        targetButton = GetComponent<Button>();
    }

    private void Update()
    {
        if (targetButton == null) return;

        // Check current states
        bool isHovered = targetButton.IsHighlighted();
        bool isPressed = targetButton.IsPressed();

        // Play hover sound only when starting to hover (was not hovered, now is)
        if (isHovered && !wasHovered)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("Hover");
        }

        // Play press sound only when starting to press (was not pressed, now is)
        if (isPressed && !wasPressed)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX("Click");
        }

        // Update previous states
        wasHovered = isHovered;
        wasPressed = isPressed;
    }
}