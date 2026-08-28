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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Inventory.OnItemObtained += ShowItemPopUp;  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowItemPopUp(Item item)
    {
        this.itemName.text = item.Name;
        this.itemName.text = item.Description;
        this.itemIcon.sprite = item.Icon;
        itemPopUpBox.SetActive(true);
    }

    public void CLoseItemPopUp()
    {
        itemPopUpBox.SetActive(false);
        OnItemPopUpClosed?.Invoke();
    }
}
