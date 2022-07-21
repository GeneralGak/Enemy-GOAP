using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class ContextBasedSteeringBehavior : MonoBehaviour
{
	[SerializeField] private int rayCount;
	[SerializeField] private float detectionRange;
	[SerializeField] private LayerMask dangerLayerMask;
	[SerializeField] private LayerMask layersToMoveAwayFrom;
	
	[SerializeField] private float maxSpeed;
	[SerializeField, Range(0, 1)] private float steerForce;

	private Vector2[] rayDirections;
	private float[] interest;
	private float[] danger;
	private Vector2 chosenDirection;
	private Vector2 velocity;
	private Vector2 targetVelocity;
	private List<Vector2> directionsToDangers = new List<Vector2>();

	public BaseAction CurrentAction { get; set; }

	public Vector2 Velocity { get { return velocity; } }
	public Vector2 DesiredVelocity { get { return targetVelocity; } }
	public int CurrentUsedRayNumber { get; private set; } = 0;
	public int RayCount { get { return rayCount; } }
	public UnityEvent<Vector2, Vector2> OnDangerCheck { get; set; } = new UnityEvent<Vector2, Vector2>();


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

	private void SetInterest(Vector2 _targetDir)
	{
		for (int i = 0; i < rayCount; i++)
		{
			interest[i] = CurrentAction.GetWeight(rayDirections[i], _targetDir);
		}
	}

	private void SetDanger()
	{
		Collider2D[] dangers = Physics2D.OverlapCircleAll(transform.position, detectionRange, dangerLayerMask);

		List<Collider2D> nonDuplicateDangers = new List<Collider2D>();

		foreach (Collider2D dangerCollider in dangers)
		{
			if (dangerCollider.gameObject != gameObject && !nonDuplicateDangers.Contains(dangerCollider))
			{
				nonDuplicateDangers.Add(dangerCollider);
			}
		}

		directionsToDangers.Clear();

		foreach (Collider2D dangerCollider in nonDuplicateDangers)
		{
			Vector2 dirToDanger = dangerCollider.ClosestPoint(transform.position) - (Vector2)transform.position;
			float weight = dirToDanger.magnitude > detectionRange ? 0 : (1 - dirToDanger.magnitude / detectionRange) * 2;
			weight = Mathf.Clamp(weight, 0, 1);

			directionsToDangers.Add(dirToDanger);

			for (int i = 0; i < rayCount; i++)
			{
				float dotProduct = Vector2.Dot(rayDirections[i], dirToDanger.normalized);

				if (dangerCollider.gameObject.layer == layersToMoveAwayFrom)
				{
					dotProduct = 1 - Mathf.Abs(dotProduct - 0.65f);
				}

				danger[i] += dotProduct * weight;
				if (danger[i] > 1) { danger[i] = 1; }
			}
		}
	}

	private void ChooseDirection()
	{
		chosenDirection = Vector2.zero;

		for (int i = 0; i < rayCount; i++)
		{
			if (danger[i] > 0)
			{
				interest[i] -= danger[i];
			}

			chosenDirection += rayDirections[i] * interest[i];
		}

		chosenDirection.Normalize();
	}

	public Vector2[] GetRays(out float[] _interest)
	{
		_interest = interest;

		return rayDirections;
	}
}