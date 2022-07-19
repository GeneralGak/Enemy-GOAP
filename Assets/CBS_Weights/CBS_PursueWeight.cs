using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBS_PursueWeight : CBS_Weighing
{
	private ContextBasedSteeringBehavior steeringBehavior;
	private Vector2 furturePosition;

	public override void SetVariables(ContextBasedSteeringBehavior _steeringBehavior)
	{
		Name = "CBS_PursueWeight";
		isUsingDanger = false;
		steeringBehavior = _steeringBehavior;
	}

	public override void WeightUpdate(Vector3 _destination)
	{
		float distance = Vector2.Distance(gameObject.transform.position, _destination);
		float pridictionInfluence = distance / 10;
		furturePosition = _destination + gameObject.transform.position * pridictionInfluence;
	}

	public override float GetWeight(Vector2 _rayDirections, Vector2 _goalDirection)
	{
		return Mathf.Max(0, 0.5f + Vector2.Dot(_rayDirections, furturePosition.normalized)) / 1.5f;
	}

	public override void CheckDangerCollision(Vector2 _directionToDanger, Vector2 _chosenDirection)
	{
		throw new System.NotImplementedException();
	}
}
