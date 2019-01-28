﻿using System;

[Serializable]
public class Wall : TileAddition
{
	public static string AdditionName = "Wall";

	public Wall() : base() {
		SetupWall ();
	}

	public Wall (Tile tile) : base (tile)
	{
		SetupWall ();
	}

	void SetupWall(){
		this.Name = Wall.AdditionName;
		movementCost = 0;
		constructionCost = 2;
	}

	public override bool Conditions(){
		return this.tile.TileType == TileType.Floor &&
			this.tile.addition == null;
	}

	public override TileAddition Clone(Tile t){
		Wall w = new Wall (t);
		return w;
	}

	public override bool DefinesRoomBorder(){
		return BuildPercentage == 1;
	}
}

