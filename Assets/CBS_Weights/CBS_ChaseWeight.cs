using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CBS_ChaseWeight : CBS_Weighing
{
	public override void SetVariables(ContextBasedSteeringBehavior _steeringBehavior)
	{
		Name = "CBS_ChaseWeight";
		isUsingDanger = false;
	}

	public override void WeightUpdate(Vector3 _destination)
	{
		
	}

	public override float GetWeight(Vector2 _rayDirections, Vector2 _goalDirection)
	{
		return Mathf.Max(0, 0.5f + Vector2.Dot(_rayDirections, _goalDirection.normalized)) / 1.5f;
	}

	public override void CheckDangerCollision(Vector2 _directionToDanger, Vector2 _chosenDirection)
	{
		throw new System.NotImplementedException();
	}
}
