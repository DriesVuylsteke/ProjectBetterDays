using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;

/// For sealialization we only save the rooms personal properties and the coordinates of a single tile so we can then when the world is recreated
/// Override these properties in the newly (dynamically) created rooms based. The coordinates will let us know which room we need to override world.getTileAt(x,y).room.properties


/// <summary>
/// A collection of tiles that hold certain values (like air levels, temperature...)
/// </summary>
public class Room{
	public List<Tile> tiles;
	public List<Tile> roomEdges;

	public Dictionary<string,List<TileAddition>> additions;

	public int MinX { get; protected set; }
	public int MaxX { get; protected set; }
	public int MinY { get; protected set; }
	public int MaxY { get; protected set; }

	public float Temperature { get; protected set; }
	public float OxygenLevel { get; protected set; }
    bool connectsToSpace = false;


	public event Action<Room> OnRoomDoorAdded;
	public event Action<Room> OnRoomTilesDepleted;

	[NonSerialized]
	protected World world;
	public World GetWorld() {
		return world;
	}

	// The coordinates of the starting tile for this room (used for serialization)
	int X;
	int Y;

	public Room(World world, Tile startTile){
		tiles = new List<Tile> ();
		roomEdges = new List<Tile> ();
		additions = new Dictionary<string, List<TileAddition>> ();

		// Use floodfill to determine where the room goes, for this we look at the existing room and whether or not the tile can even go into a room.
		FloodFillRoom(startTile);

		X = startTile.X;
		Y = startTile.Y;

		this.world = world;
	}

	public void Update(float deltaTime){
		// Since tiles don't get an update, the room will update the tile additions it is aware of.
		// This will allow us to "freeze" rooms later on (with some sort of tech?)
		foreach (string additionName in additions.Keys) {
			foreach (TileAddition addition in additions[additionName]) {
				addition.Update (deltaTime);
			}
		}
	}

	void FloodFillRoom(Tile start){
		// TODO: look at the rooms that existed before, can do some optimizations here
		// TODO: merge the existing rooms statistics into this one depending on its sizes (breaking down a wall increase the size, building one decrease it for example)
		List<Room> existingRooms = new List<Room> ();
		List<Tile> visited = new List<Tile> ();
		// Candidates that are NOT an edge to the room
		Queue<Tile> candidates = new Queue<Tile> ();
		candidates.Enqueue (start);
		visited.Add (start);

		if (start.DefinesRoomBorder ()) {
			// There can be no room starting from a tile that is an edge
			return;
		}

		// TODO: take in the values of the existing room

		while (candidates.Count > 0) {
			Tile cur = candidates.Dequeue ();
			// Add the tile and enqueue its neighbours if they aren't part of the room already.
			AddTile(cur);

			Tile[] nbs = cur.GetNeighbours(false);
			foreach (Tile nb in nbs) {
				if (!visited.Contains (nb)) {
					if (nb != null && nb.Room != this) {
						if (nb.DefinesRoomBorder () == false && !tiles.Contains (nb)) {
							candidates.Enqueue (nb);
						} else {
							AddTile (nb);
						}
					}
					visited.Add (nb);
				}
			}
		}
	}

	/// <summary>
	/// Adds the tile to the room.
	/// </summary>
	/// <returns><c>true</c>, if tile was added, <c>false</c> otherwise.</returns>
	/// <param name="tile">Tile.</param>
	public bool AddTile(Tile tile){
		bool added = false;

		if (tile.DefinesRoomBorder ()) {
			roomEdges.Add (tile);
            if (tile.TileType == TileType.Empty) // Part of the room border is space
            {
                connectsToSpace = true;
            }
			added = true;
		}
		else if (tile.TileType == TileType.Floor && !tiles.Contains (tile)) {
			tiles.Add (tile);

			// Take in the temperature and oxygen of the previous room
			float curTiles = tiles.Count;
			if (tile.Room != null) {
				Temperature = (Temperature * curTiles + tile.Room.Temperature) / (curTiles + 1);
				OxygenLevel = (OxygenLevel * curTiles + tile.Room.OxygenLevel) / (curTiles + 1);
			} else { // No levels present here, lower the levels of the room
				Temperature = (Temperature * curTiles) / (curTiles + 1);
				OxygenLevel = (OxygenLevel * curTiles) / (curTiles + 1);
			}
			tile.Room = this;
			// Update outer bounds
			if (tile.X < MinX)
				MinX = tile.X;
			if (tile.X > MaxX)
				MaxX = tile.X;
			if (tile.Y < MinY)
				MinY = tile.Y;
			if (tile.Y > MaxY)
				MaxY = tile.Y;
			added = true;
		}

		if (added) {
			// Also add the addition that is on the tile
			TileAdditionChanged(tile, null, tile.addition);
			tile.OnTileTypeChanged += TileChanged;
			tile.OnRoomChanged += UnregisterTile;
			tile.OnTileAdditionChanged += TileAdditionChanged;
		}
		return added;
	}

