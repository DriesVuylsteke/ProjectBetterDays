using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayTileInfo : MonoBehaviour {

	public GameObject displayPanel;

	public Text temperatureText;
	public Text oxygenText;
	public Text doorsText;
	public Text tilesText;
	public Text wallsText;

	MouseController mouseController;

	// Use this for initialization
	void Start () {
		mouseController = GameObject.FindObjectOfType<MouseController> ();
	}
	
	// Update is called once per frame
	void Update () {
		Tile curTile = mouseController.GetTileUnderMouse ();

		if (curTile == null || curTile.Room == null) {
			temperatureText.text = "No Room under mouse";
			oxygenText.text = "TotalRooms: " + WorldController.instance.world.GetRooms().Count;
			tilesText.text = "";
			wallsText.text = "";
			doorsText.text = "";
			return;
		}

		temperatureText.text = "Temperature: " + curTile.Room.Temperature;
		oxygenText.text = "Oxygen: " + curTile.Room.OxygenLevel;
		tilesText.text = "Tiles: " + curTile.Room.tiles.Count;
		string k = "";
		foreach (string s in curTile.Room.additions.Keys) {
			k += s + " ";
		}
		wallsText.text = "Keys: " + k;
		doorsText.text = "WHFOEWMSEBMF";
		if (curTile.Room.additions.ContainsKey ("Door")) {
			doorsText.text = "Doors: " + curTile.Room.additions ["Door"].Count;
		} else {
			doorsText.text = "No doors found";
		}
	}
}
