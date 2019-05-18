using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable{
	Tile[,] tiles;

	public int Width;
	public int Height;

	protected List<Room> rooms;
	public List<Room> GetRooms() { return rooms; }

    public StorageContainers storageContainers { get; protected set; }

	protected List<Character> characters;
	public List<Character> GetCharacters() { return characters; }
    public event Action<Character> OnCharacterAdded;

	public JobQueue Jobs { get; protected set; }

	protected WorldGraph graph;
	public WorldGraph Graph {
		get{
			if (graphOutDated) {
				graph = new WorldGraph (rooms, this);
				graphOutDated = false;
			}
			return graph;
		}
	}
	protected bool graphOutDated = false;

	/// <summary>
	/// Default constructor, only used for serialization purpose
	/// </summary>
	public World(){ }

	/// <summary>
	/// Initializes a new instance of the <see cref="World"/> class.
	/// </summary>
	/// <param name="width">The width of the world</param>
	/// <param name="height">The height of the world</param>
	public World(int width, int height){
		SetupWorld (width, height);
	}

	void SetupWorld(int width, int height){
		this.Width = width;
		this.Height = height;

		rooms = new List<Room> ();
		characters = new List<Character>();
		Jobs = new JobQueue (this);
        storageContainers = new StorageContainers();

		tiles = new Tile[width, height];

		// Initialise the world array
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				// By default all tiles are empty
				tiles [x, y] = new Tile (x, y, TileType.Empty, this);
				tiles [x, y].OnBorderDefinitionChanged += OnTileBorderDefinitionChanged;
				tiles [x, y].OnTileAdditionChanged += OnTileAdditionChanged;
			}
		}

		// Every world starts with a default 10x10 room in the middle
		int s = 5;
		for (int x = -s; x <= s; x++) {
			for (int y = -s; y <= s; y++) {
				Tile t = tiles [width / 2 + x, height / 2 + y];
				t.SetFloor ();
				if (x == s || x == -s || y == s || y == -s) {
					t.InstallAddition (new Wall (t), true);
				}
			}
		}

		Room startRoom = new Room (this, tiles [width / 2, height / 2]);
		AddRoom (startRoom);

		graph = new WorldGraph (rooms, this);
	}

	public void Update(float deltaTime){
		foreach (Character character in characters) {
			character.Update (deltaTime);
		}
		foreach (Room room in rooms) {
			room.Update (deltaTime);
		}
	}

	void OnTileAdditionChanged(Tile tile, TileAddition oldAddition, TileAddition newAddition){
		// Installing a tile addition might require to update the graph
		if (oldAddition == null && newAddition != null) {
			InvalidateWorldGraph (tile.Room);
			return;
		}

		if (newAddition == null || oldAddition.MovementCostMultiplier != newAddition.MovementCostMultiplier) {
			InvalidateWorldGraph (tile.Room);
		}
	}

	/// <summary>
	/// Invalidates the world graph.
	/// </summary>
	/// <param name="room">The room that caused the world graph to be invalid and require updating</param>
	void InvalidateWorldGraph(Room room){
		// Mark our graph to be outdated, if pathfinding is requested the next time, generate a new graph
		// This avoids regenerating it instantly (multiple times) when multiple actions on a frame invalidate it.
		graphOutDated = true;
	}

	void OnRoomNoTiles(Room room){
		rooms.Remove (room);
		room.OnRoomTilesDepleted -= OnRoomNoTiles;
	}

	/// <summary>
	/// A tile has changed from being part of a room to being an edge
	/// OR the tile has changed from being an edge to being part of a room
	/// 
	/// This means we create a new room on the new tile, or rooms around the new tile
	/// </summary>
	/// <param name="t">The tile that has changed</param>
	void OnTileBorderDefinitionChanged(Tile t){
		if (t.DefinesRoomBorder ()) {
			// It defines a border now but didn't before
			// This means we installed some sort of wall (or there is a hole in the ship? TODO)

			Tile north = GetTileAt (t.X, t.Y + 1);
			Tile east = GetTileAt (t.X + 1, t.Y);
			Tile south = GetTileAt (t.X, t.Y - 1);
			Tile west = GetTileAt (t.X - 1, t.Y);

			Room northRoom = null;
			if (!north.DefinesRoomBorder ()) {
				northRoom = new Room (this, north);
				AddRoom (northRoom);
			}

			Room eastRoom = null, southRoom = null, westRoom = null;

			if (!east.DefinesRoomBorder() && east.Room != northRoom) {
				eastRoom = new Room (this, east);
				AddRoom (eastRoom);
			}
			if (!south.DefinesRoomBorder() && south.Room != northRoom && south.Room != eastRoom) {
				southRoom = new Room (this, south);
				AddRoom (southRoom);
			}
			if (!west.DefinesRoomBorder () && west.Room != northRoom && west.Room != eastRoom && west.Room != southRoom) {
				westRoom = new Room (this, west);
				AddRoom (westRoom);
			}

		} else {
			// There used to be a wall here, not anymore though
			// Create a room at the current tile.
			Room r = new Room (this,t);
			AddRoom (r);
		}
		// TODO: just update a single piece of the graph instead of regene rating the entire thing :)
		graphOutDated = true;
	}

	private void AddRoom(Room room){
		room.OnRoomTilesDepleted += OnRoomNoTiles;	
		room.OnRoomDoorAdded += InvalidateWorldGraph;
		rooms.Add (room);
	}

	public bool AddCharacter(Character character){
		if (characters.Contains (character)) {
			Debug.LogError ("This character is already part of the world");
			return false;
		}
		characters.Add (character);
        if(OnCharacterAdded != null)
        {
            OnCharacterAdded(character);
        }
		return true;
	}

	public Character GetCharacterAt(Tile t){
		foreach (Character c in characters) {
			if (c.CurrTile == t) {
				return c;
			}
		}
		return null;
	}

	public Tile GetTileAt(int x, int y){
		if( x >= 0 && x < Width && y >= 0 && y < Height)
			return tiles [x, y];
		return null;
	}


	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// 
	/// 										SERIALIZATION OF THE ENTIRE? WORLD
	/// 
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public XmlSchema GetSchema() {
		return null;
	}

	public void WriteXml (XmlWriter writer) {

		writer.WriteAttributeString ("Width", Width.ToString ());
		writer.WriteAttributeString ("Height", Height.ToString ());

        /*
         *          TILES
         */ 
        writer.WriteStartElement ("Tiles");
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				tiles [x, y].WriteXml (writer);
			}
		}
		writer.WriteEndElement ();

        /*
         *          ROOMS
         */
        writer.WriteStartElement("Rooms");
        foreach (Room r in rooms)
        {
            r.WriteXml(writer);
        }
        writer.WriteEndElement();

        /*
         *          CHARACTERS
         */
        writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            c.WriteXml(writer);
        }
        writer.WriteEndElement();

        // It is important that we write the jobs last, because they require the tiles and their additions to be properly initialised
        Jobs.WriteXml(writer);
    }

	public void ReadXml (XmlReader reader) {
		// Read all the xml data
		Debug.Log("Reading world data");

		Width = int.Parse (reader.GetAttribute ("Width"));
		Height = int.Parse (reader.GetAttribute ("Height"));

		SetupWorld (Width, Height);

		while (reader.Read ()) {
            switch (reader.Name)
            {
                case "Rooms":
                    ReadXMLRooms(reader);
                    break;
                case "Tiles":
                    ReadXMLTiles(reader);
                    break;
                case "Characters":
                    ReadXMLCharacters(reader);
                    break;
                case "JobQueue":
                    Jobs.ReadXml(reader, this);
                    break;
            }
		}
	}

	protected void ReadXMLTiles(XmlReader reader){
		Debug.Log ("Reading Tile data");
		int count = 0;
		if (reader.ReadToDescendant ("Tile")) {
			// We have at least one tile to read
			do { // Read it while there are more tiles to read
				int X = int.Parse(reader.GetAttribute("X"));
				int Y = int.Parse(reader.GetAttribute("Y"));

				tiles[X,Y].ReadXml(reader);
				count++;
			} while(reader.ReadToNextSibling("Tile"));
		}
		Debug.Log("Read " + count + " Tiles");
	}

	protected void ReadXMLRooms(XmlReader reader){
        Debug.Log("Reading Room data");
        // Before the rooms are placed in the world, we should delete all old rooms
        int count = 0;
        if (reader.ReadToDescendant("Room"))
        {
            // We have at least one room to read
            do
            { // Read it while there are more tiles to read
                int X = int.Parse(reader.GetAttribute("X"));
                int Y = int.Parse(reader.GetAttribute("Y"));

                // We don't care about old rooms, our new rooms should fully envelop them anyway
                // So they should clean themselves up
                // I see a potential bug if you deconstruct the original room in your world, that way it won't get cleaned up since nothing is there
                // But that is really an issue for another day?
                // TODO: potential game breaking bug if you try really hard to break saving

                Room r = new Room(this, tiles[X, Y]);
                AddRoom(r);
                count++;
            } while (reader.ReadToNextSibling("Room"));
        }
        Debug.Log("Read " + count + " Rooms");
    }

	protected void ReadXMLCharacters(XmlReader reader){
		Debug.Log ("Reading character data");
        // Clear the old characters
        characters.Clear();

        int count = 0;
        if (reader.ReadToDescendant("Character"))
        {
            do
            { // Read it while there are more characters to read
                int X = int.Parse(reader.GetAttribute("X"));
                int Y = int.Parse(reader.GetAttribute("Y"));

                Character c = new Character(GetTileAt(X, Y), this);
                c.ReadXml(reader);
                count++;
                AddCharacter(c);
            } while (reader.ReadToNextSibling("Character"));
        }
        Debug.Log("Read " + count + " Characters");
    }

}
