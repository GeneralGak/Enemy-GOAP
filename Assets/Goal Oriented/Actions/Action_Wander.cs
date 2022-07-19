using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Wander : BaseAction
{
    [SerializeField] private float wanderCircleRadius;
    [SerializeField, Range(0, 1)] private float lerpDistance = 0.5f;

    private const float CIRCLE_DISTANCE = 1.5f;
    private const float ANGLE_CHANGE = 0.08f;
    private float wanderAngle;
    private Vector2 displacement;
    private Vector2 spawnPos;

	public override float Cost()
	{
		return 0f;
	}

    protected override void Init()
    {
        spawnPos = transform.position;
    }

    public override void Begin()
    {
        Navigation.StartMovement();
        Navigation.Destination = spawnPos;

        base.Begin();
    }

    public override void Tick()
    {
        
    }

    public override void End()
    {
        base.End();
    }

    private Vector2 CalculateWanderDir()
    {
        Vector2 wanderDir = Navigation.Velocity;
        wanderDir.Normalize();
        wanderDir *= CIRCLE_DISTANCE;

        displacement = new Vector2(0, -1);
        displacement *= wanderCircleRadius;

        wanderAngle += Random.value * ANGLE_CHANGE - ANGLE_CHANGE * 0.5f;

        float vectorLength = displacement.magnitude;
        displacement = new Vector2(Mathf.Cos(wanderAngle) * vectorLength, Mathf.Sin(wanderAngle) * vectorLength);

        wanderDir += displacement;
        return wanderDir;
    }

    public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        float lerpValue = (Vector2.Distance(spawnPos, transform.position) - wanderCircleRadius * lerpDistance) / wanderCircleRadius * lerpDistance;
        lerpValue = Mathf.Clamp(lerpValue, 0, 1);
        Vector2 goalDir = Vector2.Lerp(CalculateWanderDir(), _goalDirection, lerpValue);
        return CBS_WeightHelper.GoTowards(_rayDirection, goalDir);
    }
}
