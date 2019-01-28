using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

	Vector3 lastFramePostion;
	Vector3 currFramePosition;

	World world { get { return WorldController.instance.world; } }

	public Vector3 mousePressPos { get; protected set; }
	public Vector3 mousePressWorldPos { get; protected set; }
	// The tile that was under the mouse when the left mouse button was pressed
	public Tile tilePressPos { get; protected set; }

	/// <summary>
	/// Gets the current mouse position in world space (integers).
	/// </summary>
	/// <returns>The mouse position in world space.</returns>
	public Vector3 GetMousePosition(){
		Vector3 pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		pos.z = 0;
		return pos;
	}

	public Vector3 GetMouseInWorldCoordinates(){
		Vector3 worldSpaceCoordinate = GetMousePosition ();
		worldSpaceCoordinate.x = Mathf.RoundToInt (worldSpaceCoordinate.x);
		worldSpaceCoordinate.y = Mathf.RoundToInt (worldSpaceCoordinate.y);
		return worldSpaceCoordinate;
	}

	public Tile GetTileUnderMouse(){
		return world.GetTileAt ((int)GetMouseInWorldCoordinates().x, (int)GetMouseInWorldCoordinates().y);
	}

	void Update(){
		currFramePosition = GetMousePosition ();

		// If we are over a UI element, bail out
		if (EventSystem.current.IsPointerOverGameObject ()) {
			return;
		}

		if (Input.GetMouseButtonDown (0)) {
			mousePressPos = Input.mousePosition;
			mousePressWorldPos = GetMouseInWorldCoordinates ();
			tilePressPos = GetTileUnderMouse ();
		}

		UpdateCameraMovement ();

		lastFramePostion = GetMousePosition();
	}


	void UpdateCameraMovement(){
		if (Input.GetMouseButton (2)) {
			Vector3 diff = lastFramePostion - currFramePosition;
			Camera.main.transform.Translate (diff);
		}

		Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis ("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 3f, 25f);
	}
}
