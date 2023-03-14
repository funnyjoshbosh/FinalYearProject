using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node> {
	
	public bool walkable;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;

	public int costToThisNode; // Cost so far
	public int heuristic; // Estimated cost left
	public Node parent; // Previous node in path
	int heapIndex;
	
	// Node constructor
	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	}

	// Estimated journey cost
	public int estimatedTotalDistance {
		get {
			return costToThisNode + heuristic;
		}
	}

	public int HeapIndex {
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	// Compare two nodes, what is lower cost
	public int CompareTo(Node nodeToCompare) {
		int compare = estimatedTotalDistance.CompareTo(nodeToCompare.estimatedTotalDistance); // which has a better total cost
		if (compare == 0) {
			compare = heuristic.CompareTo(nodeToCompare.heuristic); // which has a better estimated cost
		}
		return -compare; // take the smaller
	}
}
