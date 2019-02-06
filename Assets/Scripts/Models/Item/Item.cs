using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class for items, items themselves never receive updates, however the stack of items might receive an update and edit the item accordingly
/// </summary>
public class Item
{
    public string ItemName { get; set; }
    public int MaxStackSize { get; set; } // The maximum
}
