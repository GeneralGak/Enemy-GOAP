using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ContextBasedWeights
{
	None,
	CBS_ChaseWeight,
	CBS_FleeWeight,
	CBS_StrafeWeight,
	CBS_Wander,
	CBS_Bouncing,
	CBS_NavMeshPath
}

public class ContextBasedSteeringBehavior : MonoBehaviour
{
	[SerializeField] private bool useTransformGoal;
	[SerializeField] [ShowIf(nameof(useTransformGoal))] private Transform transformGoal;
	[SerializeField] private Vector3 vectorGoal;
	[SerializeField] private CBS_Weighing lowLerpWeight;
	[SerializeField] private LayerMask dangerLayerMask;
	[SerializeField] private LayerMask layersToMoveAwayFrom;
	[SerializeField] private float maxSpeed;
	[SerializeField] private float steerForce;
	[SerializeField] private int rayCount;
	[SerializeField] private float detectionRange;
	[SerializeField] private bool useDistanceFromDestinationToLerp;
	[SerializeField][HideIf(nameof(useDistanceFromDestinationToLerp))] private float transitionRadius = 0.2f;
	[SerializeField][HideIf(nameof(useDistanceFromDestinationToLerp))] private float maxDistanceRange = 0.8f;
	[SerializeField][ShowIf(nameof(useDistanceFromDestinationToLerp))] private float maxLerpDistance;

	private Vector2[] rayDirections;
	private float[] interest;
	private float[] danger;
	private CBS_Weighing[] steeringWeights;
	private Vector2 chosenDirection;
	private CBS_Weighing highLerpWeight;
	private Vector2 velocity;
	private Vector2 desiredVelocity;
	private float randomDistanceAdder;
	private float setDetectionRange;
	private Rigidbody2D objectRigidbody;
	private bool stopMoving;

