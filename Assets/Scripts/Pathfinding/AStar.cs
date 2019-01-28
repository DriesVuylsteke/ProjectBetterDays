using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class AStar {

	Stack<Tile> path;

	public AStar(WorldGraph graph, Tile tileStart, Tile tileEnd){

		// A dictionary of all valid walkable nodes
		Dictionary<Tile, Node<Tile>> nodes = graph.nodes;

		// Make sure start/end tiles are in the list of nodes
		if (nodes.ContainsKey (tileStart) == false) {
			Debug.LogError ("AStar: the starting tile isn't in the list of nodes");
			return;
		}

		if (nodes.ContainsKey (tileEnd) == false) {
			Debug.LogError ("AStar: the end tile isn't in the list of nodes");
			return;
		}

		Node<Tile> start = nodes [tileStart];
		Node<Tile> goal  = nodes [tileEnd];

		/*
		 * Mostly following this pseudocode:
		 * https://en.wikipedia.org/wiki/A*_search_algorithm
		 */ 
		List<Node<Tile>> closedSet = new List<Node<Tile>> ();

		SimplePriorityQueue<Node<Tile>> openSet = new SimplePriorityQueue<Node<Tile>> ();
		openSet.Enqueue (start, 0);

		Dictionary<Node<Tile>, Node<Tile>> came_from = new Dictionary<Node<Tile>, Node<Tile>> ();
		Dictionary<Node<Tile>, float> g_score = new Dictionary<Node<Tile>, float> ();
		foreach (Node<Tile> n in nodes.Values) {
			g_score [n] = Mathf.Infinity;
		}
		g_score [start] = 0;

		Dictionary<Node<Tile>, float> f_score = new Dictionary<Node<Tile>, float> ();
		foreach (Node<Tile> n in nodes.Values) {
			f_score [n] = Mathf.Infinity;
		}
		f_score [start] = Heuristic_cost_estimate (start, goal);

		while (openSet.Count > 0) {
			Node<Tile> current = openSet.Dequeue ();

			if (current == goal) {
				// We have reached our goal, lets convert this to an actual sequence of tiles to walk on
				// then end this constructor function
				ReconstructPath(came_from, current);
				Debug.Log ("Found path of length: " + this.Length ());
				return;
			}

			closedSet.Add (current);

			foreach (Edge<Tile> edgeNeigbour in current.edges) {
				Node<Tile> neighbor = edgeNeigbour.destination;
				if (closedSet.Contains (neighbor))
					continue; // ignore this already completed neighbor

				float movementCostToNeigbour = neighbor.data.MovementCost * DistBetween (current, neighbor);
				float tentative_g_score = g_score [current] + movementCostToNeigbour;

				if (openSet.Contains (neighbor) && tentative_g_score >= g_score[neighbor]) {
					continue;
				}

				came_from [neighbor] = current;
				g_score [neighbor] = tentative_g_score;
				f_score [neighbor] = g_score [neighbor] + Heuristic_cost_estimate (neighbor, goal);

				if (openSet.Contains (neighbor) == false) {
					openSet.Enqueue (neighbor, f_score [neighbor]);
				} else {
					openSet.UpdatePriority (neighbor, f_score [neighbor]);
				}
			} // foreach neigbour
		} // while
		Debug.LogError("No path found to destination");
		// If we reached here, it means we've burned through the entire
		// OpenSet without ever re aching a point where current == goal
		// This happens when there is no path from start to goal
		// (so there's a wall or missing floor or something)

		// We don't have a failure state, maybe? It's just that th
		// pathlist will be null
	}

	float DistBetween(Node<Tile> a, Node<Tile> b){
		// WE can make assumptions because we know we are working on a grid at this point.

		// Hori/vert neighbors have a distance of 1
		int xDiff = Mathf.Abs (a.data.X - b.data.X);
		int yDiff = Mathf.Abs (a.data.Y - b.data.Y);
		if ((xDiff + yDiff == 1)) {
			return 1f;
		}
		// diag neibours have a distance of 1.41421356237
		if (xDiff == 1 && yDiff == 1) {
			return 1.41421356237f;
		}
		//Otherwise do actual math
		return Mathf.Sqrt (
			Mathf.Pow (xDiff, 2) +
			Mathf.Pow (yDiff, 2)
		);
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
		if(path != null)
			return path.Pop ();
		return null;
	}

	public int Length(){
		if (path == null)
			return 0;
		return path.Count;
	}
}
