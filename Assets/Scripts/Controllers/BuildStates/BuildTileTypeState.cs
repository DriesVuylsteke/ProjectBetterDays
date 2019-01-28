using System;
using UnityEngine;

public class BuildTileTypeState : BuildState
{
	TileType targetType;
	public BuildTileTypeState (MouseController mouseController, BuildController controller, TileType type) : base(mouseController, controller)
	{
		targetType = type;
	}


	public override void MouseButton0Down(){
		
	}

	public override void MouseButton0Up(){
		Vector2 v1 = (Vector2) mouseController.mousePressWorldPos;
		Vector2 v2 = mouseController.GetMouseInWorldCoordinates ();

		Vector2 topLeft = Vector2.Min (v1, v2);
		Vector2 bottomRight = Vector2.Max (v1, v2);

		BuildFloor (topLeft, bottomRight);
	}

	public override void MouseButton0Hold(){

	}

	void BuildFloor(Vector2 topLeft, Vector2 bottomRight){
		int width = (int)(bottomRight.x - topLeft.x) + 1;
		int height = (int)(bottomRight.y - topLeft.y) + 1;

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Tile t = world.GetTileAt ((int)(topLeft.x + x), (int)(topLeft.y + y));
				switch (targetType) {
				case TileType.Empty:
					t.SetEmpty ();
					break;
				case TileType.Floor:
					t.SetFloor ();
					break;
				}
			}
		}
	}

	public override string GetTooltipSpriteName (){
		switch (targetType) {
		case TileType.Floor:
			return "Floor";
		default:
			return null;
		}
	}
}

