using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding : MonoBehaviour {
	
	PathRequestManager requestManager;
	Grid grid;
	
	void Awake() {
		requestManager = GetComponent<PathRequestManager>();
		grid = GetComponent<Grid>();
	}
	
	public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
		StartCoroutine(FindPath(startPos,targetPos));
	}
	
	IEnumerator FindPath(Vector3 startPos, Vector3 targetPos) {

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;
		
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

        // Guard clause in case the target is in an unreachable spot then to not pathfind
        if (startNode.walkable && targetNode.walkable)
        {
            // create sets of possible paths and checked paths
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                        continue;
                    CalculateStep(targetNode, openSet, currentNode, neighbour);
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    private void CalculateStep(Node targetNode, Heap<Node> openSet, Node currentNode, Node neighbour)
    {
        int newMovementCostToNeighbour = currentNode.costToThisNode + GetDistance(currentNode, neighbour);
        if (!(newMovementCostToNeighbour < neighbour.costToThisNode || !openSet.Contains(neighbour)))
            return;

        // Update neighbour with new cost and updating the backtrack list
        neighbour.costToThisNode = newMovementCostToNeighbour;
        neighbour.heuristic = GetDistance(neighbour, targetNode);
        neighbour.parent = currentNode;

        if (!openSet.Contains(neighbour))
            openSet.Add(neighbour);
    }

    // Work way back through the nodes
    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        currentNode = BacktrackUntilStart(startNode, path, currentNode);
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints); // reverse the list of parents from end node
        return waypoints;

    }

    // Retrace step until at start (parent to parent)
    private static Node BacktrackUntilStart(Node startNode, List<Node> path, Node currentNode)
    {
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        return currentNode;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        directionOld = ConvertWaypointsToVector(path, waypoints, directionOld);
        // conversion required for return
        return waypoints.ToArray();
    }

    private static Vector2 ConvertWaypointsToVector(List<Node> path, List<Vector3> waypoints, Vector2 directionOld)
    {
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            // If the path has changed direction we need to add this to list
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }

        return directionOld;
    }

    int GetDistance(Node nodeA, Node nodeB) {
        // Calculate geometric distance between two points
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		
        // Once in line with end point, work out remaining horizontal/vertical moves left
		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}
}
