

public class Goal_TestMovement : BaseGoal
{
    public override void PreTick()
    {
        CanRun = true;
        Priority = 100;
    }
}
