using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_Chase : BaseGoal
{
    [SerializeField] private int BasePriority = 30;

    public override void PreTick()
    {
        CanRun = LinkedAIState.Target != null;

        Priority = BasePriority;
    }
}
