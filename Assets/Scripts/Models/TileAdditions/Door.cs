using System;
using UnityEngine; 

/// A door is a special case where DoWork doesn't do anything 
/// Opening the door is done through IsEnterable, it enables a flag that opens the door and leaves it open 0.5 seconds
/// After that if IsEnterable hasn't been called it starts closing

/// So door doesn't perform "work" but it still calls doWork so the event gets triggered (doWork just triggers the event if construction is complete)

[Serializable]
public class Door : TileAddition
{
	public static string AdditionName = "Door";

	protected bool entering = false;
	protected float openTime = 1f;
	protected float openTimeStart = 1f;

	Tile tile1,tile2;

	public Door () : base() {
		SetupDoor ();
	}

	public Door (Tile tile) : base (tile)
	{
		SetupDoor ();
		if (tile == null)
			return;
		DetermineRooms ();
	}

	void SetupDoor(){
		this.Name = Door.AdditionName;
		movementCost = 1;
		constructionCost = 1;

		progressSpeed = (1 / 3f);
	}

	void DetermineRooms(){
		// If the passed tile exists, check the neighbours to know which two tiles you are connecting
		Tile north = tile.world.GetTileAt (tile.X, tile.Y + 1);
		Tile east = tile.world.GetTileAt (tile.X + 1, tile.Y);
		Tile south = tile.world.GetTileAt (tile.X, tile.Y - 1);
		Tile west = tile.world.GetTileAt (tile.X - 1, tile.Y);

		// Do we connect the north and south tiles?
		if (north != null && south != null && north.Room != south.Room && north.Room != null && south.Room != null) {
			tile1 = north;
			tile2 = south;
			Debug.Log ("North south orientation door!");
		} else {
			tile1 = east;
			tile2 = west;
			this.Orientation = 90;
			Debug.Log ("East west orientation door!");
		}
	}

	public override void Update (float deltaTime)
	{
		base.Update (deltaTime);
        TransmitRoomValues(deltaTime);

        // The door hasn't been built yet! don't do anything
        if (BuildPercentage < 1)
			return;

		

		if (openTime > 0) {
			openTime -= deltaTime;
			return;
		}

		if (entering) {
			if (Progress < 1) {
				Progress += deltaTime * progressSpeed;
			}
			if (Progress >= 1) {
				Progress = 1;
				entering = false;
				openTime = openTimeStart;
			}
			DoWork (deltaTime);
		} else {
			if (Progress > 0) {
				Progress -= deltaTime * progressSpeed;

				if (Progress <= 0) {
					Progress = 0;
				}


				DoWork (deltaTime);
			}
		}
	}

	/// <summary>
	/// Transmits the values between the two rooms that the door connects
	/// </summary>
	/// <param name="deltaTime">Delta time, time since the last game update</param>
	void TransmitRoomValues(float deltaTime){
        if (tile1 == null)
        {
            Debug.LogError("Door doesn't know about tile 1?");
        }
		Room r1 = tile1.Room;
		Room r2 = tile2.Room;

		r1.MergeRoomValues (r2, BuildPercentage < 1 ? 1 : Progress, deltaTime);
	}

	public override bool Conditions(){
		Tile north = tile.world.GetTileAt (tile.X, tile.Y + 1);
		Tile east = tile.world.GetTileAt (tile.X + 1, tile.Y);
		Tile south = tile.world.GetTileAt (tile.X, tile.Y - 1);
		Tile west = tile.world.GetTileAt (tile.X - 1, tile.Y);

		return tile != null && tile.Addition != null && this.tile.Addition.Name == Wall.AdditionName &&
		(north != null && south != null && north.Room != south.Room && north.Room != null && south.Room != null) ||
		(east != null && west != null && east.Room != west.Room && east.Room != null && west.Room != null);
	}

	// Installs the door in the actual world on the given tile
	public override TileAddition Clone(Tile t){
		Door d = new Door (t);
		return d;
	}

	public override bool DefinesRoomBorder(){
		return true;
	}

	public override bool IsEnterable(){
		entering = true;
		if (Progress == 1) {
			return true;
		}
		return false;
	}

	// Change the name of the object to display it's current state (can be used to render different sprites?)
	public override string GetRenderState(){
		if (Progress >= 1) {
			return Name + "_3";
		} else if (Progress >= 0.66) {
			return Name + "_2";
		} else if (Progress >= 0.33) {
			return Name + "_1";
		}
		return Name;
	}

    public override bool CanContainItemOnTile(ItemStack stack)
    {
        // A door can never contain an item
        // A possibility for later could be to only allow it to have an item while it is open, and than have that item blocking the door
        return false;
    }

    #region Serialization
    protected override void WriteAdditionalXmlProperties (System.Xml.XmlWriter writer)
	{
		base.WriteAdditionalXmlProperties (writer);

		writer.WriteAttributeString ("Entering", entering.ToString ());
		writer.WriteAttributeString("openTime", openTime.ToString());

        // DO NOT DO tile.writeXml !!! this will cause a loop
        // Instead we just keep the tiles coordinates and ask the world for the tile when deserialising
        writer.WriteAttributeString("Tile1X", tile1.X.ToString());
        writer.WriteAttributeString("Tile1Y", tile1.Y.ToString());

        writer.WriteAttributeString("Tile2X", tile2.X.ToString());
        writer.WriteAttributeString("Tile2Y", tile2.Y.ToString());
    }

	protected override void ReadAdditionalXmlProperties (System.Xml.XmlReader reader)
	{
		base.ReadAdditionalXmlProperties (reader);

		entering = bool.Parse (reader.GetAttribute ("Entering"));
		openTime = float.Parse (reader.GetAttribute ("openTime"));

        // Tile 1
        int x = int.Parse(reader.GetAttribute("Tile1X"));
        int y = int.Parse(reader.GetAttribute("Tile1Y"));
        tile1 =tile.world.GetTileAt(x, y);

        // Tile 2
        x = int.Parse(reader.GetAttribute("Tile2X"));
        y = int.Parse(reader.GetAttribute("Tile2Y"));
        tile2 = tile.world.GetTileAt(x, y);
    }
#endregion
}

