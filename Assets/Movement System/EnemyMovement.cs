using UnityEngine;
using UnityEngine.Events;

public enum MovementType
{
    Walking,
    Jumping
}

public class EnemyMovement : MonoBehaviour
{
	[SerializeField] private float stuckCheckDistance = 0.005f;
	[SerializeField] private float stuckDeathTimer = 10f;
	[SerializeField] private ContactFilter2D contactFilter;

	Enemy enemy;
	Rigidbody2D rb;
	bool move;
	bool ignoreDistanceToTarget;
	bool doJump;
	bool jumpDone = true;
	bool ignoreCBS;
	Transform followTarget;
	Vector2 moveDirection;
	Vector2 targetPos;
	Vector2 jumpTargetPos;
	Vector2 jumpVelocity;
	Vector2 prevPosition;
	Vector2 previousDirection;
	Vector2 StuckMoveDir;
	float elapsedJumpTime;
	float elapsedDeathTime;
	float elapsedStuckTime;
	float jumpDuration;
	float jumpHeight;
	float index;
	float maxStuckTime = 0.3f;
	float maxUnstuckTime = 1.5f;
	float initialCrossProduct;
	string animationName;

	AnimationEventHandler animationEventHandler;
	float animationjumpDuration = 0.5f;
	UnityAction onMoveDoneCallback;
	Vector2 spineSpritePosition;

	const int DEFAULT_FRAMERATE = 12;
	const float CLOSE_ENOUGH_DISTANCE_THRESHOLD = 0.012f;

	public bool IsMoving { get { return move; } }
	public bool UseCBS { get; set; } = true;
	public bool IsStuck { get; private set; }
	public bool JumpDone { get { return jumpDone && !doJump; } }
	public bool JumpInitiated { get; private set; }
	public bool JumpOnEvent { get; set; } = true;
	public bool IgnoreCollidersOnJump { get; set; } = false;
	public float MaxJumpLength { get; set; }
	public float Speed { get; set; }
	public float SpeedMultiplier { get; set; } = 1;
	public float SteerForce { get; set; } = 0.8f;
	public float MaxStuckTime { get { return maxStuckTime; } set { maxStuckTime = value; } }
	public Vector2 MoveDir { get; set; }
	public Vector2 DesiredVelocity { get; private set; }
	public Vector2 Velocity { get; private set; }
	public Vector2 KnockbackVelocity { get; set; }
	public UnityEvent UpdateKnockbackVelocity { get; set; } = new UnityEvent();
	public UnityEvent OnJumpStart { get; set; } = new UnityEvent();
	public Rigidbody2D Rigidbody { get { return rb; } }

	private void Awake()
	{
		enemy = GetComponent<Enemy>();
		rb = GetComponent<Rigidbody2D>();
		animationEventHandler = GetComponentInChildren<AnimationEventHandler>();
		prevPosition = transform.position;
	}

	private void Start()
	{
		if (!enemy.Steering)
		{
			UseCBS = false;
		}
	}

	private void FixedUpdate()
	{
		if (move)
		{
			Move();

			if (!IsStuck)
			{
				CheckIfStuck();
			}
			else
			{
				CheckIfUnstuck();
			}
		}
	}

	void Move()
	{
		Vector2 targetPosition = Vector2.zero;

		if (moveDirection == Vector2.zero)
		{
			if (followTarget)
			{
				targetPosition = followTarget.position;
			}
			else
			{
				targetPosition = targetPos;
			}

			MoveDir = targetPosition - (Vector2)transform.position;
		}
		else
		{
			MoveDir = moveDirection;
		}

		if (!ignoreCBS && UseCBS)
		{
			MoveDir = enemy.Steering.CBS.GetDir(MoveDir);
		}
		//      else if(!ignoreCBS && UseCBS)
		//{
		//          MoveDir = StuckMoveDir;
		//      }

		DesiredVelocity = MoveDir.normalized * (Speed * SpeedMultiplier);
		Velocity = Vector2.Lerp(Velocity, DesiredVelocity, SteerForce);

		rb.MovePosition((Vector2)transform.position + (Velocity * Time.fixedDeltaTime));

		float distToTarget;
		Vector2 directionToPoint;

		if (moveDirection == Vector2.zero)
		{
			float currentCrossProduct;

			if (followTarget)
			{
				//distToTarget = Vector2.Distance(followTarget.transform.position, transform.position);
				directionToPoint = followTarget.transform.position - transform.position;

				//currentCrossProduct = followTarget.transform.position.x * aiBase.transform.position.y - followTarget.transform.position.y * aiBase.transform.position.x;
			}
			else
			{
				//distToTarget = Vector2.Distance(targetPos, transform.position);
				directionToPoint = targetPos - (Vector2)transform.position;

				//currentCrossProduct = targetPos.x * aiBase.transform.position.y - targetPos.y * aiBase.transform.position.x;
			}

			if (!ignoreDistanceToTarget && (directionToPoint.magnitude < CLOSE_ENOUGH_DISTANCE_THRESHOLD || (previousDirection != Vector2.zero && Vector2.Angle(directionToPoint, previousDirection) > 180)))
			{
				MoveDone(targetPosition);
			}
			else previousDirection = directionToPoint;
			//if (!ignoreDistanceToTarget && Mathf.Sign(initialCrossProduct) != Mathf.Sign(currentCrossProduct))
			//         {
			//             MoveDone(targetPosition);
			//         }
		}

		//Debug.Log("Move Velocity: " + Velocity);

	}

