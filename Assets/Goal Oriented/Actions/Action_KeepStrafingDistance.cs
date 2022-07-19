using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_KeepStrafingDistance : BaseAction
{
    [SerializeField] private float baseCost = 10f;
    [SerializeField, Range(0, 1)] private float chaseLerpDistanceMultiplier = 1f;

    private bool strafingClockwise;
    private Vector2 strafeDir;

    protected override void Init()
    {
        if (Random.Range(0, 2) == 0)
        {
            strafingClockwise = true;
        }
        else
        {
            strafingClockwise = false;
        }
    }

    public override float Cost()
    {
        return baseCost;
    }

    public override void Begin()
    {
        Navigation.StartMovement();
        
        base.Begin();
    }

    public override void Tick()
    {
        Navigation.Destination = enemy.Vision.Target.transform.position;
    }

    public override void End()
    {
        base.End();
    }

    public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        float weight;

        strafeDir = Helper.RotateVectorByAngleRadians(_goalDirection, (strafingClockwise ? -Mathf.PI : Mathf.PI) / 2f);
        
        if(enemy.DistanceToTarget < enemy.Stats.attackRange)
        {
            float lerpValue = enemy.DistanceToTarget / enemy.Stats.attackRange;
            lerpValue = Mathf.Clamp(lerpValue, 0, 1);
            weight = Mathf.Lerp(CBS_WeightHelper.GoAway(_rayDirection, _goalDirection), CBS_WeightHelper.Strafe(_rayDirection, strafeDir), lerpValue);
        }
        else
        {
            float lerpValue = (enemy.DistanceToTarget - enemy.Stats.attackRange) / (enemy.Stats.attackRange * chaseLerpDistanceMultiplier);
            lerpValue = Mathf.Clamp(lerpValue, 0, 1);
            weight = Mathf.Lerp(CBS_WeightHelper.Strafe(_rayDirection, strafeDir), CBS_WeightHelper.GoTowards(_rayDirection, _goalDirection), lerpValue);
        }

        return weight;
    }
}
