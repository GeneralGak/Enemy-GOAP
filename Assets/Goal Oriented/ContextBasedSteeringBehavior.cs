using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContextBasedSteeringBehavior : MonoBehaviour
{
	[SerializeField] private int rayCount;
	[SerializeField] private float detectionRange;
	[SerializeField] private float agentColliderSize = 0.2f;
	[SerializeField] private LayerMask dangerLayerMask;
	[SerializeField] private LayerMask layersToMoveAwayFrom;
	[SerializeField] private bool showObstacleGizmo;
	[SerializeField] private bool showInterestGizmo;

	private Vector2 chosenDirection;
	private Vector2[] rayDirections;
	private float[] interest;
	private float[] danger;
	private List<Vector2> directionsToDangers = new List<Vector2>();

	public BaseAction CurrentAction { get; set; }
	public int CurrentUsedRayNumber { get; private set; } = 0;
	public int RayCount { get { return rayCount; } }
	public UnityEvent<Vector2, Vector2> OnDangerCheck { get; set; } = new UnityEvent<Vector2, Vector2>();
	public float DetectionRange { get { return detectionRange; } set { detectionRange = value; } }
	public float AgentColliderSize { get { return agentColliderSize; } set { agentColliderSize = value; } }


	protected void Awake()
	{
		interest = new float[rayCount];
		danger = new float[rayCount];
		rayDirections = new Vector2[rayCount];

		for (int i = 0; i < rayCount; i++)
		{
			float angle = i * 2 * Mathf.PI / rayCount;
			rayDirections[i] = Helper.RotateVectorByAngleRadians(Vector2.right, angle);
		}

		chosenDirection = rayDirections[CurrentUsedRayNumber];
	}

	public Vector2 GetDir(Vector2 _targetDir)
	{
		for (int i = 0; i < rayCount; i++)
		{
			interest[i] = 0;
			danger[i] = 0;
		}

		SetDanger();
		SetInterest(_targetDir);
		ChooseDirection();

		foreach (Vector2 dirToDanger in directionsToDangers)
		{
			OnDangerCheck?.Invoke(dirToDanger, chosenDirection);
		}

		return chosenDirection.normalized;
	}

	public Vector2 GetDir(Vector2 _targetDir, Func<Vector2, Vector2, float> _getWeight)
	{
		for (int i = 0; i < rayCount; i++)
		{
			interest[i] = 0;
			danger[i] = 0;
		}

		SetDanger();
		SetInterest(_targetDir, _getWeight);
		ChooseDirection();

		foreach (Vector2 dirToDanger in directionsToDangers)
		{
			OnDangerCheck?.Invoke(dirToDanger, chosenDirection);
		}

		return chosenDirection.normalized;
	}

	private void SetInterest(Vector2 _targetDir)
	{
		for (int i = 0; i < rayCount; i++)
		{
			float weightResult;
			if (CurrentAction == null) weightResult = CBS_WeightHelper.GoTowards(rayDirections[i], _targetDir);
			else weightResult = CurrentAction.GetWeight(rayDirections[i], _targetDir);

			if (weightResult > 0 && weightResult > interest[i])
			{
				interest[i] = weightResult;
			}
		}
	}

	private void SetInterest(Vector2 _targetDir, Func<Vector2, Vector2, float> _getWeight)
	{
		for (int i = 0; i < rayCount; i++)
		{
			interest[i] = _getWeight(rayDirections[i], _targetDir);
		}
	}

	private void SetDanger()
	{
		Collider2D[] dangers = Physics2D.OverlapCircleAll(transform.position, detectionRange, dangerLayerMask);

		List<Collider2D> nonDuplicateDangers = new List<Collider2D>();

		foreach (Collider2D dangerCollider in dangers)
		{
			if (dangerCollider.transform.root.gameObject != gameObject && !nonDuplicateDangers.Contains(dangerCollider) && !dangerCollider.isTrigger)
			{
				nonDuplicateDangers.Add(dangerCollider);
			}
		}

		dangers = Physics2D.OverlapCircleAll(transform.position, detectionRange, layersToMoveAwayFrom);

		foreach (Collider2D dangerCollider in dangers)
		{
			if (dangerCollider.attachedRigidbody != null && dangerCollider.attachedRigidbody.gameObject == gameObject) continue;

			if (dangerCollider.transform.root.gameObject != gameObject && !nonDuplicateDangers.Contains(dangerCollider) && !dangerCollider.isTrigger)
			{
				nonDuplicateDangers.Add(dangerCollider);
			}
		}

		directionsToDangers.Clear();

		foreach (Collider2D dangerCollider in nonDuplicateDangers)
		{
			Vector2 dirToDanger = dangerCollider.ClosestPoint(transform.position) - (Vector2)transform.position;

			// Calculate weight based on the distance
			float weight;
			if (dangerCollider.gameObject.layer == 1 << layersToMoveAwayFrom.value) weight = 1;
			else weight = dirToDanger.magnitude <= agentColliderSize ? 1 : (detectionRange - dirToDanger.magnitude) / detectionRange;
			//weight = Mathf.Clamp(weight, 0, 1);

			directionsToDangers.Add(dirToDanger);

			for (int i = 0; i < rayCount; i++)
			{
				float dotProduct = Vector2.Dot(dirToDanger.normalized, rayDirections[i]);

				float newDangerValue = dotProduct * weight;
				//if (dangerCollider.gameObject.layer == layersToMoveAwayFrom)
				//{
				//	dotProduct = 1 - Mathf.Abs(dotProduct - 0.65f); // Enemy avoidance
				//}

				if (newDangerValue > danger[i])
				{
					danger[i] = newDangerValue;
				}
				//danger[i] += dotProduct * weight;
				//if (danger[i] > 1) { danger[i] = 1; }
			}
		}
	}

	private void ChooseDirection()
	{
		chosenDirection = Vector2.zero;

		for (int i = 0; i < rayCount; i++)
		{
			interest[i] = Mathf.Clamp01(interest[i] - danger[i]);

			//if (danger[i] > 0)
			//{
			//	interest[i] -= danger[i];
			//}

			chosenDirection += rayDirections[i] * interest[i];
		}

		chosenDirection.Normalize();
	}

	/// <summary>
	/// Used for CBS visualizer
	/// </summary>
	public Vector2[] GetRays(out float[] _interest)
	{
		_interest = interest;

		return rayDirections;
	}

	public void ShowGizmos(bool _showObstacleGizmo, bool _showInterestGizmo)
	{
		showInterestGizmo = _showInterestGizmo;
		showObstacleGizmo = _showObstacleGizmo;
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			if (showObstacleGizmo && danger != null)
			{
				Gizmos.color = Color.red;

				for (int i = 0; i < danger.Length; i++)
				{
					Gizmos.DrawRay(transform.position, rayDirections[i] * danger[i]);
				}
			}

			if (showInterestGizmo && interest != null)
			{
				Gizmos.color = Color.green;
				for (int i = 0; i < interest.Length; i++)
				{
					Gizmos.DrawRay(transform.position, rayDirections[i] * interest[i]);
				}

				Gizmos.color = Color.yellow;
				Gizmos.DrawRay(transform.position, chosenDirection * 1);
			}
		}
	}
}