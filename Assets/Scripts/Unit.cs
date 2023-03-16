using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {


	public Transform target;
	float speed = 20;
	Vector3[] path;
	int targetIndex;

	public float thrust;
	public float maxSpeed;
	public float rotSpeed;
	public float maxRotStep;
	private Vector2 movement;

	void Start()
	{
		PathRequestManager.RequestPath(transform.position,target.position, OnPathFound);
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
	{
		if (pathSuccessful) {
			path = newPath;
			targetIndex = 0;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath()
	{
		Vector3 currentWaypoint = path[0];
		while (true) 
		{
			if (transform.position == currentWaypoint) 
			{
				targetIndex ++;
				if (targetIndex >= path.Length) 
					yield break;
				currentWaypoint = path[targetIndex];
			}
            LookForward(currentWaypoint);
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
			yield return null;
		}
	}

	public void LookForward(Vector3 currentWaypoint)
    {
		Vector2 shipPos = transform.position;
		Vector2 shipDirection = new Vector2(transform.forward.x, transform.forward.z);
		Vector2 waypoint = new Vector2(currentWaypoint.x, currentWaypoint.z);
		Vector2 targetDirection = waypoint - shipPos;

		float angle = Vector2.SignedAngle(shipDirection, targetDirection);
		float step = Mathf.Clamp(angle * rotSpeed, -maxRotStep, maxRotStep) * Time.fixedDeltaTime;
		transform.RotateAround(transform.position, Vector3.down, step);
	}
}
