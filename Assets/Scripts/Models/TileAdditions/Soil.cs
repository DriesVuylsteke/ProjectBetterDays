using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Soil : TileAddition
{
    public static string AdditionName = "Soil";

    public Soil(Tile tile) : base(tile)
    {
        SetupSoil();
    }

    public Soil() : base()
    {
        SetupSoil();
    }

    private void SetupSoil()
    {
        this.Name = Soil.AdditionName;
        movementCost = 0.75f;
        constructionCost = 1;
    }

    public override TileAddition Clone(Tile t)
    {
        Soil s = new Soil(t);
        return s;
    }

    public override bool Conditions()
    {
        return this.tile.TileType == TileType.Floor &&
            this.tile.addition == null;
    }
}
