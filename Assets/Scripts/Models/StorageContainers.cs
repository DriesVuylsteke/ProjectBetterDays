using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Keeps track of all the storage containers in the world
/// Allows an entity to receive a target storage container based on several parameters
/// 
/// A StorageTileAddition has to register itself here to be able to receive mapping to itself
/// </summary>
public class StorageContainers
{
    // Keep priority etc in here too
    // For now all containers are just in a list
    // TODO: support different lists to do certain calls faster

    protected List<ItemContainer> activeContainers;
    
    public StorageContainers()
    {
        activeContainers = new List<ItemContainer>();
    }

    /// <summary>
    /// Adds a container to the list of active containers
    /// </summary>
    /// <param name="container">container to add</param>
    public void AddContainer(ItemContainer container)
    {
        Debug.Log("Adding a new container");
        activeContainers.Add(container);
        container.TileAdditionRemoved += ContainerRemoved;
    }

    /// <summary>
    /// Retrieves the closest container to the stack that can contain the stack.
    /// </summary>
    /// <param name="stack">The stack to add to the container</param>
    /// <param name="origin">The current location of the stack</param>
    /// <returns></returns>
    public ItemContainer GetContainerToStoreItemStack(ItemStack stack, Tile origin)
    {
        IEnumerable<ItemContainer> res = activeContainers
            .FindAll((c) => c.CanAddItemStackToTileAddition(stack)) // First find all containers that can actually contain the stack.
            .OrderBy((c) => Mathf.Pow(c.tile.X - origin.X, 2) + Mathf.Pow(c.tile.Y - origin.Y, 2)); // Sort them by distance to the origin
        if(res.Count() > 0)
        {
            return res.First();
        }
        else
        {
            return null;
        }
    }

    private void ContainerRemoved(TileAddition addition)
    {
        if(addition is ItemContainer)
        {
            ItemContainer c = (ItemContainer)addition;
            activeContainers.Remove(c);
        } else
        {
            Debug.LogError("The tile addition telling us that it got removed is not an item container, so why are we getting a message from it?");
        }
    }

}
