using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Goal Oriented Action Planning (GOAP)
/// Is responsible for picking the next action to perform based on the highest prioritised goal if goal has a possible action
/// </summary>
public class GOAPBrain : MonoBehaviour
{
	BaseGoal[] Goals;
	BaseAction[] Actions;
	Enemy enemy;

	BaseGoal activeGoal;
	BaseAction activeAction;

	public bool DissableBrain { get; set; } = false;
	public BaseAction ActiveAction { get { return activeAction; } }
	public UnityAction<BaseAction> OnGoalChanged { get; set; }

	// For debugging
	public string DebugInfo_ActiveGoal => activeGoal != null ? activeGoal.GetType().Name : "None";
	public string DebugInfo_ActiveAction => activeAction != null ? $"{activeAction.GetType().Name}{activeAction.GetDebugInfo()}" : "None";
	public int NumGoals => Goals.Length;
	public string DebugInfo_ForGoal(int index)
	{
		return Goals[index].GetDebugInfo();
	}
	// *********


	void Awake()
	{
		Goals = GetComponents<BaseGoal>();
		Actions = GetComponents<BaseAction>();
		enemy = GetComponentInParent<Enemy>();
	}

	void Start()
	{
		if (AIDebugger.Instance != null)
			AIDebugger.Instance.Register(this);
	}

	void OnDestroy()
	{
		if (AIDebugger.Instance != null)
			AIDebugger.Instance.Deregister(this);
	}

	void Update()
	{
		if (DissableBrain)
		{
			return;
		}

		// pretick all goals to refresh priorities
		for (int goalIndex = 0; goalIndex < Goals.Length; ++goalIndex)
			Goals[goalIndex].PreTick();

		if ((activeGoal && !activeGoal.CommitTo) || !activeGoal || (activeGoal && activeGoal.CommitTo && enemy.DamageReceiver.IsDead))
			RefreshPlan();

		if (activeGoal)
		{
			activeGoal.Tick();

			// if action finished - cleanup the goal
			if (activeAction.HasFinished)
			{
				activeGoal.Deactivate();
				activeGoal = null;
				activeAction = null;
			}
		}
	}

	void RefreshPlan()
	{
		// find the best goal-action pair
		BaseGoal bestGoal = null;
		BaseAction bestAction = null;
		for (int goalIndex = 0; goalIndex < Goals.Length; ++goalIndex)
		{
			var candidateGoal = Goals[goalIndex];

			// skip if goal can't run when AI is dead
			if (!candidateGoal.RunOnDeath && enemy.DamageReceiver.IsDead)
				continue;

			// skip if can't run
			if (!candidateGoal.CanRun)
				continue;

			// skip if current best goal is a higher priority
			if (bestGoal != null && bestGoal.Priority > candidateGoal.Priority)
				continue;

			// find the cheapest action for this goal
			BaseAction bestActionForCandidateGoal = null;
			for (int actionIndex = 0; actionIndex < Actions.Length; ++actionIndex)
			{
				var candidateAction = Actions[actionIndex];

				// skip if action cannot satisfy the goal
				if (!candidateAction.CanSatisfy(candidateGoal))
					continue;

				// is this action more expensive - if so skip
				if (bestActionForCandidateGoal != null && candidateAction.Cost() > bestActionForCandidateGoal.Cost())
					continue;

				bestActionForCandidateGoal = candidateAction;
			}

			// found a viable action
			if (bestActionForCandidateGoal != null)
			{
				bestGoal = candidateGoal;
				bestAction = bestActionForCandidateGoal;
			}
		}

		// current plan holds - do nothing
		if (bestGoal == activeGoal && bestAction == activeAction)
			return;

		// no plan viable currently
		if (bestGoal == null)
		{
			if (activeGoal != null)
				activeGoal.Deactivate();

			activeGoal = null;
			activeAction = null;
			return;
		}

		// goal has changed?
		if (bestGoal != activeGoal)
		{
			if (activeGoal != null)
				activeGoal.Deactivate();

			bestGoal.Activate();

			OnGoalChanged?.Invoke(bestAction);
		}

		// start the action
		activeGoal = bestGoal;
		activeAction = bestAction;
		activeGoal.SetAction(activeAction);
	}

	public void ResetClass()
	{
		if (activeGoal != null)
		{
			activeGoal.Deactivate();
		}
		activeGoal = null;
		activeAction = null;
		if (AIDebugger.Instance != null)
			AIDebugger.Instance.Register(this);
		CustomInitialization();
	}

	public void CustomInitialization()
	{
		for (int i = 0; i < Actions.Length; i++)
		{
			Actions[i].Init();
		}
	}

	public void OnDisable()
	{
		if (AIDebugger.Instance != null)
			AIDebugger.Instance.Deregister(this);
	}
}
