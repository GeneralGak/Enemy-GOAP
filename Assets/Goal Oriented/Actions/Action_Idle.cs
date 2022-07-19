using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Idle : BaseAction
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
        Navigation.StopMovement();
        enemy.Animator.SetTrigger("DoIdle");

        base.Begin();
    }

    public override void Tick()
    {

    }

    public override void End()
    {
        base.End();
    }
}
