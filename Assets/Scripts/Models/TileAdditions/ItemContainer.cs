using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class for containers of ItemStacks. An example could be a chest holding several stacks of tomatoes.
/// </summary>
public class ItemContainer : TileAddition
{
    public static string AdditionName = "Chest";

    // A list of all itemStacks currently present in the container
    protected List<ItemStack> stacks;
    protected int maxAmountOfStacks;


    public ItemContainer(Tile tile) : base(tile)
    {
        SetupContainer();
    }

    public ItemContainer() : base()
    {
        SetupContainer();
    }

    private void SetupContainer()
    {
        movementCost = 1f;
        constructionCost = 1;
        maxAmountOfStacks = 4;
        this.Name = ItemContainer.AdditionName;

        stacks = new List<ItemStack>();
        TileAdditionBuilt += ContainerBuilt;
    }

    /// <summary>
    /// Registered to when the tile addition is fully built. Registers the container as an available storage container
    /// </summary>
    /// <param name="container"></param>
    private void ContainerBuilt(TileAddition container)
    {
        Debug.Log("Container is built!");
        if(container == this)
        {
            tile.world.storageContainers.AddContainer(this);
        } else
        {
            Debug.LogError("Receiving information about a container that is built that is not this container?");
        }
    }

    public override bool CanAddItemStackToTileAddition(ItemStack stack)
    {
        // This function doesn't manipulate the actual items or stack
        foreach (ItemStack curStack in stacks)
        {
            stack = curStack.MergeResult(stack);
            if (stack == null)
                return true;
        }
        // We haven't successfully merged the stack into an existing stack
        // See if there is still room in the container and add it if there is room
        if (stacks.Count < maxAmountOfStacks)
        {
            return true;
        }
        return false;
    }

    public override ItemStack AddItemStackToTileAddition(ItemStack stack)
    {
        // A container can be full for certain items but not for others if there are still ItemStacks that aren't maxed out. Think of it like a chest in minecraft.

        foreach(ItemStack curStack in stacks)
        {
            stack = curStack.MergeStackInto(stack);
            if (stack == null)
                return null;
        }
        // We haven't successfully merged the stack into an existing stack
        // See if there is still room in the container and add it if there is room
        if(stacks.Count < maxAmountOfStacks)
        {
            stacks.Add(stack);
            return null;
        }
        return stack;
    }

    public override bool CanContainItemOnTile(ItemStack stack)
    {
        return false;
    }

    public override bool Conditions()
    {
        return this.tile.TileType == TileType.Floor && // Can only construct on floor
            this.tile.Addition == null; // And there can not be another tile addition present
    }

    public override TileAddition Clone(Tile t)
    {
        return new ItemContainer(t);
    }
}
