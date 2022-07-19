using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_InAttackRange : BaseGoal
{
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private int basePriority = 35;

    public override void PreTick()
    {
        if(LinkedAIState.DistanceToTarget >= 0)
		{
            CanRun = LinkedAIState.DistanceToTarget <= attackRange;
        }
        else
		{
            CanRun = false;
		}

        Priority = basePriority;
    }
}
