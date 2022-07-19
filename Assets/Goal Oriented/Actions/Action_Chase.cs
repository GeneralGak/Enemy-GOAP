using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Chase : BaseAction
{
    protected override void Init()
    {

    }

    public override float Cost()
    {
        return 0f;
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
        return CBS_WeightHelper.GoTowards(_rayDirection, _goalDirection);
    }
}
