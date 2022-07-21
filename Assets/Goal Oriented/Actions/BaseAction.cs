using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    [SerializeField] List<BaseGoal> satisfiableGoals = new List<BaseGoal>();

    protected Enemy enemy;

    public bool HasFinished { get; protected set; } = false;

    void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    void Start()
    {
        Init();
    }
    
    protected abstract void Init();

    public virtual bool CanSatisfy(BaseGoal _goal)
    {
        foreach (BaseGoal goal in satisfiableGoals)
        {
            if(_goal == goal)
            {
                return true;
            }
        }

        return false;
    }
    public abstract float Cost();

    public virtual void Begin()
    {
        enemy.CBS.CurrentAction = this;
    }

    public abstract void Tick();
    public virtual void End()
    {
        enemy.CBS.CurrentAction = null;
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
