using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge<T> {
	public float weight { get; set; }
	public Node<T> destination { get; set; }
}
