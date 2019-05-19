using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an item stack, These stacks can be on the ground or in a tile entity (like a closet for example). An isntance of this object
/// is used to represent items in the game.
/// </summary>
public class ItemStack
{
    protected Queue<Item> stack;
    protected string stackType;
    protected int maxStackSize;

    // Creating an item stack requires an item to determine what type of itemStack it'll be
    public ItemStack(Item item)
    {
        stackType = item.ItemName;
        maxStackSize = item.MaxStackSize;
        stack = new Queue<Item>();
        stack.Enqueue(item);
    }

    // Duplicates an itemstack so it can be manipulated independant of the original instance
    private ItemStack(ItemStack other)
    {
        stackType = other.stackType;
        maxStackSize = other.maxStackSize;
        this.stack = new Queue<Item>();
        foreach(Item item in other.stack)
        {
            stack.Enqueue(item.DuplicateItem());
        }
    }

    public event Action<ItemStack> StackDepleted;
    public event Action<ItemStack> StackCountUpdated;

    /// <summary>
    /// Attempts to merge another stack into this item Stack without actually merging the other into this ItemStack
    /// Useful to see if you were to merge two stacks what the result on the other stack would be.
    /// </summary>
    /// <param name="other">The other item stack to merge into this one, this instance is not manipulated during this method</param>
    /// <returns></returns>
    public ItemStack MergeResult(ItemStack other)
    {
        // The instance we work with is now a copy of the passed instance and will no longer manipulate the original instance
        other = new ItemStack(other);
        if (other.GetStackType() != this.GetStackType())
        {
            // The two stacks contain different items, you can never merge them
            return other;
        }

        int overflow = stack.Count + other.stack.Count - maxStackSize;
        // At this point the stacks are of the same type, this means we can merge them but might still be restricted by the stack size
        if (overflow > 0)
        {
            // The stack would become too big, only merge part of it and return the excess
            int takeN = other.stack.Count - overflow;
            while (takeN > 0)
            {
                other.Take();
                takeN--;
            }
            return other;
        }

        // If there is no overflow just drain the other stack entirely
        for (int i = other.stack.Count; i > 0; i--)
        {
            other.Take();
        }
        return null;
    }

    /// <summary>
    /// Attempts to merge another stack into this item Stack
    /// </summary>
    /// <param name="other">The other item stack to merge into this one</param>
    /// <returns></returns>
    public ItemStack MergeStackInto(ItemStack other)
    {
        if(other.GetStackType() != this.GetStackType())
        {
            // The two stacks contain different items, you can never merge them
            return other;
        }

        int overflow = stack.Count + other.stack.Count - maxStackSize;
        // At this point the stacks are of the same type, this means we can merge them but might still be restricted by the stack size
        if (overflow > 0)
        {
            // The stack would become too big, only merge part of it and return the excess
            int takeN = other.stack.Count - overflow;
            while(takeN > 0)
            {
                AddItem(other.Take());
                takeN--;
            }
            return other;
        }

        // If there is no overflow just drain the other stack entirely
        for(int i = other.stack.Count; i > 0; i--)
        {
            AddItem(other.Take());
        }
        return null;
    }

    /// <summary>
    /// Adds an item to the stack if possible
    /// </summary>
    /// <param name="item">the item to add to the stack</param>
    /// <returns>true if the item was added, false otherwise</returns>
    public bool AddItem(Item item)
    {
        if (!item.ItemName.Equals(this.stackType))
        {
            Debug.LogError("Trying to add an item of type " + item.ItemName + " to a stack of type " + this.stackType);
            return false;
        }

        if(stack.Count < this.maxStackSize)
        {
            stack.Enqueue(item);
            StackCountUpdated?.Invoke(this);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Takes an item from the top of the stack
    /// </summary>
    /// <returns>The next item on the itemstack</returns>
    public Item Take()
    {
        if(stack.Count > 0)
        {
            Item takenItem = stack.Dequeue();
            if(stack.Count == 0)
            {
                if(StackDepleted != null)
                {
                    StackDepleted(this);
                }
            }

            return takenItem;
        } else
        {
            Debug.LogError("ItemStack.Take --- Polling from an empty stack, references to this stack should have been removed when the stack got empty, " +
                "you might have not registered StackDepleted");
            return null;
        }
    }
    
    public float GetCurrentStackSize()
    {
        return stack.Count;
    }

    public string GetStackType()
    {
        return stackType;
    }

    public override string ToString()
    {
        return stackType + ":" + stack.Count;
    }
}
