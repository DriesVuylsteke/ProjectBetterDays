using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class AStarWorldPath {

	Stack<Tile> path;

	public AStarWorldPath(WorldGraph graph, Tile tileStart, Tile tileEnd){

		// A dictionary of all valid walkable nodes (doors in this case)
		Dictionary<Tile, Node<Tile>> nodes = graph.nodes;

		// Make sure start/end tiles rooms have a door
		bool containsStart = false;
		if (tileStart.Room == null || tileEnd.Room == null) {
			Debug.LogError ("Trying to move on top of a door");
		}

		if (!tileStart.Room.additions.ContainsKey (Door.AdditionName)) {
			Debug.LogError ("Starting room does not have a door to leave the room");
			return;
		}
		if (!tileEnd.Room.additions.ContainsKey (Door.AdditionName)) {
			Debug.LogError ("Destination room does not have a door to enter the room");
			return;
		}

		// Make sure we are aware of the door
		foreach (TileAddition door in tileStart.Room.additions[Door.AdditionName]) {
			if (nodes.ContainsKey (door.tile)) {
				containsStart = true;
				break;
			}
		}
		if (containsStart == false) {
			Debug.LogError ("AStarWorld: the starting door isn't in the list of nodes");
			return;
		}

		bool containsEnd = false;
		foreach (TileAddition door in tileEnd.Room.additions[Door.AdditionName]) {
			if (nodes.ContainsKey (door.tile)) {
				containsEnd = true;
				break;
			}
		}
		if (containsEnd == false) {
			Debug.LogError ("AStarWorld: the end door isn't in the list of nodes");
			return;
		}


		///
		// 		START OF ACTUAL PATHFINDING
		///

		/*
		 * Mostly following this pseudocode:
		 * https://en.wikipedia.org/wiki/A*_search_algorithm
		 */ 
		List<Node<Tile>> closedSet = new List<Node<Tile>> ();

		SimplePriorityQueue<Node<Tile>> openSet = new SimplePriorityQueue<Node<Tile>> ();
		Dictionary<Node<Tile>, Node<Tile>> came_from = new Dictionary<Node<Tile>, Node<Tile>> ();

		Dictionary<Node<Tile>, float> g_score = new Dictionary<Node<Tile>, float> ();
		foreach (Node<Tile> n in nodes.Values) {
			g_score [n] = Mathf.Infinity;
		}

		Dictionary<Node<Tile>, float> f_score = new Dictionary<Node<Tile>, float> ();
		foreach (Node<Tile> n in nodes.Values) {
			f_score [n] = Mathf.Infinity;
		}

		// enqueue all doors from the starting room
		Node<Tile> charNode = new Node<Tile>();
		charNode.data = tileStart;

		Node<Tile> destNode = new Node<Tile>();
		destNode.data = tileEnd;

		foreach (TileAddition door in tileStart.Room.additions[Door.AdditionName]) {
			// enqueue them based on their distance from the character (straight line heuristic, lets assume no shenanigans)
			Node<Tile> curNode = nodes [door.tile];

			openSet.Enqueue (curNode, Heuristic_cost_estimate(charNode, curNode));
			g_score [curNode] = Heuristic_cost_estimate (charNode, curNode);
			f_score [curNode] = Heuristic_cost_estimate (curNode, destNode);
		}

		while (openSet.Count > 0) {
			Node<Tile> current = openSet.Dequeue ();

			// We reached our goal if our current node is part of the doors of the destination room
			foreach (TileAddition door in tileEnd.Room.additions[Door.AdditionName]) {
				if (current.data == door.tile) {
					// We have reached our goal, lets convert this to an actual sequence of tiles to walk on
					// then end this constructor function
					Debug.Log("Found a path in our world graph");
					ReconstructPath(came_from, current);
					return;
				}
			}

			closedSet.Add (current);

			foreach (Edge<Tile> edgeNeigbour in current.edges) {
				Node<Tile> neighbor = edgeNeigbour.destination;
				if (closedSet.Contains (neighbor))
					continue; // ignore this already completed neighbor

				float movementCostToNeigbour = neighbor.data.MovementCost * Heuristic_cost_estimate (current, neighbor);
				float tentative_g_score = g_score [current] + movementCostToNeigbour;

				if (openSet.Contains (neighbor) && tentative_g_score >= g_score[neighbor]) {
					continue;
				}

				came_from [neighbor] = current;
				g_score [neighbor] = tentative_g_score;
				f_score [neighbor] = g_score [neighbor] + Heuristic_cost_estimate (neighbor, destNode);

				if (openSet.Contains (neighbor) == false) {
					openSet.Enqueue (neighbor, f_score [neighbor]);
				} else {
					openSet.UpdatePriority (neighbor, f_score [neighbor]);
				}
			} // foreach neigbour
		} // while

		// If we reached here, it means we've burned through the entire
		// OpenSet without ever re aching a point where current == goal
		// This happens when there is no path from start to goal
		// (so there's a wall or missing floor or something)

		// We don't have a failure state, maybe? It's just that th
		// pathlist will be null
	}

	void ReconstructPath(Dictionary<Node<Tile>, Node<Tile>> cameFrom,
		Node<Tile> current){
		// current is the goal, so what we want to do is walk backwards through the camemfrom
		// map until we reach the end of that map.. which will be our starting node
		path = new Stack<Tile>();
		path.Push (current.data);

		while (cameFrom.ContainsKey (current)) {
			// Came from is a map where the
			// key => value relation is saying
			// some node => we got there from this node
			current = cameFrom [current];
			path.Push (current.data);
		}
		// At this point path is a stack that runs from start to end (start is on top)
	}

	float Heuristic_cost_estimate(Node<Tile> a, Node<Tile> b){
		return Mathf.Sqrt (
			Mathf.Pow (a.data.X - b.data.X, 2) +
			Mathf.Pow (a.data.Y - b.data.Y, 2)
		);
	}

	public Tile DequeueNextTile(){
		if(path != null && path.Count > 0)
			return path.Pop ();
		return null;
	}

	public int Length(){
		if (path == null)
			return 0;
		return path.Count;
	}
}
