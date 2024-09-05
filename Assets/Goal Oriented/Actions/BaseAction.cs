using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all actions
/// </summary>
public abstract class BaseAction : MonoBehaviour
{
	[SerializeField] List<BaseGoal> satisfiableGoals = new List<BaseGoal>();
	[SerializeField] protected bool satisfyOnHealth;
	[SerializeField][ShowIf(nameof(satisfyOnHealth))] HealthStatus healthStatus;
	[SerializeField][ShowIf(nameof(satisfyOnHealth))] float percentHealth;

	protected Enemy enemy;

	public bool HasFinished { get; protected set; } = false;


	public enum HealthStatus
	{
		LessThen,
		MoreThen
	}

	void Awake()
	{
		enemy = GetComponentInParent<Enemy>();
	}

	void Start()
	{
		Init();
	}

	public abstract void Init();

	public virtual bool CanSatisfy(BaseGoal _goal)
	{
		foreach (BaseGoal goal in satisfiableGoals)
		{
			if (_goal == goal)
			{
				bool satisfy = true;

				if (satisfyOnHealth)
				{
					float percentOfHealth = enemy.DamageReceiver.CurrentHealth / enemy.DamageReceiver.MaxHealth * 100;
					if ((healthStatus == HealthStatus.LessThen && percentOfHealth > percentHealth) ||
						(healthStatus == HealthStatus.MoreThen && percentOfHealth < percentHealth))
					{
						satisfy = false;
					}
				}

				return satisfy;
			}
		}

		return false;
	}

	public abstract float Cost();

	public virtual void Begin()
	{
		if (enemy.Steering)
		{
			enemy.Steering.CBS.CurrentAction = this;
		}

		HasFinished = false;
	}

	public abstract void Tick();

	public virtual void End()
	{
		if (enemy.Steering)
		{
			enemy.Steering.CBS.CurrentAction = null;
		}
	}

	public virtual float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
	{
		return 0;
	}

	public virtual string GetDebugInfo()
	{
		return string.Empty;
	}
}
