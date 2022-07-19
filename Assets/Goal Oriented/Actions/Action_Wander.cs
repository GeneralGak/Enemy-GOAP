using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Wander : BaseFSMAction<Action_Wander.EState>
{
	[SerializeField] private float maxWanderRange = 3f;
	[SerializeField] private float minWanderRange = 1.5f;

    private Vector2 spawnLocation;
    private bool returnToLocation;

	public enum EState
	{
		WanderLocation,
		MoveBack
	}

	public override bool CanSatisfy(BaseGoal goal)
	{
		return goal is Goal_Wander;
	}

	public override float Cost()
	{
		return 0f;
	}

    protected override void Initialise()
    {
        AddState(EState.WanderLocation, OnEnter_WanderLocation, OnTick_WanderLocation, null, CheckTransition_WanderLocation);
        AddState(EState.MoveBack, OnEnter_MoveBack, OnTick_MoveBack, null, CheckTransition_MoveBack);
        spawnLocation = transform.position;
    }

    void OnEnter_MoveBack()
    {
        Navigation.VectorGoal = spawnLocation;
        Navigation.SetNewWeights(ContextBasedWeights.CBS_ChaseWeight);
    }

    void OnTick_MoveBack()
	{
        returnToLocation = Vector2.Distance(spawnLocation, transform.position) >= minWanderRange;
    }

    EState CheckTransition_MoveBack()
    {
        return returnToLocation ? EState.MoveBack : EState.WanderLocation;
    }

    void OnEnter_WanderLocation()
    {
        Navigation.StartMovement();
        Navigation.UseTransformGoal = false;
        Navigation.SetNewWeights(ContextBasedWeights.CBS_Wander);
    }

    void OnTick_WanderLocation()
    {
        returnToLocation = Vector2.Distance(spawnLocation, transform.position) >= maxWanderRange;
    }

    EState CheckTransition_WanderLocation()
    {
        return returnToLocation ? EState.MoveBack : EState.WanderLocation;
    }
}
