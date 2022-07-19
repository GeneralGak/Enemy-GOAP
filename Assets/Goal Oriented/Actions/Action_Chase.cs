using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Chase : BaseAction
{
    protected override void Initialise()
    {

    }

    public override bool CanSatisfy(BaseGoal goal)
    {
        return goal is Goal_Chase;
    }

    public override float Cost()
    {
        return 0f;
    }

    public override void Begin()
    {
        Navigation.TransformGoal = LinkedAIState.Vision.Target.transform;
        Navigation.UseTransformGoal = true;
        Navigation.SetNewWeights(ContextBasedWeights.CBS_ChaseWeight);
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
