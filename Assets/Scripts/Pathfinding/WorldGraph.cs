using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldGraph {

	public Dictionary<Tile, Node<Tile>> nodes;

	// The room with the list of the tiles to build a path from
	// The world to look up tiles in a timely manner
	public WorldGraph(List<Room> rooms, World world){
		nodes = new Dictionary<Tile, Node<Tile>> ();

		// Loop over all rooms
		foreach (Room room in rooms) {
			// Add nodes for all the tiles in the room
			foreach (Tile t in room.tiles) {
				Node<Tile> node = new Node<Tile> ();
				node.data = t;
				nodes.Add (t, node);
			}

			// Loop over all the doors
			if (room.additions.ContainsKey (Door.AdditionName)) {
				// The room has doors, lets loop over them and add nodes for the doors if they don't exist yet
				foreach (TileAddition door in room.additions[Door.AdditionName]) {
					Tile t = door.tile;
					// if the node doesn't exist yet for this door, make it !
					// It could exist because multiple rooms are aware of a single door
					if (!nodes.ContainsKey (t)) {
						Node<Tile> node = new Node<Tile> ();
						node.data = t;
						nodes.Add (t, node);
					}
				}
			}
		}

		foreach (Tile t in nodes.Keys) {
			Node<Tile> node = nodes [t];
			List<Edge<Tile>> edges = new List<Edge<Tile>> ();

			Tile[] neighbours = t.GetNeighbours (true);
			foreach (Tile nb in neighbours) {
				// A walkable neighbour that is part of the room
				if (nb.MovementCost > 0 && nodes.ContainsKey(nb) && IsClippingCorner(t,nb) == false) {
					// Lets prevent cutting of corners

					Edge<Tile> edge = new Edge<Tile> ();
					edge.weight = nb.MovementCost;
					edge.destination = nodes [nb];
					edges.Add (edge);
				}
			}

			node.edges = edges;
		}
	}

	bool IsClippingCorner(Tile curr, Tile nb){
		// If movement is diagonal, we have the possibility of clipping a corner

		int dx = curr.X - nb.X;
		int dy = curr.Y - nb.Y;

		// Moving both horizontal and vertical = diagonal
		if (Mathf.Abs (dx) + Mathf.Abs (dy) == 2) {

			if (curr.world.GetTileAt (curr.X - dx, curr.Y).MovementCost == 0) {
				// East or west is unwalkable, so clipped movement
				return true;
			}

			if (curr.world.GetTileAt (curr.X, curr.Y - dy).MovementCost == 0) {
				// north or south is unwalkable, so clipped movement
				return true;
			}
		}

		return false;
	}
}
