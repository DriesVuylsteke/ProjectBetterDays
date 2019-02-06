using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tomato : Plant
{
    public static string AdditionName = "Tomato";

    // TODO: read this from config files
    private int minYield, maxYield;

    public Tomato(Tile tile) : base(tile)
    {
        SetupTomato();
    }

    public Tomato() : base()
    {
        SetupTomato();
    }

    private void SetupTomato()
    {
        this.Name = Tomato.AdditionName;
        constructionCost = 0.5f;
        progressSpeed = 0.2f;
        progressStates = 4;
    }

    public override TileAddition Clone(Tile t)
    {
        Tomato s = new Tomato(t);
        return s;
    }

    public override bool Conditions()
    {
        return base.Conditions();
    }

    public override string GetRenderState()
    {
        return AdditionName + "_" + GetProgressState();
    }

    protected override void PlantHarvested(Job job)
    {
        Debug.Log("Harvested tomato plant");

        // Install a new ungrown tomato plant, a character will have to construct (read plant) it again
        // Don't remove and replace the tile, reset the values to default!
        this.ResetPlantValues();

        // Create an itemStack with the yield of the harvest and put it
        float stackSize = UnityEngine.Random.Range(ItemValues.tomato_min_yield, ItemValues.tomato_max_yield);
        if(stackSize > 0)
        {
            // We have a yield! (probably should always have this though)
            ItemStack tomatoStack = new ItemStack(ItemFactory.GetTomato());
            stackSize--;

            // Now add tomato's so the full yield is on the tile
            while(stackSize > 0)
            {
                tomatoStack.AddItem(ItemFactory.GetTomato());
                stackSize--;
            }

            // now that we have the yield inside of a stack, try add this stack to the tile of the tomato plant.
            this.tile.AddItemStackToTile(tomatoStack);

            // TODO: try add the stack to neighbours? for now if the tile is occupied the excess tomato's are lost

        }
        
    }

    protected override void HarvestCancelled(Job job)
    {
        throw new NotImplementedException();
    }
}
