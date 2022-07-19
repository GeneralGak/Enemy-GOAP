using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CBS_Bouncing : CBS_Weighing
{
	private ContextBasedSteeringBehavior behavior;
	private Vector2 newDirection;
	private Vector2 lastRay;
	private bool changeDirection;


	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
		{
			Vector2 normal = ((Vector2)transform.position - collision.collider.ClosestPoint(transform.position)).normalized;
			newDirection = Vector2.Reflect(behavior.Velocity.normalized, normal);
			changeDirection = true;
		}
	}

	public override void SetVariables(ContextBasedSteeringBehavior _steeringBehavior)
	{
		Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

		newDirection = randomDirection;
		behavior = _steeringBehavior;

		Vector2[] tmpRay = behavior.GetRays(out float[] interests);
		lastRay = tmpRay[behavior.RayCount - 1];

		Name = "CBS_Bouncing";
		isUsingDanger = false;
	}

	public override void WeightUpdate(Vector3 _destination)
	{

	}

	public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
	{
		if(changeDirection)
		{
			_goalDirection = newDirection;
			float checkMoveDirection = Vector2.Angle(newDirection, behavior.Velocity);

			if(checkMoveDirection <= 0.1f)
			{
				changeDirection = false;
			}
		}
		else
		{
			_goalDirection = behavior.Velocity;
		}

		return Mathf.Max(0, 0.5f + Vector2.Dot(_rayDirection, _goalDirection.normalized)) / 1.5f;
	}

	public override void CheckDangerCollision(Vector2 _directionToDanger, Vector2 _chosenDirection)
	{
		throw new System.NotImplementedException();
	}
}
