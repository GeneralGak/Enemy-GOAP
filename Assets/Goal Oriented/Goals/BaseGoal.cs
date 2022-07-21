using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGoal : MonoBehaviour
{
    public const int MaxPriority = 100;
    
    public bool CanRun { get; protected set; } = false;
    public bool CommitTo { get; protected set; } = false;
    public int Priority { get; protected set; } = 0;
    public bool IsActive { get; protected set; } = false;

    protected BaseAction LinkedAction;
    protected Enemy enemy;
    protected GOAPBrain brain;

    void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        brain = GetComponentInParent<GOAPBrain>();
    }

    public virtual void Activate() 
    {
        IsActive = true;
    }

    public virtual void Deactivate() 
    {
        LinkedAction.End();

        IsActive = false;
    }

    public void SetAction(BaseAction newAction)
    {
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
