using UnityEngine;

public class Action_Wander : BaseAction
{
	public float wanderCircleRadius;
	[SerializeField, Range(0, 1)] private float lerpDistance = 0.3f;
	[SerializeField] private float angleChange = 0.1f;
	[SerializeField] private LayerMask collidersToMoveAwayFrom;
	[SerializeField] private float LockTurningTime = 1.5f;

	private Vector2 wanderCenter;
	private Vector2 wanderDir;
	private bool colliding;
	private bool isFlipped;
	private float elapsedTimeBeforeCanTurn;
	private Coroutine removeCollisionCall;


	public override float Cost()
	{
		return 0f;
	}

	public override void Init()
	{
		if (enemy.WanderCenter == Vector2.zero)
		{
			wanderCenter = transform.position;
		}
		else
		{
			wanderCenter = enemy.WanderCenter;
		}
	}

	public override void Begin()
	{
		enemy.Animator.SetTrigger("DoWalk");
		enemy.Movement.MoveToPosition(wanderCenter, true);
		isFlipped = enemy.Aiming.IsFlipped;
		elapsedTimeBeforeCanTurn = LockTurningTime;

		base.Begin();
	}

	public override void Tick()
	{
		elapsedTimeBeforeCanTurn += Time.deltaTime;

		float lerpValue = (Vector2.Distance(wanderCenter, transform.position) - wanderCircleRadius * lerpDistance) / wanderCircleRadius * lerpDistance;

		Vector2 goalDir = Vector2.Lerp(enemy.Movement.MoveDir, wanderCenter - (Vector2)transform.position, lerpValue);

		wanderDir = enemy.Steering.CalculateWanderDir(goalDir, wanderCircleRadius, angleChange);
	}

	public override void End()
	{
		enemy.Movement.StopMovement();

		base.End();
	}

	public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
	{
		return CBS_WeightHelper.GoTowards(_rayDirection, wanderDir);
	}
}
