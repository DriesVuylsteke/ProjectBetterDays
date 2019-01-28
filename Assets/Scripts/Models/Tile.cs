using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Serialization;

[Serializable]
public enum TileType {
	Empty, Floor
}

[Serializable]
public class Tile {

	public int X;
	public int Y;

	Room room;
	public Room Room {
		get {
			return room;
		}
		set {
			Room old = room;
			this.room = value;
			if (old != room) {
				if (OnRoomChanged != null) {
					OnRoomChanged (this);
				}
			}
		}
	}

	// TODO: serialize this
	public TileAddition addition;
	bool borderDefinition = false;

	public World world;

	public float MovementCost {
		get {
			// TODO: add a default cost of 1 for all walkable tiles
			// TODO: don't really like this, need to fix somehow?
			// Perhaps the only walkable tile is always goign to be floor? 
			// Things like dirt can be tile assets?
			// Wonder how I would do plants that way though (A tile asset allowing another tile asset?)
			float defaultCost = (TileType == TileType.Floor) ? 1 : 0;
			if (addition != null)
				defaultCost *= addition.MovementCostMultiplier;

			return defaultCost; // When we implement tile assets, change this cost to take into account what is on the actual tile!
		}
	}
		
	protected TileType type;
	public TileType TileType { 
		get{
			return type;
		}
		protected set{
			TileType old = type;
			type = value;
			if (old != value && OnTileTypeChanged != null) {
				OnTileTypeChanged (this, old, value);
			}
			// changing the type can lead to the border definition changing
			BorderDefinitionPossiblyChanged();
		}
	}

	// Tells the listeners that the tile has changed its type
	public event Action<Tile, TileType, TileType> OnTileTypeChanged;
	public event Action<Tile, TileAddition, TileAddition> OnTileAdditionChanged;
	public event Action<Tile> OnRoomChanged;
	public event Action<Tile> OnBorderDefinitionChanged;

	public Tile(int x, int y, TileType type, World world){
		this.X = x;
		this.Y = y;
		this.type = type;
		this.world = world;

		borderDefinition = DefinesRoomBorder ();
	}

	/// <summary>
	/// Changed the tiletype to floor when possible
	/// </summary>
	public bool SetFloor(){
		if (TileType == TileType.Empty) {
			TileType = TileType.Floor;
			return true;
		}
		return false;
	}

	void BorderDefinitionPossiblyChanged(){
		if (borderDefinition != DefinesRoomBorder ()) {
			borderDefinition = DefinesRoomBorder ();
			if (OnBorderDefinitionChanged != null) {
				OnBorderDefinitionChanged (this);
			}
		}
	}

	// we register this method to the tileAddition.OnBuilt event
	void TileAdditionBuilt(TileAddition addition){
		BorderDefinitionPossiblyChanged ();
		addition.TileAdditionBuilt -= TileAdditionBuilt;
	}

	/// <summary>
	/// Installs the addition on the tile.
	/// </summary>
	/// <returns><c>true</c>, if addition was installed, <c>false</c> otherwise.</returns>
	/// <param name="addition">the addition to install.</param>
	/// <param name="fullyBuilt">If set to <c>true</c> the addition gets fully built</param>
	/// <param name="loading">If set to <c>true</c>, we are loading the world, so conditions don't apply (since the world isn't properly loaded yet).</param>
	public bool InstallAddition(TileAddition addition, bool fullyBuilt = false, bool loading = false){
		if (addition == null || addition.Conditions () || loading) {
			TileAddition oldAddition = this.addition;
			this.addition = addition;
			addition.TileAdditionBuilt += TileAdditionBuilt;

			if (OnTileAdditionChanged != null) {
				OnTileAdditionChanged (this, oldAddition, addition);
			}
			if (fullyBuilt)
				addition.FinishBuilding();
			
			BorderDefinitionPossiblyChanged ();

			return true;
		}
		return false;
	}

