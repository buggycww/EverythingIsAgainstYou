using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public static event Action<Item> OnItemObtained;

    private void Awake()
    {
        instance = this;
    }

    private List<Item> inventory = new List<Item>();

    public bool HasItem(int ID) => inventory.Contains(inventory.Find(i => i.id == ID));

    public void ObtainItem(Item newItem)
    {
        inventory.Add(newItem);
        OnItemObtained?.Invoke(newItem);
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
