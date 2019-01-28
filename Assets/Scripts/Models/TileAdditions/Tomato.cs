using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tomato : Plant
{
    public static string AdditionName = "Tomato";

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
        Debug.Log("Harvesting plant");
        tile.RemoveTileAddition();
        tile.InstallAddition(new Tomato(tile), true);
        // also give the player the plants fruits perhaps?
        // Probably should have a setter for a "job reward" that gets passed to the entity that complets the job
        // Worries for later, for now lets just have the player come to the plant and harvest it
    }

    protected override void HarvestCancelled(Job job)
    {
        throw new NotImplementedException();
    }
}
