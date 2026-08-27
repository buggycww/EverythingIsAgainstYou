using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private bool showGizmos = true;
    private IInteractable currentTarget;

    private void Update()
    {
        var newInteractable = FindInteractable();

        if (newInteractable != null)
        {
            if (currentTarget != null && currentTarget != newInteractable)
            {
                currentTarget.HideInteractionPrompt();
                currentTarget = newInteractable;
                currentTarget.ShowInteractionPrompt();
            }
            else if (currentTarget == null)
            {
                currentTarget = newInteractable;
                currentTarget.ShowInteractionPrompt();
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.HideInteractionPrompt();
                currentTarget = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            currentTarget.OnInteract(gameObject);
        }
    }

    private IInteractable FindInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);

        IInteractable closest = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable != null && interactable.IsInteractable())
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = interactable;
                }
            }
        }

        return closest;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    // Optional: Draw filled circle with transparency
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
        Gizmos.DrawSphere(transform.position, interactionRange);
    }
}