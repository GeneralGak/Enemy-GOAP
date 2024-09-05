

public class Goal_Chase : BaseGoal
{
    public override void PreTick()
    {
        CanRun = enemy.Target != null;

        Priority = basePriority;
    }
}