	void MoveDone(Vector2 _targetPos)
	{
		rb.MovePosition(_targetPos);
		move = false;

		if (onMoveDoneCallback != null)
		{
			onMoveDoneCallback.Invoke();
			onMoveDoneCallback = null;
		}
	}

	public void MoveToPosition(Vector2 _targetPos, bool _ignoreDistanceToTarget = false, UnityAction _onMoveDoneCallback = null, bool _ignoreCBS = false)
	{
		move = true;
		followTarget = null;
		moveDirection = Vector2.zero;
		targetPos = _targetPos;
		ignoreDistanceToTarget = _ignoreDistanceToTarget;
		onMoveDoneCallback = _onMoveDoneCallback;
		ignoreCBS = _ignoreCBS;
		initialCrossProduct = targetPos.x * enemy.transform.position.y - targetPos.y * enemy.transform.position.x;
	}

	public void FollowTarget(Transform _followTarget, bool _ignoreDistanceToTarget, bool _ignoreCBS = false)
	{
		move = true;
		moveDirection = Vector2.zero;
		followTarget = _followTarget;
		ignoreDistanceToTarget = _ignoreDistanceToTarget;
		ignoreCBS = _ignoreCBS;
		initialCrossProduct = followTarget.transform.position.x * enemy.transform.position.y - followTarget.transform.position.y * enemy.transform.position.x;
	}

	public void WalkInDirection(Vector2 _dir, bool _ignoreCBS = false)
	{
		move = true;
		moveDirection = _dir;
		followTarget = null;
		ignoreCBS = _ignoreCBS;
	}

	public void StopMovement()
	{
		move = false;
	}

	private void ResetClass()
	{
		enemy.SpriteRenderer.transform.localPosition = Vector3.zero;
		enemy.Animator.SetFloat("Speed", 1);
		doJump = false;
		move = false;
		UseCBS = true;
		IsStuck = false;
		elapsedStuckTime = 0;
		prevPosition = Vector2.zero;
	}

	public void OnObjectSpawn()
	{
		ResetClass();
	}

	public void OnSpawning()
	{
		ResetClass();
	}

	private void CheckIfStuck()
	{
		float distance = Vector2.Distance(prevPosition, transform.position);
		if (distance < stuckCheckDistance)
		{
			elapsedStuckTime += Time.fixedDeltaTime;

			if (elapsedStuckTime >= maxStuckTime)
			{
				IsStuck = true;
				elapsedStuckTime = 0;
				StuckMoveDir = -MoveDir;
			}
		}
		else
		{
			prevPosition = transform.position;
			elapsedStuckTime = 0;
		}
	}

	private void CheckIfUnstuck()
	{
		if (Vector2.Distance(prevPosition, transform.position) > stuckCheckDistance)
		{
			elapsedDeathTime = 0;
			elapsedStuckTime += Time.fixedDeltaTime;

			if (elapsedStuckTime >= maxUnstuckTime)
			{
				IsStuck = false;
				elapsedStuckTime = 0;
			}
		}
		else
		{
			elapsedStuckTime = 0;
			elapsedDeathTime += Time.fixedDeltaTime;

			if (elapsedDeathTime > stuckDeathTimer)
			{
				Debug.Log("Stuck death");
				enemy.DamageReceiver.Kill();
			}
		}
	}
}
