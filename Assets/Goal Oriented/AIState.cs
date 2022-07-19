using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState : MonoBehaviour
{
    [SerializeField] float attackRange = 2;

    public AIVision Vision { get; private set; }
    public float DistanceToTarget { get; private set; } = -1f;
    public float AttackRange => attackRange;
    public int AmountCarried { get; private set; } = 0;
    public GameObject Target { get; private set; } = null;

	private void Awake()
	{
		Vision = GetComponent<AIVision>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Target != null)
		{
            DistanceToTarget = Vector2.Distance(transform.position, Target.transform.position);

            if(Vision.Target == null)
			{
                RemoveTarget();
			}
		}
    }

    public void SetAmountCarried(int amount)
    {
        AmountCarried = amount;
    }

    public void SetNewTarget(GameObject _newTarget)
	{
        Target = _newTarget;
	}

    private void RemoveTarget()
	{
        Target = null;
        DistanceToTarget = -1;
	}
}
