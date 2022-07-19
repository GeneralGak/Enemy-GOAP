using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Flee : BaseAction
{
    protected override void Initialise()
    {

    }

    public override bool CanSatisfy(BaseGoal goal)
    {
        if(goal is Goal_TargetTooClose || goal is Goal_InAttackRange)
		{
            return true;
		}

        return false;
    }

    // TODO: Look into an easier way.
    public override float Cost()
    {
        if(LinkedAIState.DistanceToTarget < LinkedAIState.AttackRange)
		{
            return LinkedAIState.DistanceToTarget / LinkedAIState.AttackRange * 100f;
        }

        return 100f;
    }

    public override void Begin()
    {
        Navigation.TransformGoal = LinkedAIState.Vision.Target.transform;
        Navigation.UseTransformGoal = true;
        Navigation.SetNewWeights(ContextBasedWeights.CBS_FleeWeight);
        Navigation.StartMovement();
    }

    public override void Tick()
    {

    }

    public override void Halt()
    {
        Navigation.UseTransformGoal = false;
        Navigation.TransformGoal = null;
    }
}
