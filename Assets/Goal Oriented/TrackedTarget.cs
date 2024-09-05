using UnityEngine;

/// <summary>
/// Keeps track of awareness score about the tracked target
/// </summary>
public class TrackedTarget
{
	public GameObject Target;
	public DamageReceiver TargetDamageReceiver;
	public Vector3 RawPosition;

	public bool canSeeTarget;
	public float damageDealt;
	public float LastSensedTime = -1f;
	public float lastDamagedTime = -1f;
	public float Awareness; // 0   = Lost (Removed from awareness); 
							// 1   = Suspicious (know possible position)
							// 2   = Seen (Set as potential target)


	public bool UpdateAwareness(GameObject _target, bool _canSeeTarget, Vector3 _position, float _minAwareness)
	{
		var oldAwareness = Awareness;

		if (_canSeeTarget == true)
		{
			Target = _target;
			TargetDamageReceiver = _target.GetComponent<DamageReceiver>();
			canSeeTarget = true;
		}
		RawPosition = _position;
		LastSensedTime = Time.time;
		Awareness = Mathf.Clamp(Mathf.Max(Awareness, _minAwareness), 0f, 2f);

		if ((oldAwareness < 2f && Awareness >= 2f) || // Was not seen and is now seen
			(oldAwareness < 1f && Awareness >= 1f) || // Was not suspicious and is now suspicious
			(oldAwareness <= 0f && Awareness >= 0f))  // Was not lost and is now lost
		{
			return true;
		}

		return false;
	}

	public void UpdateDamageDealt(float _damageAmount)
	{
		damageDealt += _damageAmount;
		lastDamagedTime = Time.time;
	}

	public void DecayDamageDealt(float _decayTime, float _amount)
	{
		// damaged too recently - no change
		if ((Time.time - lastDamagedTime) < _decayTime)
			return;

		damageDealt -= _amount;
	}

	public bool DecayAwareness(float decayTime, float amount)
	{
		// detected too recently - no change
		if ((Time.time - LastSensedTime) < decayTime)
			return false;

		float oldAwareness = Awareness;

		canSeeTarget = false;
		if (TargetDamageReceiver && TargetDamageReceiver.IsDead)
		{
			Awareness = 0;
		}
		else
		{
			Awareness -= amount;
		}

		if (oldAwareness >= 1f && Awareness < 1f)
			return true;
		return Awareness <= 0f;
	}
}
