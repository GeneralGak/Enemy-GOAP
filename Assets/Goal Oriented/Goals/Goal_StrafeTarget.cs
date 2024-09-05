

public class Goal_StrafeTarget : BaseGoal
{
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
