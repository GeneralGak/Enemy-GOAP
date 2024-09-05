using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AISteering : MonoBehaviour
{
	[SerializeField] private float distanceToCorner = 0.3f;

	private int currentPathPoint;
	private Vector3[] currentCornersUsed;
	private Vector2 pathDestination;
	private Vector2 displacement;
	private const float CIRCLE_DISTANCE = 1.1f;
	private NavMeshPath currentPath;
	private NavMeshPath newPath;

	public bool RunPathUpdate { get; set; }
	public float WanderAngle { get; set; }
	public ContextBasedSteeringBehavior CBS { get; private set; }
	public NavMeshPath NavMeshPathUsed { get { return currentPath; } }
	public Vector2 PathDestination { get { return pathDestination; } }

	private void Awake()
	{
		CBS = GetComponent<ContextBasedSteeringBehavior>();
		currentPath = new NavMeshPath();
		newPath = new NavMeshPath();
	}

	public Vector2 CalculateWanderDir(Vector2 _moveDir, float _wanderCircleRadius, float _angleChange)
	{
		if (_moveDir == Vector2.zero)
		{
			_moveDir = Vector2.left;
		}

		_moveDir.Normalize();
		_moveDir *= CIRCLE_DISTANCE;

		displacement = new Vector2(0, -1);
		displacement *= _wanderCircleRadius;

		float vectorLength = displacement.magnitude;
		displacement = new Vector2(Mathf.Cos(WanderAngle) * vectorLength, Mathf.Sin(WanderAngle) * vectorLength);

		float angleChangeDirection = Random.value;

		WanderAngle += (angleChangeDirection * _angleChange) - (_angleChange * 0.5f);

		Vector2 newMoveDirection = _moveDir + displacement;
		return newMoveDirection;
	}

	public void ManuallyUpdatePath(Vector2 _endGoalPosition)
	{
		NavMeshHit hit;

		if (NavMesh.SamplePosition(_endGoalPosition, out hit, 0.1f, NavMesh.AllAreas))
		{
			NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, newPath);
			pathDestination = hit.position;
		}
	}

	public void ManuallyUpdatePath(NavMeshPath _newPath)
	{
		if (_newPath.status != NavMeshPathStatus.PathInvalid && _newPath.corners.Length > 0)
		{
			newPath = _newPath;
			if (_newPath.corners.Length == 1)
			{
				pathDestination = _newPath.corners[0];
			}
			else pathDestination = _newPath.corners[_newPath.corners.Length - 1];
		}
	}

	public IEnumerator UpdatePath(float _updateInterval, Vector2 _endGoalPosition)
	{
		NavMeshHit hit;
		pathDestination = _endGoalPosition;

		while (RunPathUpdate)
		{
			if (NavMesh.SamplePosition(_endGoalPosition, out hit, 0.1f, NavMesh.AllAreas))
			{
				NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, newPath);
			}

			yield return new WaitForSeconds(_updateInterval);
		}
	}

	public IEnumerator UpdatePath(float _updateInterval, Transform _endGoaltransform)
	{
		NavMeshHit hit;

		while (RunPathUpdate)
		{
			pathDestination = _endGoaltransform.position;
			if (NavMesh.SamplePosition(_endGoaltransform.position, out hit, 0.1f, NavMesh.AllAreas))
			{
				NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, newPath);
			}

			yield return new WaitForSeconds(_updateInterval);
		}
	}

	public Vector2 GetNavMeshMoveToPosition()
	{
		if (newPath.status != NavMeshPathStatus.PathInvalid)
		{
			currentPath = newPath;
			currentPathPoint = 1;

			//for (int i = 0; i < currentPath.corners.Length; i++)
			//{
			//	if (Vector2.Distance(transform.position, currentPath.corners[i]) > distanceToCorner)
			//	{
			//		currentPathPoint = i;
			//		break;
			//	}
			//}
			//currentCornersUsed = currentPath.corners;
		}

		Vector2 moveToPosition;
		if (currentPath.corners.Length == 0 || currentPathPoint == currentPath.corners.Length - 1)
		{
			if (currentPath.status == NavMeshPathStatus.PathPartial) moveToPosition = currentPath.corners[currentPath.corners.Length - 1];
			else moveToPosition = pathDestination;
		}
		else
		{
			if (currentPathPoint > currentPath.corners.Length - 1) currentPathPoint = currentPath.corners.Length - 1; // Added to fix bug: currentPathPoint = 3 and corners.length = 2

			if (Vector2.Distance(transform.position, currentPath.corners[currentPathPoint]) < distanceToCorner)
			{
				currentPathPoint++;
			}

			moveToPosition = currentPath.corners[currentPathPoint];
		}

		return moveToPosition;
	}
}
