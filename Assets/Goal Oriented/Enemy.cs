using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // ************* //
    // TODO: REMOVE THIS
    public float attackCooldown;
    [SerializeField] SpriteRenderer spriteObject;
    [HideInInspector] public float elapsedTime;

    private void Start()
    {

    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if(Target) spriteObject.flipX = Movement.MoveDir.x < 0;
    }
    // ************* //

    [SerializeField] SO_EnemyStats stats;

    public AIVision Vision { get; private set; }
    public Animator Animator { get; private set; }
    public AnimationEventHandler AnimEventHandler { get; private set; }
    public GameObject Target { get { return Vision.Target; } }
    public SO_EnemyStats Stats { get { return stats; } }
    public ContextBasedSteeringBehavior CBS { get; private set; }
    public GOAPBrain Brain { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public GameObject SpriteObject { get; private set; }

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
        CBS = GetComponent<ContextBasedSteeringBehavior>();
        Brain = GetComponentInChildren<GOAPBrain>();
        Movement = GetComponent<EnemyMovement>();
        SpriteObject = GetComponentInChildren<AnimationEventHandler>().gameObject;
    }
}