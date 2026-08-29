using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPopUpBox;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private Image itemIcon;

    public event Action OnItemPopUpClosed;

    private void Start()
    {
        Inventory.OnItemObtained += ShowItemPopUp;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent calling destroyed object
        Inventory.OnItemObtained -= ShowItemPopUp;
    }

    private void OnDisable()
    {
        // Also unsubscribe when disabled (safety measure)
        Inventory.OnItemObtained -= ShowItemPopUp;
    }

    public void ShowItemPopUp(Item item)
    {
        // Check if the UI elements are still valid
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
        {
            Debug.LogWarning("InventoryUI is destroyed or inactive!");
            return;
        }

        if (item == null)
        {
            Debug.LogWarning("Item is null!");
            return;
        }

        // Check if itemPopUpBox is valid
        if (itemPopUpBox == null || itemPopUpBox.gameObject == null)
        {
            Debug.LogWarning("Item popup box is null or destroyed!");
            return;
        }

        // Set UI elements with null checks
        if (itemName != null && itemName.gameObject != null)
        {
            itemName.text = item.Name;
        }

        if (itemDescription != null && itemDescription.gameObject != null)
        {
            itemDescription.text = item.Description;
        }

        if (itemIcon != null && itemIcon.gameObject != null)
        {
            if (item.Icon != null)
            {
                try
                {
                    itemIcon.sprite = item.Icon;
                }
                catch (MissingReferenceException)
                {
                    Debug.LogWarning("Item icon image was destroyed!");
                    return;
                }
            }
        }

        itemPopUpBox.SetActive(true);
    }

    public void CloseItemPopUp()
    {
        // Check if still valid
        if (this == null || gameObject == null) return;

        if (itemPopUpBox != null && itemPopUpBox.gameObject != null)
        {
            itemPopUpBox.SetActive(false);
        }

        OnItemPopUpClosed?.Invoke();
    }
}