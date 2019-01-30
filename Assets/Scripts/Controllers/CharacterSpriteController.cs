using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterSpriteController : MonoBehaviour {

	Dictionary<Character, GameObject> CharacterGOMap;
	Dictionary<string, Sprite> characterSprites;

	World world { get { return WorldController.instance.world; } }

	MouseController mouseController;
	public Color highlightColor;

    // Use this for initialization
    void Start()
    {
        CharacterGOMap = new Dictionary<Character, GameObject>();
        characterSprites = new Dictionary<string, Sprite>();

        LoadSprites();

        mouseController = GameObject.FindObjectOfType<MouseController>();
        world.OnCharacterAdded += CreateCharacterGO;

        // DEBUG ONLY:
        // Lets add a character for now
        if (world.GetCharacters().Count == 0)
        {
            CreateCharacter(50, 50);
            //CreateCharacter(52, 50);
        }
        else
        {
            foreach (Character c in world.GetCharacters())
            { // Create the character game objects for the characters that already exist
                CreateCharacterGO(c);
            }
        }
	}

	void CreateCharacter(int x, int y){
		Character character = new Character(world.GetTileAt(x,y), world);
		world.AddCharacter (character);
	}

    void CreateCharacterGO(Character character)
    {
        character.OnCharacterPositionChanged += OnCharacterPositionChanged;
        character.OnCharacterSelectedChanged += OnCharacterSelectedChanged;

        GameObject char_go = new GameObject();
        CharacterGOMap.Add(character, char_go);
        char_go.name = "Character";
        char_go.transform.position = new Vector3(character.X, character.Y, 0);
        char_go.transform.SetParent(transform);

        SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
        sr.sprite = characterSprites["Character"];
        sr.sortingLayerName = "Characters";
    }

	void Update(){
		
	}

	void OnCharacterSelectedChanged(Character character){
		if (CharacterGOMap.ContainsKey (character) == false) {
			Debug.LogError ("A character without a gameobject is talking to us! This should not be possible");
			return;
		}
		// Move the character
		GameObject char_go = CharacterGOMap [character];
		SpriteRenderer sr = char_go.GetComponent<SpriteRenderer> ();

        Debug.Log("Character selection changed");
		sr.color = character.Selected ? highlightColor : Color.white;
	}

	void OnCharacterPositionChanged (Character character)
	{
		if (CharacterGOMap.ContainsKey (character) == false) {
			Debug.LogError ("A character without a gameobject is talking to us! This should not be possible");
			return;
		}
		// Move the character
		GameObject char_go = CharacterGOMap [character];
		char_go.transform.position = new Vector3 (character.X, character.Y, 0);
	}

	void LoadSprites(){
		Sprite[] sprites = Resources.LoadAll<Sprite> ("Textures/Characters/");
		foreach (Sprite s in sprites) {
			characterSprites [s.name] = s;
		}
	}

	void OnDrawGizmos(){
		if (!EditorApplication.isPlaying || world.GetCharacters() == null || world.GetCharacters().Count == 0)
			return;

		// Draw the graph for every room
		Gizmos.color = Color.black;
		WorldGraph graph = world.Graph;
		foreach (Node<Tile> node in graph.nodes.Values) {
			foreach (Edge<Tile> edge in node.edges) {
				Gizmos.DrawLine (
					new Vector3 (node.data.X, node.data.Y, 0),
					new Vector3 (edge.destination.data.X, edge.destination.data.Y, 0));
			}
		}

		// Draw the graph to indicate which rooms connect to which rooms
		// Display a gizmo for the room pathfinding graph
		Gizmos.color = Color.green;
		WorldGraph worldGraph = world.Graph;
		foreach (Node<Tile> node in worldGraph.nodes.Values) {
			foreach (Edge<Tile> edge in node.edges) {
				Gizmos.DrawLine (
					new Vector3 (node.data.X, node.data.Y, 0),
					new Vector3 (edge.destination.data.X, edge.destination.data.Y, 0));
			}
		}
	}
}
