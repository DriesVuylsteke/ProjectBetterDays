using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node<T> {
	public List<Edge<T>> edges;
	public T data;

	public Node(){
		edges = new List<Edge<T>> ();
	}
}
