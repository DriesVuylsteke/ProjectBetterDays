using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGraph {

	public Dictionary<Tile, Node<Tile>> nodes;

	// The room with the list of the tiles to build a path from
	// The world to look up tiles in a timely manner
	public RoomGraph(Room room, World world){
		nodes = new Dictionary<Tile, Node<Tile>> ();

		// Create all the nodes (one for each tile)
		foreach (Tile t in room.tiles) {
			Node<Tile> node = new Node<Tile> ();
			node.data = t;
			nodes.Add (t, node);
		}
		// Don't forget to add nodes for doors!
		if (room.additions.ContainsKey (Door.AdditionName)) {
			foreach (TileAddition door in room.additions[Door.AdditionName]) {
				Node<Tile> node = new Node<Tile> ();
				node.data = door.tile;
				nodes.Add (door.tile, node);
			}
		}


		foreach (Tile t in nodes.Keys) {
			Node<Tile> node = nodes [t];
			List<Edge<Tile>> edges = new List<Edge<Tile>> ();

			Tile[] neighbours = t.GetNeighbours (true);
			foreach (Tile nb in neighbours) {
				// A walkable neighbour that is part of the room
				if (nb.MovementCost > 0 && nodes.ContainsKey(nb) && IsClippingCorner(t,nb) == false) {
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
