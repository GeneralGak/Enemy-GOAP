using UnityEngine;

public class Goal_TargetTooClose : BaseGoal
{
    [SerializeField] private float fleeRange = 2f;

    public override void PreTick()
    {
        if (enemy.DistanceToTarget >= 0)
        {
            CanRun = enemy.DistanceToTarget <= fleeRange;
        }
        else
        {
            CanRun = false;
        }

        Priority = basePriority;
    }
}
