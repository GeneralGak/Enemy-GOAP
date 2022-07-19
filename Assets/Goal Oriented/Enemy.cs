using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // TODO: REMOVE THIS
    public float attackCooldown;
    public SpriteRenderer spriteObject;
    [HideInInspector] public float elapsedTime;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if(Target) spriteObject.flipX = Target.transform.position.x < transform.position.x;
    }


    [SerializeField] SO_EnemyStats stats;

    public AIVision Vision { get; private set; }
    public Animator Animator { get; private set; }
    public AnimationEventHandler AnimEventHandler { get; private set; }
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
        Animator = GetComponentInChildren<Animator>();
        AnimEventHandler = GetComponentInChildren<AnimationEventHandler>();
    }
}
