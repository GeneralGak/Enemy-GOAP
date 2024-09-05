using UnityEngine;

public class Action_Chase : BaseAction
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
        enemy.Animator.SetTrigger("DoWalk");
        enemy.Movement.FollowTarget(enemy.Target.transform, false);

        base.Begin();
    }

    public override void Tick()
    {

    }

    public override void End()
    {
        enemy.Movement.StopMovement();
        
        base.End();
    }

    public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        return CBS_WeightHelper.GoTowards(_rayDirection, _goalDirection);
    }
}
