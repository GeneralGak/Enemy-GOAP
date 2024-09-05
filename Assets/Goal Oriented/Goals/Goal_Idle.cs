

public class Goal_Idle : BaseGoal
{
    public override void PreTick()
    {
        CanRun = true;
        Priority = 0;        
    }
}
