

public class Goal_Attack : BaseGoal
{
    public override void PreTick()
    {
        CanRun = enemy.Target != null && enemy.DistanceToTarget < enemy.Stats.attackRange;

        Priority = basePriority;
    }
}