	void TileAdditionChanged(Tile tile, TileAddition old, TileAddition addition){
		if (old != null) {
			// Need to remove the old entry
			if (additions.ContainsKey (old.Name)) {
				additions [old.Name].Remove (old);
			} else {
				Debug.LogError ("An addition is being changed, but the old entry wasn't in the dictionary!");
			}
		}
		if (addition == null)
			return;
		
		if (!additions.ContainsKey (addition.Name)) {
			// There is no list yet for this type of tile addition
			additions[addition.Name] = new List<TileAddition>();
		}
		additions [addition.Name].Add (addition);

		// Change the worlds graph if a door was added
		if (addition.Name == Door.AdditionName) {
			if (OnRoomDoorAdded != null) {
				OnRoomDoorAdded (this);
			}
		}
	}

	void TileChanged(Tile tile, TileType oldType, TileType newType){
		// Check if the room is still enclosed
		// Update the list of walls
		switch (oldType) {
		case TileType.Floor:
			UnregisterTile (tile);
			break;
		}
	}

	void UnregisterTile(Tile t){
		tiles.Remove(t);
		t.OnRoomChanged -= this.UnregisterTile;
		t.OnTileTypeChanged -= this.TileChanged;
        t.OnTileAdditionChanged -= this.TileAdditionChanged;

		if (tiles.Count == 0) {
            
			// Let everyone know you don't contain any tiles anymore.
			if (OnRoomTilesDepleted != null) {
                OnRoomTilesDepleted (this);

                // Should probably also clean up the room so noone has references to it.
                OnRoomDoorAdded = null;
                OnRoomTilesDepleted = null;
			}
		}
	}

	/// <summary>
	/// Adds the heat to the room
	/// </summary>
	/// <param name="amount">Amount.</param>
	/// <param name="maxTemp">Max temp the outside source can generate</param>
	public void AddHeat(float amount, float maxTemp){
        if (connectsToSpace)
        {
            Temperature = 0;
            return;
        }

		float tilesAmount = tiles.Count;
		// The heat is added distributed across the entire room, this means that
		// Adding 20 heat to a room with 20 tiles will up the temperature with 1
		// While adding 20 heat to a room with 100 tiles will only upp the temperature with 0.2 degrees
		Temperature += amount / tilesAmount;
		if (Temperature > maxTemp)
			Temperature = maxTemp;
	}

	/// <summary>
	/// Adds the oxygen to the room
	/// </summary>
	/// <param name="amount">Amount.</param>
	/// <param name="maxOxygen">Max oxygen that the outside source can generate</param>
	public void AddOxygen(float amount, float maxOxygen){
        if (connectsToSpace)
        {
            OxygenLevel = 0;
            return;
        }
		float tilesAmount = tiles.Count;
		OxygenLevel += amount / tilesAmount;
		if (OxygenLevel > maxOxygen)
			OxygenLevel = maxOxygen;
	}

	/// <summary>
	/// Merges the rooms values with another room based on how much they are connected, this function needs to be called from a place that is aware of the game speed
	/// </summary>
	/// <param name="other">the other room</param>
	/// <param name="connection">1 == full connection (fully open door?), 0 == no connection</param> 
	/// <param name="deltaTime">The current game speed, the time since the last game update</param>
	public void MergeRoomValues(Room other, float connection, float deltaTime){
		float otherTemp = other.Temperature;
		float otherOxygen = other.OxygenLevel;

		float oxygenDifference = OxygenLevel - otherOxygen;

		// Add this amount * openness clamped to 1 per second
		AddOxygen (
			Mathf.Clamp (-oxygenDifference * connection, 0, 1) * deltaTime,
			Mathf.Infinity);
		other.AddOxygen(
			Mathf.Clamp (oxygenDifference * connection, 0, 1) * deltaTime,
			Mathf.Infinity);
	}


	public void WriteXml (XmlWriter writer) {
		writer.WriteStartElement ("Room");

		writer.WriteAttributeString ("X", X.ToString());
		writer.WriteAttributeString ("Y", Y.ToString());
		writer.WriteAttributeString ("Temperature", Temperature.ToString());
		writer.WriteAttributeString ("Oxygen", OxygenLevel.ToString());

		writer.WriteEndElement ();
	}

    public void ReadXml(XmlReader reader)
    {
        // X and Y is read at world level (to know which tile to load data for)
        Temperature = float.Parse(reader.GetAttribute("Temperature"));
        OxygenLevel = float.Parse(reader.GetAttribute("Oxygen"));
    }
}
