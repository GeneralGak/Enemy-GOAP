using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets the enemy's target based on awareness scores
/// </summary>
public class EnemyAwareness : AwarenessSystem
{
	[SerializeField] private bool canLooseTarget;

	private Enemy enemy;
	private bool disableCoroutine;

	public GameObject ClosestTarget
	{
		get
		{
			GameObject closestTarget = null;
			float distance = 0;
			foreach (KeyValuePair<GameObject, TrackedTarget> entry in ActiveTargets)
			{
				if (entry.Value.Target == null) continue;

				float tmpDistance = Vector2.Distance(entry.Value.Target.transform.position, enemy.transform.position);

				if (distance == 0 || distance > tmpDistance)
				{
					distance = tmpDistance;
					closestTarget = entry.Value.Target;
				}
			}

			return closestTarget;
		}
	}
	public GameObject FocusedTarget { get; private set; }
	public Vector2 SuspiciousPosition { get; private set; }
	public int TargetsSpottedAmount { get { return ActiveTargets.Count; } }
	public bool CanLooseTarget { get { return canLooseTarget; } set { canLooseTarget = value; } }
	public bool CanSeeTarget { get { return ActiveTargets[enemy.Target].canSeeTarget; } }
	public bool LockOnTarget { get; set; }
	public float FocusTargetDistance { get; set; }
	public float ClosestTargetDistance { get; set; }


	private void OnEnable()
	{
		disableCoroutine = false;
		StartCoroutine(SetDistanceFromTarget(0.1f));
	}

	void Awake()
	{
		enemy = GetComponent<Enemy>();
	}

	protected override void OnBeingDetected(GameObject _target)
	{
		if (FocusedTarget == null)
		{
			FocusedTarget = _target;
		}
		else if (!LockOnTarget && _target != FocusedTarget && ActiveTargets[_target].Awareness > ActiveTargets[FocusedTarget].Awareness)
		{
			FocusedTarget = _target;
		}
	}

	protected override void OnBeingFullyLost()
	{
		if (ActiveTargets.Count == 1)
		{
			SuspiciousPosition = Vector2.zero;
		}
		else
		{
			float awareness = 0;
			Vector2 searchPosition = Vector2.zero;
			foreach (KeyValuePair<GameObject, TrackedTarget> entry in ActiveTargets)
			{
				if (entry.Value.Awareness > awareness)
				{
					awareness = entry.Value.Awareness;
					searchPosition = entry.Value.RawPosition;
				}
			}

			SuspiciousPosition = searchPosition;
		}
	}

	protected override void OnBeingSuspicios(Vector2 _position)
	{
		if (FocusedTarget == null)
		{
			SuspiciousPosition = _position;
		}
	}

	protected override void OnHavingLostDetection(GameObject _target)
	{
		if (!LockOnTarget && ActiveTargets.Count > 1 && FocusedTarget != null && _target == FocusedTarget)
		{
			float awareness = 0;
			GameObject newTarget = null;
			foreach (KeyValuePair<GameObject, TrackedTarget> entry in ActiveTargets)
			{
				if (entry.Value.Awareness > awareness)
				{
					awareness = entry.Value.Awareness;
					newTarget = entry.Value.Target;
				}
			}

			FocusedTarget = newTarget;
		}
		else if (_target == FocusedTarget)
		{
			if (canLooseTarget)
			{
				if (ActiveTargets[FocusedTarget].TargetDamageReceiver)
				{
					ActiveTargets[FocusedTarget].TargetDamageReceiver.onDeath.RemoveListener(LooseAwareness);
				}
				FocusedTarget = null;
				SuspiciousPosition = ActiveTargets[_target].RawPosition;
			}
		}
	}

	protected override void OnHavingTakenHighDamage(GameObject _dangerTarget)
	{
		if (!LockOnTarget && _dangerTarget != null && _dangerTarget != FocusedTarget)
		{
			TrackedTarget target;
			ActiveTargets.TryGetValue(_dangerTarget, out target);
			float damageDealt = target.damageDealt;
			GameObject newTarget = _dangerTarget;
			foreach (KeyValuePair<GameObject, TrackedTarget> entry in ActiveTargets)
			{
				if (entry.Value.damageDealt > damageDealt)
				{
					damageDealt = entry.Value.Awareness;
					newTarget = entry.Value.Target;
				}
			}
			FocusedTarget = newTarget;
		}
	}

	public void ManuallySetTarget(GameObject _newTarget)
	{
		if (_newTarget && !ActiveTargets.ContainsKey(_newTarget))
		{
			ActiveTargets.Add(_newTarget, new TrackedTarget());
		}
		FocusedTarget = _newTarget;
	}

	public override void ResetClass()
	{
		if (enemy == null)
		{
			enemy = GetComponent<Enemy>();
		}

		FocusedTarget = null;
		SuspiciousPosition = Vector2.zero;
		LockOnTarget = false;
		disableCoroutine = false;
		StartCoroutine(SetDistanceFromTarget(0.1f));
		base.ResetClass();
	}

	private IEnumerator SetDistanceFromTarget(float _waitTime)
	{
		while (!disableCoroutine)
		{
			if (FocusedTarget) FocusTargetDistance = Vector2.Distance(transform.position, FocusedTarget.transform.position);
			else FocusTargetDistance = -1;

			if (ClosestTarget) ClosestTargetDistance = Vector2.Distance(transform.position, ClosestTarget.transform.position);
			else ClosestTargetDistance = -1;

			yield return new WaitForSeconds(_waitTime);
		}
	}
}
