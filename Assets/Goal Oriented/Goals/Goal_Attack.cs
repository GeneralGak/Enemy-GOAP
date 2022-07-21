using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_Attack : BaseGoal
{
    [SerializeField] private int BasePriority = 50;

    public override void PreTick()
    {
        CanRun = enemy.Target != null && enemy.DistanceToTarget < enemy.Stats.attackRange;
        Priority = BasePriority;
    }

    public override void Activate()
    {
        CommitTo = true;
        base.Activate();
    }

    public override void Deactivate()
    {
        CommitTo = false;
        base.Deactivate();
    }
}
