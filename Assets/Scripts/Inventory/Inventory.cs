using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private List<int> inventory = new List<int>();

    public bool HasItem(int id) => inventory.Contains(id);

    public void obtainItem(int id) => inventory.Add(id);
}
