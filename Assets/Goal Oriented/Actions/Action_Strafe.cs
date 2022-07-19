using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Strafe : BaseAction
{
    [SerializeField] private float baseCost = 10f;

    protected override void Initialise()
    {

    }

    public override bool CanSatisfy(BaseGoal goal)
    {
        return goal is Goal_InAttackRange;
    }

    public override float Cost()
    {
        return baseCost;
    }

    public override void Begin()
    {
        Navigation.TransformGoal = LinkedAIState.Vision.Target.transform;
        Navigation.UseTransformGoal = true;
        Navigation.SetNewWeights(ContextBasedWeights.CBS_StrafeWeight);
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
