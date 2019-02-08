using System;
using UnityEngine;

public class BuildAdditionState : BuildState
{
	TileAddition proto;
    Skills requiredSkill;
	public BuildAdditionState (MouseController mouseController, BuildController controller, TileAddition prototype, Skills requiredSkill) : base(mouseController, controller)
	{
		proto = prototype;
        this.requiredSkill = requiredSkill;
	}


	public override void MouseButton0Down(){
		
	}

	public override void MouseButton0Up(){
		Vector2 v1 = (Vector2) mouseController.mousePressWorldPos;
		Vector2 v2 = mouseController.GetMouseInWorldCoordinates ();

		Vector2 topLeft = Vector2.Min (v1, v2);
		Vector2 bottomRight = Vector2.Max (v1, v2);

		int width = (int)(bottomRight.x - topLeft.x) + 1;
		int height = (int)(bottomRight.y - topLeft.y) + 1;

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Tile tile = world.GetTileAt ((int)(topLeft.x + x), (int)(topLeft.y + y));
				TryBuildAddition (tile);
			}
		}

	}
	public override void MouseButton0Hold(){

	}



	void TryBuildAddition(Tile tile){
		// North-South door
		Room north = world.GetTileAt (tile.X, tile.Y + 1).Room;
		Room south = world.GetTileAt (tile.X, tile.Y - 1).Room;
		Room east = world.GetTileAt (tile.X + 1, tile.Y).Room;
		Room west = world.GetTileAt (tile.X - 1, tile.Y).Room;

		if (proto == null)
			tile.RemoveTileAddition ();
		else {
			TileAddition addition = proto.Clone (tile);
			if (tile.InstallAddition (addition)) { // We can install the addition on the tile, so lets add a job to build it
                switch (requiredSkill)
                {
                    case Skills.Construction:
                        world.Jobs.EnqueueJob(new ConstructionJob(addition));
                        break;
                    case Skills.Planting:
                        world.Jobs.EnqueueJob(new PlantJob(addition));
                        break;
                    case Skills.Harvesting:
                        world.Jobs.EnqueueJob(new HarvestJob(addition));
                        break;
                    default:
                        Debug.LogError("Don't know how to create a job for this skill");
                        break;
                }
				
			} else {
				Debug.Log("Can't install " + addition.Name + " at the tile" + addition.tile.ToString());
			}
		}
	}

	public override string GetTooltipSpriteName (){
		return proto == null ? null : proto.GetRenderState();
	}
}

