using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_AttackMelee : BaseFSMAction<Action_AttackMelee.AttackState>
{
    float elapsedTime;
    bool attackDone;
    [SerializeField] float windupTime;
    [SerializeField] float windupDistance;
    [SerializeField] float dashStopDistanceFromPlayer;

    public enum AttackState
    {
        Windup,
        DashForward,
        MoveBack
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
        AddState(AttackState.MoveBack);
    }

    protected override void OnEnter()
    {
        switch (State)
        {
            case AttackState.Windup:
                attackDone = false;
                elapsedTime = 0;
                //enemy.elapsedTime = 0;
                enemy.Animator.SetTrigger("DoWindup");
                Navigation.StartMovement();
                Navigation.Destination = (transform.position - enemy.Target.transform.position).normalized * windupDistance;
                break;

            case AttackState.DashForward:
                enemy.Animator.SetTrigger("DoAttack");
                enemy.AnimEventHandler.onAttack.AddListener(SpawnHitbox);
                enemy.AnimEventHandler.onAttackDone.AddListener(AttackDone);
                Vector2 dirToTarget = enemy.Target.transform.position - transform.position;
                Navigation.Destination = dirToTarget - dirToTarget.normalized * dashStopDistanceFromPlayer;
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
                    HasFinished = true;
                }
                break;
        }

        return State;
    }

    void SpawnHitbox()
    {
        enemy.AnimEventHandler.onAttack.RemoveListener(SpawnHitbox);
        Debug.Log("Hit!");
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
