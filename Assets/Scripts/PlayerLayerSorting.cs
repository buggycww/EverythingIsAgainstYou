using System.Collections;
using UnityEngine;

public class PlayerLayerSorting : MonoBehaviour
{
    [SerializeField] private string[] blockableTags;
    [SerializeField] private float checkRange = 0.5f;
    [SerializeField] private int defaultSortingOrder = 2;
    [SerializeField] private Vector3 offset;

    private SpriteRenderer playerSprite;
    private int currentSortingOrder;

    void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite == null)
        {
            Debug.LogError("PlayerLayerSorting requires a SpriteRenderer component!");
            enabled = false;
            return;
        }

        currentSortingOrder = defaultSortingOrder;
        playerSprite.sortingOrder = currentSortingOrder;
        StartCoroutine(CheckForBlockableObjects());
    }

    private IEnumerator CheckForBlockableObjects()
    {
        while (true)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                transform.position + offset,
                checkRange
            );

            Collider2D closestBlockable = GetClosestBlockableCollider(colliders);

            if (closestBlockable != null)
            {
                HandleBlocking(closestBlockable);
            }
            else
            {
                SetSortingOrder(defaultSortingOrder);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private Collider2D GetClosestBlockableCollider(Collider2D[] colliders)
    {
        Collider2D closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            bool hasBlockableTag = false;
            foreach (string tag in blockableTags)
            {
                if (collider.CompareTag(tag))
                {
                    hasBlockableTag = true;
                    break;
                }
            }

            if (!hasBlockableTag) continue;

            SpriteRenderer blockRenderer = collider.GetComponent<SpriteRenderer>();
            if (blockRenderer == null) continue;

            float distance = Vector2.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = collider;
            }
        }

        return closest;
    }

    private void HandleBlocking(Collider2D blockableObject)
    {
        SpriteRenderer blockRenderer = blockableObject.GetComponent<SpriteRenderer>();
        if (blockRenderer == null) return;

        bool sameLayer = playerSprite.sortingLayerID == blockRenderer.sortingLayerID;

        if (!sameLayer)
        {
            SetSortingOrder(defaultSortingOrder);
            return;
        }

        bool playerIsAbove = transform.position.y > blockableObject.transform.position.y;
        bool playerIsBelow = transform.position.y < blockableObject.transform.position.y;

        if (playerIsAbove)
        {
            SetSortingOrder(blockRenderer.sortingOrder - 1);
        }
        else if (playerIsBelow)
        {
            SetSortingOrder(blockRenderer.sortingOrder + 1);
        }
        else
        {
            SetSortingOrder(defaultSortingOrder);
        }
    }

    private void SetSortingOrder(int newOrder)
    {
        if (playerSprite.sortingOrder != newOrder)
        {
            playerSprite.sortingOrder = newOrder;
            currentSortingOrder = newOrder;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + offset, checkRange);
    }
}
