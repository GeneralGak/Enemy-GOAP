using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBS_Wander : CBS_Weighing
{
	[SerializeField] private float wanderCircleRadius;

	private ContextBasedSteeringBehavior behavior;
	private const float CIRCLE_DISTANCE = 1.5f;
	private const float ANGLE_CHANGE = 0.12f;
	private float wanderAngle;

	public Vector2 WanderDirection { get; private set; }
	public Vector2 displacement { get; private set; }
	public float WanderCircleRadius { get { return wanderCircleRadius; } }


	public override void SetVariables(ContextBasedSteeringBehavior _steeringBehavior)
	{
		Name = "CBS_Wander";
		isUsingDanger = false;

		behavior = _steeringBehavior;
	}

	public override void WeightUpdate(Vector3 _destination)
	{
		WanderDirection = behavior.DesiredVelocity;
		WanderDirection.Normalize();
		WanderDirection *= CIRCLE_DISTANCE;

		displacement = new Vector2(0, -1);
		displacement *= wanderCircleRadius;

		wanderAngle += Random.value * ANGLE_CHANGE - ANGLE_CHANGE * 0.5f;

		float vectorLenght = displacement.magnitude;
		displacement = new Vector2(Mathf.Cos(wanderAngle) * vectorLenght, Mathf.Sin(wanderAngle) * vectorLenght);

		WanderDirection += displacement;
	}

	public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
	{
		return Mathf.Max(0, 0.5f + Vector2.Dot(_rayDirection, WanderDirection.normalized)) / 1.5f;
	}

	public override void CheckDangerCollision(Vector2 _directionToDanger, Vector2 _chosenDirection)
	{
		
	}
}
