

public class Action_Idle : BaseAction
{
    public override void Init()
    {

    }

    public override float Cost()
    {
        return 0f;
    }

    public override void Begin()
    {
        enemy.Animator.SetTrigger("DoIdle");

        base.Begin();
    }

    public override void Tick()
    {

    }

    public override void End()
    {
        base.End();
    }
}
