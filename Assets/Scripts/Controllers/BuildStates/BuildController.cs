using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BuildController : MonoBehaviour {

	MouseController mouseController;
	TileSpriteController spriteController;

    // A script that displays the jobs a character can perform at a certain tile
    public TileJobList tileJobList;

	World world { get { return WorldController.instance.world; } }

	public float buildDelayStart;
	private float buildDelay;

	BuildState state;
	public BuildState State {
		get{
			return state;
		}
		set {
			state = value;
			buildDelay = buildDelayStart;
			isDragging = false;

			if (value == null) {
				if (SelectedCharacter != null) {
					SelectedCharacter = null;
				}
			}

            if (state != null && state.GetTooltipSpriteName () != null) {
				cursorGO.GetComponent<SpriteRenderer> ().sprite = spriteController.GetSprite (state.GetTooltipSpriteName ());
			} else {
				cursorGO.GetComponent<SpriteRenderer> ().sprite = null;
			}
		}
	}

	// Selection area
	Texture2D selectionTexture;
	bool isDragging = false;
	public Color SelectionColour;

	[SerializeField]
	GameObject cursorGO;

	Character selectedCharacter;
	public Character SelectedCharacter {
		get {
			return selectedCharacter;
		}
		set {
            if(selectedCharacter == value)
            {
                selectedCharacter.DeselectCharacter();
                selectedCharacter = null;
                return;
            }

			if (selectedCharacter != null) {
				selectedCharacter.DeselectCharacter ();
			}
			selectedCharacter = value;
			if(selectedCharacter != null)
				selectedCharacter.SelectCharacter ();
		}
	}

	// Use this for initialization
	void Start () {
		mouseController = GameObject.FindObjectOfType<MouseController> ();
		spriteController = GameObject.FindObjectOfType<TileSpriteController> ();

		selectionTexture = new Texture2D (1, 1);
		selectionTexture.SetPixel (0, 0, Color.white);
		selectionTexture.Apply ();
	}
		
	// Update is called once per frame
	void Update () {
		cursorGO.transform.position = mouseController.GetMousePosition();

		// If we are over a UI element, bail out
		if (EventSystem.current.IsPointerOverGameObject ()) {
			return;
		}

		if (buildDelay > 0) {
			buildDelay -= Time.deltaTime;
			return;
		}

		if (Input.GetMouseButtonDown (0)) {
			isDragging = true;

			if (State == null) {
				// We can select a new character
				Character c = world.GetCharacterAt (mouseController.GetTileUnderMouse ());
				if (c != null)
					SelectedCharacter = c;
			} else {
				state.MouseButton0Down ();
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			isDragging = false;
			if(state != null)
				state.MouseButton0Up ();
		}
		if(Input.GetMouseButtonDown(1)){
			if(state != null)
				state.MouseButton1Down();
            else
            {
                // We aren't building, call the TileJobList to display a list of active jobs on the current tile
                tileJobList.Mouse1Down(SelectedCharacter, mouseController.GetTileUnderMouse());
            }
		}
	}

	void OnGUI(){
		if (!isDragging)
			return;

		GUI.color = SelectionColour;

		Tile startTile = mouseController.tilePressPos;
		if (startTile == null) {
			Debug.LogError ("You are trying to build without a starting tile?");
			return;
		}
		Vector3 screenPos1 = Camera.main.WorldToScreenPoint (new Vector3 (startTile.X, startTile.Y, 0));
		Vector3 screenPos2 = Camera.main.WorldToScreenPoint (mouseController.GetMouseInWorldCoordinates ());

		// Move the origin from bottom left to top left
		screenPos1.y = Screen.height - screenPos1.y;
		screenPos2.y = Screen.height - screenPos2.y;

		var topLeft = Vector3.Min (screenPos1, screenPos2);
		var bottomRight = Vector3.Max (screenPos1, screenPos2);

		Rect drawArea = Rect.MinMaxRect (topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
		GUI.DrawTexture (drawArea, selectionTexture);
	}

	public void BuildMode_Floor(){
		State = new BuildTileTypeState (mouseController, this, TileType.Floor);
	}

	public void BuildMode_Empty(){
		State = new BuildTileTypeState (mouseController, this, TileType.Empty);
	}

	public void BuildMode_Wall(){
		State = new BuildAdditionState (mouseController, this, new Wall(null), Skills.Construction);
	}

	public void BuildMode_Door(){
		State = new BuildAdditionState (mouseController, this, new Door(null), Skills.Construction);
	}

	public void BuildMode_BulldozeAddition(){
		State = new BuildAdditionState (mouseController, this, null, Skills.Construction);
	}

	public void BuildMode_OxygenGenerator(){
		State = new BuildAdditionState (mouseController, this, new OxygenGenerator(null), Skills.Construction);
	}

    public void BuildMode_Soil()
    {
        State = new BuildAdditionState(mouseController, this, new Soil(null), Skills.Planting);
    }

    public void BuildMode_Tomato()
    {
        State = new BuildAdditionState(mouseController, this, new Tomato(null), Skills.Planting);
    }
}