	/// <summary>
	/// Defineses the room border.
	/// </summary>
	/// <returns><c>true</c>, if the tile should not be part of a room, <c>false</c> otherwise.</returns>
	public bool DefinesRoomBorder(){
		if (TileType == TileType.Empty) {
			this.Room = null;
			return true;
		}

		if(addition != null){
			if(addition.DefinesRoomBorder()){
				this.Room = null;
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Changed the tiletype to empty when possible
	/// </summary>
	/// <param name="force">If set to <c>true</c> force setting the tile to empty, also destroying the tile additions. Should be used when tile gets destroyed</param>
	public bool SetEmpty(bool force = false){
		if (force) {
			addition = null; // materials are lost too!
		}
		if (addition != null) {
			Debug.Log ("Can't set tile " + X + "-" + Y + " to empty because there is an installed addition");
			return false;
		}
		TileType = TileType.Empty;
		return true;
	}

	public Tile[] GetNeighbours(bool diagOkay = false){
		List<Tile> tiles = new List<Tile> ();

		Tile t = world.GetTileAt (X, Y + 1);
		if (t != null)
			tiles.Add (t);
		t = world.GetTileAt (X, Y - 1);
		if (t != null)
			tiles.Add (t);
		t = world.GetTileAt (X + 1, Y);
		if (t != null)
			tiles.Add (t);
		t = world.GetTileAt (X - 1, Y);
		if (t != null)
			tiles.Add (t);

		if(diagOkay){
			t = world.GetTileAt (X + 1, Y + 1);
			if (t != null)
				tiles.Add (t);
			t = world.GetTileAt (X + 1, Y - 1);
			if (t != null)
				tiles.Add (t);
			t = world.GetTileAt (X - 1, Y + 1);
			if (t != null)
				tiles.Add (t);
			t = world.GetTileAt (X - 1, Y - 1);
			if (t != null)
				tiles.Add (t);
		}

		return tiles.ToArray();
	}

	public bool IsEnterable(){
		if (addition != null) {
			return addition.IsEnterable ();
		}
		return true;
	}

	public override string ToString ()
	{
		return string.Format ("[Tile: X={0}, Y={1}, Type={2}]", X, Y, TileType);
	}

    /// <summary>
    /// Remove the currently installed addition from the tile.
    /// </summary>
    public void RemoveTileAddition()
    {
        TileAddition old = addition;
        addition = null;
        if(old != null)
        {
            old.UnregisterTileAddition();
        }
        OnTileAdditionChanged(this, old, addition);
    }

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// 
	/// 										SERIALIZATION
	/// 
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void WriteXml (XmlWriter writer) {
		// A tile should only be written to the world if it represents a non empty space
		if (TileType == TileType.Empty)
			return;

		writer.WriteStartElement ("Tile");

		writer.WriteAttributeString ("X", X.ToString());
		writer.WriteAttributeString ("Y", Y.ToString());
		writer.WriteAttributeString ("DefinesRoomBorder", this.borderDefinition.ToString());
		writer.WriteAttributeString ("TileType", ((int)this.TileType).ToString());

		// TODO: serialize the tile addition
		if (addition != null)
			addition.WriteXml (writer);

		writer.WriteEndElement ();
	}

	public void ReadXml(XmlReader reader){
		// X and Y is read at world level (to know which tile to load data for)
		borderDefinition = bool.Parse(reader.GetAttribute("DefinesRoomBorder"));
		this.TileType = (TileType) int.Parse ((reader.GetAttribute ("TileType")));

		if (reader.ReadToDescendant ("Addition")) {
			do {
				// We have an addition
				// For now lets juist build the serialization string
				string classPath = reader.GetAttribute ("TypePath");
				string assemblyName = reader.GetAttribute ("Assembly");

				Type targetType = Type.GetType (classPath);
				TileAddition emptyAddition = (TileAddition)Activator.CreateInstance (targetType);
				emptyAddition.ReadXml (reader, this);
				InstallAddition(emptyAddition,false,true);
			} while (reader.ReadToNextSibling ("Addition")); // Should always be one, but how to advance?
		}
	}

	public override bool Equals (object other)
	{
		// Two tiles are equal if they have the same position
		return (other is Tile) && ((Tile)other).X == X && ((Tile)other).Y == Y;
	}
}
