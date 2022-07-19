using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_CheckForTarget : StateMachineBehaviour
{
	[SerializeField] private EAIState newState;

	private EnemyAI enemy;
	private AIVision vision;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if(enemy == null)
		{
			enemy = animator.GetComponent<EnemyAI>();
			vision = animator.GetComponent<AIVision>();
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if(vision.Target != null)
		{
			enemy.SwitchToState(newState);
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{

	}
}
