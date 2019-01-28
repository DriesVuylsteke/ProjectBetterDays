using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour {

	public static WorldController instance { get; private set;}
	public World world { get; private set; }

	private Color[] colors = new Color[] { Color.grey, Color.black, Color.cyan, Color.magenta};
	static bool loadWorld = false;

	void OnEnable(){
		if (instance != null) {
			Debug.LogError ("There should never be two worldcontrollers");
		}
		instance = this;

		if (loadWorld) {
			DeSerializeAndLoadWorld ();
		} else {
			world = new World (100, 100);
			Camera.main.transform.position = new Vector3 (50, 50, Camera.main.transform.position.z);
		}
	}

	void Update(){
		world.Update (Time.deltaTime);
	}

	void OnDrawGizmos(){
		if (world == null || world.GetRooms() == null)
			return;

		// We will display rooms as a gizmo once they work!
		int colorID = 0;
		foreach (Room r in world.GetRooms()) {
			Gizmos.color = colors [colorID];
			foreach (Tile t in r.tiles) {
				Gizmos.DrawCube (new Vector3 (t.X, t.Y, 0), new Vector3 (1, 1, 0));
			}

			colorID++;
			colorID %= colors.Length;
		}

		/*foreach (Room r in world.rooms) {
			Gizmos.color = colors [colorID];
			foreach (Tile t in r.roomEdges) {
				Gizmos.DrawCube (new Vector3 (t.X, t.Y, 0), new Vector3 (1, 1, 0));
			}

			colorID++;
			colorID %= colors.Length;
		}*/
	}

	public void SerializeAndSaveWorld(){
		string path = "Assets/Resources/save.txt";

		//Re-import the file to update the reference in the editor
		AssetDatabase.ImportAsset(path); 

		XmlSerializer serializer = new XmlSerializer (typeof(World));
		TextWriter writer = new StringWriter ();
		serializer.Serialize (writer, world);
		writer.Close ();

		Debug.Log ("Writing to: " + path);
		StreamWriter fileWriter = new StreamWriter(path, false);
		fileWriter.WriteLine(writer.ToString());
		fileWriter.Close();
	}

	public void LoadWorld(){
		loadWorld = true;
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	void DeSerializeAndLoadWorld(){
		Debug.Log ("Loading world");
		string path = "Assets/Resources/save.txt";

		StreamReader streamreader = new StreamReader (path, false);
		string data = streamreader.ReadToEnd ();
		streamreader.Close ();

		XmlSerializer serializer = new XmlSerializer (typeof(World));
		TextReader reader = new StringReader (data);
		this.world = (World)serializer.Deserialize (reader);
		reader.Close ();

		Camera.main.transform.position = new Vector3 (world.Width/2, world.Height/2, Camera.main.transform.position.z);
	}
}
