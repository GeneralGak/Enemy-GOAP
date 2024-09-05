using UnityEngine;

/// <summary>
/// Base class for all goals
/// </summary>
public abstract class BaseGoal : MonoBehaviour
{
	[SerializeField] protected int basePriority = 30;
	[SerializeField] private bool setAsCommitTo = false;
	[SerializeField] private bool canRunOnDeath = false;

	public const int MaxPriority = 100;

	public bool CanRun { get; protected set; } = false;
	public bool CommitTo { get; private set; } = false;
	public bool RunOnDeath { get; private set; } = false;
	public int Priority { get; protected set; } = 0;
	public bool IsActive { get; protected set; } = false;

	protected BaseAction LinkedAction;
	protected Enemy enemy;
	protected GOAPBrain brain;

	protected virtual void Awake()
	{
		enemy = GetComponentInParent<Enemy>();
		brain = GetComponentInParent<GOAPBrain>();
	}

	protected virtual void Start()
	{

	}

	public virtual void Activate()
	{
		if (setAsCommitTo)
		{
			CommitTo = true;
		}

		if (canRunOnDeath)
		{
			RunOnDeath = true;
		}

		IsActive = true;
	}

	public virtual void Deactivate()
	{
		CommitTo = false;
		LinkedAction.End();
		LinkedAction = null;

		IsActive = false;
	}

	public void SetAction(BaseAction newAction)
	{
		if (LinkedAction != null && newAction != LinkedAction)
		{
			LinkedAction.End();
		}

		LinkedAction = newAction;

		LinkedAction.Begin();
	}

	/// <summary>
	/// Used to set CanRun and Priority before choosing goal
	/// </summary>
	public abstract void PreTick();

	public void Tick()
	{
		LinkedAction.Tick();
	}

	public virtual string GetDebugInfo()
	{
		return $"{GetType().Name}: Priority={Priority} CanRun={CanRun}";
	}
}
