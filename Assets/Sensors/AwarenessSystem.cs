using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Gives an AI different levels of awareness of targets
/// Awareness is based on vision, damage received and proximity
/// Sets awareness score on all visible and previously visible targets
/// Awareness scores decay over time if no new information is received
/// </summary>
public abstract class AwarenessSystem : MonoBehaviour
{
	[SerializeField] float awarenessDecayDelay = 0.3f;
	[SerializeField] float awarenessDecayRate = 0.1f;

	[SerializeField] float damageTakenDecayDelay = 0.8f;
	[SerializeField] float damageTakenDecayRate = 0.2f;
	[SerializeField] float damageTakenTriggerAmount = 15f;


	private Dictionary<GameObject, TrackedTarget> targets = new Dictionary<GameObject, TrackedTarget>();


	public Dictionary<GameObject, TrackedTarget> ActiveTargets { get { return targets; } }
	public float VisionMinimumAwareness { get { return 2; } }
	public float BeingDamagedMinimumAwareness { get { return 1; } }
	public float ProximityMinimumAwareness { get { return 1; } }
	public float AwarenessDecayDelay { get { return awarenessDecayDelay; } set { awarenessDecayDelay = value; } }
	public float AwarenessDecayRate { get { return awarenessDecayRate; } set { awarenessDecayRate = value; } }


	// Update is called once per frame
	void Update()
	{
		List<GameObject> toCleanup = new List<GameObject>();
		foreach (var targetGO in targets.Keys)
		{
			targets[targetGO].DecayDamageDealt(damageTakenDecayDelay, damageTakenDecayRate * Time.deltaTime);
			if (targets[targetGO].DecayAwareness(awarenessDecayDelay, awarenessDecayRate * Time.deltaTime))
			{
				if (targets[targetGO].Awareness <= 1f)
					OnHavingLostDetection(targetGO);
				else if (targets[targetGO].Awareness == 0f)
				{
					OnBeingFullyLost();
					toCleanup.Add(targetGO);
				}
			}
		}

		// cleanup targets that are no longer detected
		foreach (var target in toCleanup)
			targets.Remove(target);
	}

	void UpdateAwareness(GameObject _targetGO, bool _canSetTarget, Vector3 _position, float _minAwareness)
	{
		// not in targets
		if (!targets.ContainsKey(_targetGO))
		{
			targets[_targetGO] = new TrackedTarget();
		}

		// update target awareness
		if (targets[_targetGO].UpdateAwareness(_targetGO, _canSetTarget, _position, _minAwareness))
		{
			if (targets[_targetGO].Awareness >= 2f)
				OnBeingDetected(_targetGO);
			else if (targets[_targetGO].Awareness >= 1f)
				OnBeingSuspicios(_position);
		}
	}

	private void UpdateDamageDealt(GameObject _damageDealer, float _damageAmount)
	{
		targets[_damageDealer].UpdateDamageDealt(_damageAmount);

		if (targets[_damageDealer].damageDealt > damageTakenTriggerAmount)
		{
			float highestDamage = 0f;
			GameObject priorityTarget = null;
			foreach (GameObject target in targets.Keys)
			{
				if (targets[target].damageDealt > highestDamage && targets[target].Target != null)
				{
					highestDamage = targets[target].damageDealt;
					priorityTarget = targets[target].Target;
				}
			}

			if (priorityTarget != null)
			{
				OnHavingTakenHighDamage(priorityTarget);
			}
		}
	}

	public void ReportCanSee(GameObject _seenGO)
	{
		UpdateAwareness(_seenGO.gameObject, true, _seenGO.transform.position, 2);
	}

	public void ReportBeingDamaged(GameObject _damagingGO, float _damage)
	{
		if (_damagingGO == null) return;

		UpdateAwareness(_damagingGO.gameObject, false, _damagingGO.transform.position, 1);
		UpdateDamageDealt(_damagingGO, _damage);
	}

	public void ReportInProximity(GameObject target)
	{
		UpdateAwareness(target, false, target.transform.position, 1);
	}

	public void LooseAwareness(GameObject _go)
	{
		targets[_go].Awareness = 0f;
		targets[_go].Target.GetComponent<DamageReceiver>().onDeath.RemoveListener(LooseAwareness);
	}

	public virtual void ResetClass()
	{
		targets.Clear();
	}

	protected abstract void OnBeingDetected(GameObject _target);

	protected abstract void OnBeingSuspicios(Vector2 _position);

	protected abstract void OnHavingLostDetection(GameObject _target);

	protected abstract void OnBeingFullyLost();

	protected abstract void OnHavingTakenHighDamage(GameObject _dangerTarget);
}
