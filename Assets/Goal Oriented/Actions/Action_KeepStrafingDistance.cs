using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_KeepStrafingDistance : BaseAction
{
    [SerializeField] private float baseCost = 10f;
    [SerializeField, Range(0, 1)] private float chaseLerpDistanceMultiplier = 1f;
    [SerializeField] private float flipStrafeDirRange = 0.4f;

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
        enemy.Animator.SetTrigger("DoWalk");
        enemy.CBS.OnDangerCheck.AddListener(CheckDangersWhileStrafing);
        enemy.Movement.FollowTarget(enemy.Target.transform);

        base.Begin();
    }

    public override void Tick()
    {

    }

    public override void End()
    {
        enemy.Movement.StopMovement();
        enemy.CBS.OnDangerCheck.RemoveListener(CheckDangersWhileStrafing);
        base.End();
    }

    public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        float weight;

        strafeDir = Helper.RotateVectorByAngleRadians(_goalDirection, (strafingClockwise ? -Mathf.PI : Mathf.PI) / 2f);

        float strafeRange = enemy.Stats.attackRange * 0.9f;

        if (enemy.DistanceToTarget < strafeRange)
        {
            float lerpValue = enemy.DistanceToTarget / strafeRange;
            weight = Mathf.Lerp(CBS_WeightHelper.GoAway(_rayDirection, _goalDirection), CBS_WeightHelper.Strafe(_rayDirection, strafeDir), lerpValue);
        }
        else
        {
            float lerpValue = (enemy.DistanceToTarget - strafeRange) / (strafeRange * chaseLerpDistanceMultiplier);
            weight = Mathf.Lerp(CBS_WeightHelper.Strafe(_rayDirection, strafeDir), CBS_WeightHelper.GoTowards(_rayDirection, _goalDirection), lerpValue);
        }

        return weight;
    }

    void CheckDangersWhileStrafing(Vector2 _directionToDanger, Vector2 _chosenDirection)
    {
        float angleBetweenChosenAndDanger = Vector2.Angle(_directionToDanger, _chosenDirection);

        if (_directionToDanger.magnitude < flipStrafeDirRange && angleBetweenChosenAndDanger < 45)
        {
            strafingClockwise = !strafingClockwise;
        }
    }
}
