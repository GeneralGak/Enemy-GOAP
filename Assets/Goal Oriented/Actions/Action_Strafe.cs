using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Strafe : BaseAction
{
    [SerializeField] private float baseCost = 10f;

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
        enemy.Animator.SetTrigger("DoWalk");

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
        strafeDir = Helper.RotateVectorByAngleRadians(_goalDirection, (strafingClockwise ? -Mathf.PI : Mathf.PI) / 2f);
        return CBS_WeightHelper.Strafe(_rayDirection, strafeDir);
    }
}
