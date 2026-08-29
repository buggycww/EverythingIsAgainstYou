using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public static event Action<Item> OnItemObtained;

    private List<Item> inventory = new List<Item>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep inventory across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Clear static event subscriptions when destroyed
        OnItemObtained = null;

        // Clear instance reference if this is the instance being destroyed
        if (instance == this)
        {
            instance = null;
        }
    }

    public bool HasItem(int ID)
    {
        return inventory.Any(item => item.id == ID);
    }

    public bool HasItem(string itemName)
    {
        return inventory.Any(item => item.Name == itemName);
    }

    public Item GetItem(int ID)
    {
        return inventory.Find(item => item.id == ID);
    }

    public Item GetItem(string itemName)
    {
        return inventory.Find(item => item.Name == itemName);
    }

    public void ObtainItem(Item newItem)
    {
        if (newItem == null)
        {
            Debug.LogWarning("Trying to obtain a null item!");
            return;
        }

        inventory.Add(newItem);
        Debug.Log($"Obtained item: {newItem.Name}");

        OnItemObtained?.Invoke(newItem);
    }

    public void RemoveItem(int ID)
    {
        Item item = GetItem(ID);
        if (item != null)
        {
            inventory.Remove(item);
            Debug.Log($"Removed item: {item.Name}");
        }
    }

    public void RemoveItem(string itemName)
    {
        Item item = GetItem(itemName);
        if (item != null)
        {
            inventory.Remove(item);
            Debug.Log($"Removed item: {item.Name}");
        }
    }

    public List<Item> GetAllItems()
    {
        return new List<Item>(inventory);
    }

    public void ClearInventory()
    {
        inventory.Clear();
        Debug.Log("Inventory cleared");
    }

    public int GetItemCount()
    {
        return inventory.Count;
    }
}

[System.Serializable]
public class Item
{
    public int id;
    public string Name;
    public string Description;
    public Sprite Icon;
}
