using UnityEngine;

public class CBS_StrafeWeight : CBS_Weighing
{
	[SerializeField] private float flipStrafeDirRange = 0.4f;

	private bool strafingClockwise;
	private Vector2 strafeDir;


	public override void SetVariables(ContextBasedSteeringBehavior _steeringBehavior)
	{
		Name = "CBS_StrafeWeight";
		isUsingDanger = true;

		if (Random.Range(0, 2) == 0)
		{
			strafingClockwise = true;
		}
		else
		{
			strafingClockwise = false;
		}
	}

	public override void WeightUpdate(Vector3 _destination)
	{
		strafeDir = Helper.RotateVectorByAngleRadians(_destination - transform.position, (strafingClockwise ? -Mathf.PI : Mathf.PI) / 2f);
	}

	public override float GetWeight(Vector2 _rayDirections, Vector2 _goalDirection)
	{
		return Vector2.Dot(_rayDirections, strafeDir.normalized);
	}

	public override void CheckDangerCollision(Vector2 _directionToDanger, Vector2 _chosenDirection)
	{
		float angleBetweenChosenAndDanger = Vector2.Angle(_directionToDanger, _chosenDirection);

		if (_directionToDanger.magnitude < flipStrafeDirRange && angleBetweenChosenAndDanger < 45)
		{
			strafingClockwise = !strafingClockwise;
		}
	}
}
