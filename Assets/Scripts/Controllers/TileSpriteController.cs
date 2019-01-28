using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpriteController : MonoBehaviour {

	Dictionary<Tile, GameObject> TileGOMap;
	Dictionary<Tile, GameObject> additionGOMap;
	Dictionary<string, Sprite> tileSprites;
	Dictionary<string, Sprite> additionSprites;

	World world { get { return WorldController.instance.world; } }

	MouseController mouseController;

	Tile oldTile, currTile;

	public Color highlightColor;

	// Use this for initialization
	void Start () {
		TileGOMap = new Dictionary<Tile, GameObject> ();
		additionGOMap = new Dictionary<Tile, GameObject> ();
		tileSprites = new Dictionary<string, Sprite> ();
		additionSprites = new Dictionary<string, Sprite> ();

		LoadSprites ();

		// Set up gameobjects for all tiles in the world
		// We will not create tiles after this point, ONLY edit them
		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {
				Tile tile_data = world.GetTileAt (x, y);

				// Create a game object to render the sprite
				GameObject tile_go = new GameObject ();
				TileGOMap.Add (tile_data, tile_go);
				tile_go.name = "tile_" + x + "_" + y;
				tile_go.transform.position = new Vector3 (tile_data.X, tile_data.Y, 0);
				tile_go.transform.SetParent (transform);

				SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
				sr.sprite = tileSprites [tile_data.TileType.ToString ()];
				sr.sortingLayerName = "Tiles";

				tile_data.OnTileTypeChanged += OnTileTypeChanged;
				tile_data.OnTileAdditionChanged += OnTileAdditionChanged;

				// force trigger the events upon creation so we can properly render them (the world creates and fires the events before the controller exists)
				if (tile_data.addition != null) {
					OnTileAdditionChanged (tile_data, null, tile_data.addition);
					OnTileAdditionWorkDone (tile_data.addition);
				}
			}
		}

		mouseController = GameObject.FindObjectOfType<MouseController> ();
	}

	// Listens to the tile for when the adddition changes, merely updates our listener to start listening to the right addition
	void OnTileAdditionChanged(Tile tile, TileAddition oldAddition, TileAddition newAddition){

        if (newAddition == null)
        {
            // No addition is present here anymore, just disable the SR for now (no need to delete the game object, we might need it later?)
            if (additionGOMap.ContainsKey(tile) == false)
            {
                return;
            }
            additionGOMap[tile].GetComponent<SpriteRenderer>().sprite = null;
            return;
        }

		if (additionGOMap.ContainsKey (tile) == false) {
			// There is no gameobject yet to visualise the tile additions for this tile
			// So lets create one
			GameObject additionGO = new GameObject();
			additionGOMap.Add (tile, additionGO);
			additionGO.name = "Addition_" + tile.X + "_" + tile.Y + ": " + (newAddition == null ? "" : newAddition.GetRenderState());
			additionGO.transform.position = new Vector3 (tile.X, tile.Y, 0);
			additionGO.transform.SetParent (TileGOMap[tile].transform);
			additionGO.transform.eulerAngles = new Vector3 (0, 0, newAddition.Orientation);

			SpriteRenderer sr = additionGO.AddComponent<SpriteRenderer>();
			sr.sprite = newAddition == null ? null : additionSprites [newAddition.GetRenderState()];
			sr.sortingLayerName = "TileAdditions";
		}
		if (oldAddition != null) {
			oldAddition.WorkDone -= OnTileAdditionWorkDone;
		}

		newAddition.WorkDone += OnTileAdditionWorkDone;

		// Manually call OnWorkDone to update the texture
		OnTileAdditionWorkDone(newAddition);
	}

	public Sprite GetSprite(string spriteName){
		if (tileSprites.ContainsKey (spriteName)) {
			return tileSprites [spriteName];
		} else if (additionSprites.ContainsKey (spriteName)) {
			return additionSprites [spriteName];
		} else {
			Debug.LogError ("Asking for an unexisting sprite: " + spriteName);
			return null;
		}
	}

	void Update(){
		Tile currTile = mouseController.GetTileUnderMouse ();

		if (currTile != oldTile && oldTile != null && currTile != null) {
			TileGOMap [oldTile].GetComponent<SpriteRenderer> ().color = Color.white;
			TileGOMap [currTile].GetComponent<SpriteRenderer> ().color = highlightColor;
		}

		oldTile = currTile;
	}

	void OnTileTypeChanged (Tile tile, TileType oldType, TileType newType)
	{
		if (TileGOMap.ContainsKey (tile) == false) {
			Debug.LogError ("A tile without a gameobject is talking to us! This should not be possible");
			return;
		}

		GameObject tile_go = TileGOMap [tile];
		SpriteRenderer sr = tile_go.GetComponent<SpriteRenderer> ();
		sr.sprite = tileSprites [newType.ToString ()];
	}

	void OnTileAdditionWorkDone (TileAddition addition){
		Tile tile = addition.tile;
		// For now we just set an alpha texture, later on we could do stages of progress based on the buildpercentage
		// At this point we know that there is a gameobject to render the new tile addition;

		//

		GameObject addition_go = additionGOMap [addition.tile];
		addition_go.transform.eulerAngles = new Vector3 (0, 0, addition.Orientation);

		SpriteRenderer spriteRenderer = additionGOMap[tile].GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = addition == null ? null : additionSprites [addition.GetRenderState()];
		Color c = spriteRenderer.color;
		c.a = addition.BuildPercentage == 1 ? 1 : 0.4f;
		spriteRenderer.color = c;
	}

	void LoadSprites(){
		Sprite[] sprites = Resources.LoadAll<Sprite> ("Textures/Tiles/");
		foreach (Sprite s in sprites) {
			tileSprites [s.name] = s;
		}

		sprites = Resources.LoadAll<Sprite> ("Textures/TileAdditions/");
		foreach (Sprite s in sprites) {
			additionSprites [s.name] = s;
		}
	}
}
