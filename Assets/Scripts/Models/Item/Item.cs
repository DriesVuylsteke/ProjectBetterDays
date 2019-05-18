using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class for items, items themselves never receive updates, however the stack of items might receive an update and edit the item accordingly.
/// An instance of this class is used to identify what type of item an itemstack consists off. For example one "instance" of a tomato
/// Rotting etc should all happen inside this class, however the only class that should be able to create these instances is the ItemFactory.
/// </summary>
public class Item
{
    public string ItemName { get; set; }
    public int MaxStackSize { get; set; } // The maximum amount of items that can be present in the "stack" at once

    /// <summary>
    /// Duplicates the current item instance, in the future this means we get an instance that is "decayed" as much for example
    /// This means we can work on the duplicated instance without affecting the original
    /// </summary>
    /// <returns>A duplicate instance independant from the original</returns>
    public Item DuplicateItem()
    {
        return new Item()
        {
            ItemName = this.ItemName,
            MaxStackSize = this.MaxStackSize
        };
    }
}
