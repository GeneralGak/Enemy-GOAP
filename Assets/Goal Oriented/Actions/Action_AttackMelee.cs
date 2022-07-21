using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_AttackMelee : BaseFSMAction<Action_AttackMelee.AttackState>
{
    float elapsedTime;
    bool attackDone;
    [SerializeField] float windupTime = 0.5f;
    [SerializeField] float windupDistance = 0.3f;
    [SerializeField] float dashStopDistanceFromPlayer = 0.6f;
    [SerializeField] float windupJumpHeight = 0.1f;
    [SerializeField] float windupJumpDuration = 0.2f;
    [SerializeField] float dashHeight = 0.4f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float maxDashLength = 3f;

    public enum AttackState
    {
        Windup,
        DashForward
    }

    public override bool CanSatisfy(BaseGoal _goal)
    {
        return base.CanSatisfy(_goal) && enemy.elapsedTime > enemy.attackCooldown;
    }

    public override float Cost()
    {
        return 0f;
    }

    protected override void Init()
    {
        AddState(AttackState.Windup);
        AddState(AttackState.DashForward);
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case AttackState.Windup:
                attackDone = false;
                enemy.elapsedTime = 0;
                enemy.Animator.SetTrigger("DoWindup");
                enemy.Movement.MoveDir = enemy.Target.transform.position - transform.position;
                Vector2 jumpVelocity = (transform.position - enemy.Target.transform.position).normalized * windupDistance;
                enemy.Movement.JumpToPosition((Vector2)transform.position + jumpVelocity, windupJumpHeight, windupJumpDuration);
                break;

            case AttackState.DashForward:
                enemy.Animator.SetTrigger("DoAttack");
                enemy.AnimEventHandler.onAttack.AddListener(SpawnHitbox);
                enemy.AnimEventHandler.onAttackDone.AddListener(AttackDone);
                Vector2 dirToTarget = enemy.Target.transform.position - transform.position;
                Vector2 jumpVelocity_ = dirToTarget - dirToTarget.normalized * dashStopDistanceFromPlayer;
                if(jumpVelocity_.magnitude > maxDashLength) { jumpVelocity_ = jumpVelocity_.normalized * maxDashLength; }
                enemy.Movement.JumpToPosition((Vector2)transform.position + jumpVelocity_, dashHeight, dashDuration);
                break;
        }
    }

    protected override void OnTick()
    {
        elapsedTime += Time.deltaTime;
    }

    protected override AttackState CheckTransition()
    {
        switch (State)
        {
            case AttackState.Windup:
                if(elapsedTime > windupTime)
                {
                    return AttackState.DashForward;
                }
                break;

            case AttackState.DashForward:
                if (attackDone)
                {
                    enemy.elapsedTime = 0;
                    elapsedTime = 0;
                    HasFinished = true;
                }
                break;
        }

        return State;
    }

    void SpawnHitbox()
    {
        enemy.AnimEventHandler.onAttack.RemoveListener(SpawnHitbox);
        //Debug.Log("Hit!");
    }

    void AttackDone()
    {
        enemy.AnimEventHandler.onAttackDone.RemoveListener(AttackDone);
        attackDone = true;
    }

    public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        return CBS_WeightHelper.GoTowards(_rayDirection, _goalDirection);
    }
}
