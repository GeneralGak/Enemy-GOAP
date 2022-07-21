using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_StrafeTarget : BaseGoal
{
    [SerializeField] private int basePriority = 35;

    public override void PreTick()
    {
        if(enemy.Target)
		{
            CanRun = enemy.DistanceToTarget <= enemy.Stats.attackRange * 2f;
        }
        else
		{
            CanRun = false;
		}

        Priority = basePriority;
    }
}
