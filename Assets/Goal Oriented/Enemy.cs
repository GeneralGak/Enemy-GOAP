using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] SO_EnemyStats stats;

    public AIVision Vision { get; private set; }
    public GameObject Target { get { return Vision.Target; } }
    public SO_EnemyStats Stats { get { return stats; } }

    public float DistanceToTarget
    {
        get
        {
            if (Target) 
            { 
                return Vector2.Distance(transform.position, Target.transform.position); 
            }
            else 
            { 
                return -1; 
            }
        }
    }

    private void Awake()
	{
		Vision = GetComponent<AIVision>();
	}
}
