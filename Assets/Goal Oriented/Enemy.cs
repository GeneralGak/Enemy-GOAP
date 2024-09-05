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

	public EnemyAwareness Awareness { get; private set; }
	public AIVision Vision { get; private set; }
    public Animator Animator { get; private set; }
    public AnimationEventHandler AnimEventHandler { get; private set; }
    public GameObject Target { get { return Awareness.FocusedTarget; } }
    public SO_EnemyStats Stats { get { return stats; } }
    public GOAPBrain Brain { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
	public DamageReceiver DamageReceiver { get; protected set; }
	public AISteering Steering { get; private set; }
	public Aiming Aiming { get; private set; }
	public float DistanceToTarget { get { return Awareness.FocusTargetDistance; } }
	public Vector2 WanderCenter { get; set; }


	private void Awake()
	{
		Vision = GetComponentInChildren<AIVision>();
		DamageReceiver = GetComponent<DamageReceiver>();
		Aiming = GetComponent<Aiming>();
		Awareness = GetComponent<EnemyAwareness>();
        Movement = GetComponent<EnemyMovement>();
		Steering = GetComponent<AISteering>();
        Animator = GetComponentInChildren<Animator>();
        AnimEventHandler = Animator.GetComponent<AnimationEventHandler>();
		SpriteRenderer = AnimEventHandler.GetComponent<SpriteRenderer>();
        Brain = GetComponentInChildren<GOAPBrain>();
        WanderCenter = transform.position;
    }
}