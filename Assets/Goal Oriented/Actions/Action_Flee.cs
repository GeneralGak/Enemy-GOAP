using UnityEngine;

public class Action_Flee : BaseAction
{
    public override void Init()
    {

    }

    // TODO: Look into an easier way.
    public override float Cost()
    {
        if(enemy.DistanceToTarget < enemy.Stats.attackRange)
		{
            return enemy.DistanceToTarget / enemy.Stats.attackRange * 100f;
        }

        return 100f;
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
        return CBS_WeightHelper.GoAway(_rayDirection, _goalDirection);
    }
}
