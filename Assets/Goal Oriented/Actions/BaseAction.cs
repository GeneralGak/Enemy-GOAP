using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    [SerializeField] List<BaseGoal> satisfiableGoals = new List<BaseGoal>();

    protected ContextBasedSteeringBehavior Navigation;
    protected Enemy enemy;
    protected GOAPBrain brain;

    public bool HasFinished { get; protected set; } = false;

    void Awake()
    {
        Navigation = GetComponentInParent<ContextBasedSteeringBehavior>();
        enemy = GetComponentInParent<Enemy>();
        brain = GetComponentInParent<GOAPBrain>();
    }

    void Start()
    {
        Init();
    }
    
    protected abstract void Init();

    public bool CanSatisfy(BaseGoal _goal)
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
        Navigation.CurrentAction = this;
    }

    public abstract void Tick();
    public virtual void End()
    {
        Navigation.CurrentAction = null;
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
