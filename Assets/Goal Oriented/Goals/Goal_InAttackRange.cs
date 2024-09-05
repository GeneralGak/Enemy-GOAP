

public class Goal_InAttackRange : BaseGoal
{
    public override void PreTick()
    {
        if(enemy.DistanceToTarget >= 0)
		{
            CanRun = enemy.DistanceToTarget <= enemy.Stats.attackRange;
        }
        else
		{
            CanRun = false;
		}

        Priority = basePriority;
    }
}