	public Transform TransformGoal { get { return transformGoal; } set { transformGoal = value; } }
	public Vector3 VectorGoal { get { return vectorGoal; } set { vectorGoal = value; } }
	public Vector3 Destination { get; private set; }
	public Vector2 Velocity { get { return velocity; } }
	public Vector2 DesiredVelocity { get { return desiredVelocity; } }
	public int CurrentUsedRayNumber { get; private set; } = 0;
	public int RayCount { get { return rayCount; } }
	public float DetectionRange { get { return detectionRange; } }
	public float TransitionRadius { get { return transitionRadius; } }
	public float RandomDistanceAdder { get { return randomDistanceAdder; } }
	public float MaxLerpDistance { get { return maxLerpDistance; } set { maxLerpDistance = value; } }
	public bool UseTransformGoal { get { return useTransformGoal; } set { useTransformGoal = value; } }
	public bool UseDistanceFromPositionToLerp { get { return useDistanceFromDestinationToLerp; } }
	public float MaxDistanceRange 
	{ 
		get { return maxDistanceRange; } 
		set
		{
			if(value < 0)
			{
				maxDistanceRange = 0;
			}
			else
			{
				maxDistanceRange = value;
			}
		}
	}
	public float MaxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }
	public float SteerForce { get { return steerForce; } set { steerForce = value; } }


	protected void Awake()
	{
		steeringWeights = GetComponents<CBS_Weighing>();
		objectRigidbody = GetComponent<Rigidbody2D>();
		
		setDetectionRange = detectionRange;
		interest = new float[rayCount];
		danger = new float[rayCount];
		rayDirections = new Vector2[rayCount];

		for (int i = 0; i < rayCount; i++)
		{
			float angle = i * 2 * Mathf.PI / rayCount;
			rayDirections[i] = Helper.RotateVectorByAngleRadians(Vector2.right, angle);
		}

		chosenDirection = rayDirections[CurrentUsedRayNumber];

		foreach (CBS_Weighing weights in steeringWeights)
		{
			weights.SetVariables(this);
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < rayCount; i++)
		{
			interest[i] = 0;
			danger[i] = 0;
		}

		if (lowLerpWeight && (!useTransformGoal && VectorGoal != Vector3.zero || useTransformGoal && transformGoal != null))
		{
			SetDanger();
			SetInterest();
			ChooseDirection();

			desiredVelocity = chosenDirection.normalized * maxSpeed;

			if (!stopMoving)
			{
				velocity = Vector2.Lerp(velocity, desiredVelocity, steerForce);
			}
			else
			{
				velocity = Vector2.zero;
			}

			objectRigidbody.MovePosition(objectRigidbody.position + velocity * Time.fixedDeltaTime);
		}
	}

	private void SetInterest()
	{
		float lerpValue = 0;
		Vector2 goalDirection = Vector2.zero;
		Destination = Vector3.zero;

		if (useTransformGoal)
		{
			goalDirection = transformGoal.position - transform.position;
			Destination = transformGoal.position;
		}
		else
		{
			goalDirection = vectorGoal - transform.position;
			Destination = vectorGoal;
		}
		
		lowLerpWeight.WeightUpdate(Destination);

		if (highLerpWeight != null)
		{
			highLerpWeight.WeightUpdate(Destination);
		}

		if(useDistanceFromDestinationToLerp)
		{
			lerpValue = Mathf.Min(1, Mathf.Max(0, Vector2.Distance(transform.position, Destination) - maxLerpDistance) / transitionRadius);
		}
		else if(maxDistanceRange > 0)
		{
			lerpValue = Mathf.Min(1, Mathf.Max(0, goalDirection.magnitude - maxDistanceRange + randomDistanceAdder) / transitionRadius);
		}

		for (int i = 0; i < rayCount; i++)
		{
			float weight = 0;

			if (highLerpWeight != null)
			{
				weight = Mathf.Lerp(highLerpWeight.GetWeight(rayDirections[i], goalDirection), lowLerpWeight.GetWeight(rayDirections[i], goalDirection), lerpValue);
			}
			else
			{
				weight = lowLerpWeight.GetWeight(rayDirections[i], goalDirection);
			}

			interest[i] = weight;
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

		foreach (Collider2D dangerCollider in nonDuplicateDangers)
		{
			Vector2 dirToDanger = dangerCollider.ClosestPoint(transform.position) - (Vector2)transform.position;
			float weight = dirToDanger.magnitude > detectionRange ? 0 : (1 - dirToDanger.magnitude / detectionRange) * 2;
			weight = Mathf.Clamp(weight, 0, 1);

			for (int i = 0; i < steeringWeights.Length; i++)
			{
				if(steeringWeights[i].IsUsingDanger)
				{
					steeringWeights[i].CheckDangerCollision(dirToDanger, chosenDirection);
				}
			}

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

	public void SetRandomStrafingRange(float _minRange, float _maxRange)
	{
		randomDistanceAdder = Random.Range(_minRange, _maxRange);
	}

	public void IsDetectionRangeDisabled(bool _isDisabled)
	{
		if(_isDisabled)
		{
			detectionRange = 0;
		}
		else
		{
			detectionRange = setDetectionRange;
		}
	}

	public void SetNewWeights(ContextBasedWeights _fromWeightType)
	{
		highLerpWeight = null;
		
		for (int i = 0; i < steeringWeights.Length; i++)
		{
			if(steeringWeights[i].Name == _fromWeightType.ToString())
			{
				lowLerpWeight = steeringWeights[i];
			}
		}
	}

	public void SetNewWeights(ContextBasedWeights _fromWeightType, ContextBasedWeights _toWeightType)
	{
		for (int i = 0; i < steeringWeights.Length; i++)
		{
			if (steeringWeights[i].Name == _fromWeightType.ToString())
			{
				lowLerpWeight = steeringWeights[i];
			}
			else if (steeringWeights[i].Name == _toWeightType.ToString())
			{
				highLerpWeight = steeringWeights[i];
			}
		}
	}

	public Vector2[] GetRays(out float[] _interest)
	{
		_interest = interest;

		return rayDirections;
	}

	public CBS_Weighing GetSteeringWeightScript(ContextBasedWeights _weightScriptType)
	{
		for (int i = 0; i < steeringWeights.Length; i++)
		{
			if (steeringWeights[i].Name == _weightScriptType.ToString())
			{
				return steeringWeights[i];
			}
		}

		return null;
	}

	public void StopMovement()
	{
		stopMoving = true;
	}

	public void StartMovement()
	{
		stopMoving = false;
	}

	public void ResetSteeringBehavior()
	{
		IsDetectionRangeDisabled(false);
		stopMoving = false;
		TransformGoal = null;
		VectorGoal = Vector3.zero;
		chosenDirection = Vector3.zero;
		lowLerpWeight = null;
		highLerpWeight = null;
	}
}
