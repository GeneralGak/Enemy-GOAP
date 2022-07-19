using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CBS_NavMeshPath : CBS_Weighing
{
	[SerializeField] private float distanceToCorner = 0.3f;

	private bool runPathUpdate = true;
	private int currentPathPoint;
	private NavMeshPath currentPath;
	private ContextBasedSteeringBehavior steering;

	public NavMeshPath CurrentPath { get { return currentPath; } }


	public override void SetVariables(ContextBasedSteeringBehavior _steeringBehavior)
	{
		Name = "CBS_NavMeshPath";
		isUsingDanger = false;
		runPathUpdate = true;
		steering = _steeringBehavior;
		currentPath = new NavMeshPath();
		StartCoroutine(UpdatePath(1f));
	}

	public override void WeightUpdate(Vector3 _destination)
	{

	}

	public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
	{
		return Mathf.Max(0, 0.5f + Vector2.Dot(_rayDirection, GetNavMeshPathDirection(_goalDirection).normalized)) / 1.5f;
	}

	public override void CheckDangerCollision(Vector2 _directionToDanger, Vector2 _chosenDirection)
	{
		throw new System.NotImplementedException();
	}

	private IEnumerator UpdatePath(float _updateInterval)
	{
		while (runPathUpdate)
		{
			if (NavMesh.CalculatePath(transform.position, steering.Destination, NavMesh.AllAreas, currentPath))
			{
				currentPathPoint = 0;
			}

			yield return new WaitForSeconds(_updateInterval);
		}
	}

	private Vector2 GetNavMeshPathDirection(Vector2 _goalDirection)
	{
		Vector2 pathDirection;
		if (currentPath.corners.Length == 0 || currentPathPoint > currentPath.corners.Length - 1)
		{
			pathDirection = _goalDirection;
		}
		else
		{
			pathDirection = currentPath.corners[currentPathPoint] - transform.position;

			if (Vector2.Distance(transform.position, currentPath.corners[currentPathPoint]) < distanceToCorner)
			{
				currentPathPoint++;
			}
		}

		return pathDirection;
	}
}
