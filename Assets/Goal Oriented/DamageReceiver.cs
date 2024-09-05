using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
	[SerializeField, MinValue(0)] float maxHealth = 1; // Health amount at initialization
	[SerializeField, MinValue(0)] float takingDamageInvincibilityDuration = 0;
	[SerializeField, ShowIf("@takingDamageInvincibilityDuration > 0")] bool disableInvincibilityVisuals;
	[SerializeField, MinValue(0)] float reviveInvincibilityDuration = 0;
	[SerializeField, MinValue(0)] float despawnTime = 0;
	[SerializeField, Range(0, 100)] float chanceToMiss = 0;
	[SerializeField, ShowIf("@invincibilityFramesDuration > 0")] float invincibilityVisualFlashTime = 0;
	[SerializeField] bool destroyOnDeath;
	[SerializeField] SpriteRenderer[] spriteRenderers;
	public Sprite hurtSprite;
	[SerializeField] float hurtSpriteDuration = 0.25f;
	[SerializeField] int lives = 1;
	public bool ShowHurtSpriteOnDeath = true;
	public bool overrideDamageAmount;
	[SerializeField][ShowIf(nameof(overrideDamageAmount))] float overridedDamageAmount;
	[SerializeField] bool canBeStunned;
	[SerializeField] bool disableDamageVisual;
	public bool dontBlockProjectile;
	[SerializeField][ShowIf(nameof(canBeStunned))] float stunCooldown;
	[SerializeField] UnityEvent<GameObject> onDeathEvent = new UnityEvent<GameObject>();
	[SerializeField] UnityEvent onTakeDamageNotDeadEvent = new UnityEvent();
	[SerializeField] UnityEvent onHitAfterDead = new UnityEvent();

	const float DAMAGE_VISUAL_DURATION = 0.05f;
	const float REMOVE_MISSED_DAMAGE_OBJECT_BUFFER = 0.1f;
	bool invincible = false;
	bool isAwake = false;
	float elapsedInvincibilityVisualTime;
	float elapsedStunCooldown;
	float invincibilityFramesDuration;
	int maxLives;
	Animator animator;
	[HideInInspector] public float damageReceivedMultiplier = 1;

	public int InvulnerabilityCount { get; private set; } = 0;
	public bool Invincible { get { return invincible; } }
	public bool DebugInvincible { get; set; }
	public float CurrentHealth { get; private set; }
	public float MaxHealth { get { return maxHealth; } }
	public float MaxMaxHealth { get; } = 12;
	public int CurrentLives { get { return lives; } }
	public int MaxLives { get { return maxLives; } }
	public bool IsDead { get; private set; }
	public bool RoundDamageToInt { get; set; }
	public bool InterceptDeath { get; set; }
	public float DespawnTime { get { return despawnTime; } set { despawnTime = value; } }
	public float ChanceToMiss { get { return chanceToMiss; } set { chanceToMiss = value; } }
	public SpriteRenderer[] SpriteRenderers { get { return spriteRenderers; } }
	public Dictionary<GameObject, bool> MissedCheckerColliders { get; private set; } = new Dictionary<GameObject, bool>();
	public List<GameObject> RemoveMissedColliders { get; private set; } = new List<GameObject>();
	public UnityEvent<DamageReceiver> OnDamageMissed { get; set; } = new UnityEvent<DamageReceiver>();
	public UnityEvent<GameObject> onDeath { get { return onDeathEvent; } }
	public UnityEvent<GameObject> onDeath_Dealer { get; set; } = new UnityEvent<GameObject>();
	public UnityEvent<GameObject> onDeathIntercepted { get; set; } = new UnityEvent<GameObject>();
	public UnityEvent OnRevive { get; set; } = new UnityEvent();
	public UnityEvent<GameObject, float> onTakeDamage { get; set; } = new UnityEvent<GameObject, float>();
	public UnityEvent<GameObject> onHitDealer { get; set; } = new UnityEvent<GameObject>();
	public UnityEvent<float> OnShowDamageVisual { get; set; } = new UnityEvent<float>();
	public UnityEvent<bool> OnShowInvincibilityVisual { get; set; } = new UnityEvent<bool>();
	public UnityEvent onInvulnerabelHit { get; set; } = new UnityEvent();
	public UnityEvent<GameObject> onTakeDamageNotDead { get; set; } = new UnityEvent<GameObject>();
	public UnityEvent<GameObject, float> OnHurtSpriteShown { get; set; } = new UnityEvent<GameObject, float>();
	public UnityEvent<GameObject> OnHurtSpriteRemoved { get; set; } = new UnityEvent<GameObject>();
	public UnityEvent OnHitAfterDead { get { return onHitAfterDead; } set { onHitAfterDead = value; } }
	public UnityAction OnHealthUpdated { get; set; }
	public UnityAction<float> OnMaxHealthUpdated { get; set; }
	public UnityAction<GameObject, GameObject> onKilled { get; set; }
	public UnityAction OnLifeLost { get; set; }


	private void Awake()
	{
		CurrentHealth = maxHealth;
		if (spriteRenderers.Length == 0) spriteRenderers = new SpriteRenderer[] { GetComponentInChildren<SpriteRenderer>() };
		animator = GetComponentInChildren<Animator>();
		isAwake = true;
		maxLives = lives;
	}

	private void Update()
	{
		if (invincible)
		{
			elapsedInvincibilityVisualTime += Time.deltaTime;

			if (!disableInvincibilityVisuals)
			{
				bool showSprite = (int)(elapsedInvincibilityVisualTime / invincibilityVisualFlashTime) % 2 == 0;
				foreach (var sr in spriteRenderers) { sr.enabled = showSprite; }
				OnShowInvincibilityVisual?.Invoke(showSprite);
			}

			if (elapsedInvincibilityVisualTime >= invincibilityFramesDuration)
			{
				if (!disableInvincibilityVisuals)
				{
					foreach (var sr in spriteRenderers) { sr.enabled = true; }
					OnShowInvincibilityVisual?.Invoke(true);
				}

				invincible = false;
				elapsedInvincibilityVisualTime = 0;
			}
		}

		if (elapsedStunCooldown < stunCooldown)
		{
			elapsedStunCooldown += Time.deltaTime;
		}

	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (RemoveMissedColliders.Contains(collision.gameObject))
		{
			StopCoroutine(RemoveMissedColliderBuffer(REMOVE_MISSED_DAMAGE_OBJECT_BUFFER, collision.gameObject));
			RemoveMissedColliders.Remove(collision.gameObject);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (MissedCheckerColliders.ContainsKey(collision.gameObject))
		{
			StartCoroutine(RemoveMissedColliderBuffer(REMOVE_MISSED_DAMAGE_OBJECT_BUFFER, collision.gameObject));
			RemoveMissedColliders.Add(collision.gameObject);
		}
	}

	private void RemoveMissedObject(Collider2D _collision)
	{
		MissedCheckerColliders.Remove(_collision.gameObject);
	}

	private IEnumerator RemoveMissedColliderBuffer(float _removeTime, GameObject _objectToRemove)
	{
		float elapsedRemoveTime = 0;
		while (elapsedRemoveTime < _removeTime)
		{
			elapsedRemoveTime += Time.deltaTime;

			if (elapsedRemoveTime >= _removeTime)
			{
				MissedCheckerColliders.Remove(_objectToRemove);
				RemoveMissedColliders.Remove(_objectToRemove);
			}

			yield return null;
		}
	}

	/// <summary>
	/// Change users max health and adjusts current health to fit the change
	/// </summary>
	/// <param name="_newMaxHealth"></param>
	public void UpdateMaxHealth(float _newMaxHealth)
	{
		if (_newMaxHealth < 0)
		{
			_newMaxHealth = 0;
		}
		else if (RoundDamageToInt)
		{
			_newMaxHealth = Mathf.Min(_newMaxHealth, MaxMaxHealth); // Max max health is 12
		}

		if (CurrentHealth == maxHealth)
		{
			CurrentHealth = _newMaxHealth;
		}
		else
		{
			CurrentHealth *= _newMaxHealth / maxHealth;
		}

		if (RoundDamageToInt)
		{
			CurrentHealth = Mathf.Round(CurrentHealth);
		}

		maxHealth = _newMaxHealth;
		OnMaxHealthUpdated?.Invoke(_newMaxHealth);
		OnHealthUpdated?.Invoke();
	}

	/// <summary>
	/// Applies damage to receivers current health
	/// </summary>
	/// <param name="_amount"></param>
	/// <param name="_sender"></param>
	[Button]
	public void ApplyDamage(float _amount, GameObject _sender, GameObject _dealer, bool _sendKilledEvent = true)
	{
		if (IsDead)
		{
			onHitAfterDead?.Invoke();
			return; // TODO: Remove return and fix problem with lives resetting health, if this causes too much trouple
		}

		if (DebugInvincible)
		{
			onTakeDamage?.Invoke(_sender, _amount);
			return;
		}

		// If invulnerable just return
		if (InvulnerabilityCount > 0 || invincible)
		{
			//TODO: Show invulnerability effect...
			onInvulnerabelHit?.Invoke();
			return;
		}

		onHitDealer?.Invoke(_dealer);

		float dmgAmount = HandleDamage(_amount, _sender);

		if (dmgAmount > 0) CheckInvincibility(takingDamageInvincibilityDuration);

		// Handle death
		if (lives <= 1 && CurrentHealth <= 0 && !IsDead)
		{
			lives--;
			HandleDeath(_dealer, _sender, _sendKilledEvent);
		}
		else if (CurrentHealth <= 0)
		{
			lives--;
			CurrentHealth = maxHealth;

			OnLifeLost?.Invoke();
		}
		else if (dmgAmount > 0)
		{
			onTakeDamageNotDead?.Invoke(_sender);
			onTakeDamageNotDeadEvent?.Invoke(); // TODO: Remove double event
		}

		OnHealthUpdated?.Invoke();
	}

	private float HandleDamage(float _amount, GameObject _sender, bool _triggerEvents = true)
	{
		// Calculate damage amount
		float dmgAmount = _amount;

		if (RoundDamageToInt)
		{
			dmgAmount = Mathf.Round(_amount * damageReceivedMultiplier);
		}

		if (overrideDamageAmount) dmgAmount = overridedDamageAmount;

		// Handle take damage
		if (dmgAmount > 0)
		{
			if (CurrentHealth <= 0)
			{
				return dmgAmount;
			}

			if (dmgAmount > CurrentHealth)
			{
				dmgAmount = CurrentHealth;
				CurrentHealth = 0;
			}
			else
			{
				CurrentHealth -= dmgAmount;
			}

			// Apply damage visual
			if (!disableDamageVisual)
			{
				if (canBeStunned && elapsedStunCooldown >= stunCooldown)
				{
					//spriteRenderers[0].sprite = hurtSprite;
					Invoke(nameof(RemoveHurtSprite), hurtSpriteDuration);
					elapsedStunCooldown = 0;
					if (_triggerEvents) OnHurtSpriteShown?.Invoke(gameObject, hurtSpriteDuration);
				}
				OnShowDamageVisual?.Invoke(DAMAGE_VISUAL_DURATION);
			}

			if (_triggerEvents) onTakeDamage?.Invoke(_sender, dmgAmount);
		}

		return dmgAmount;
	}

	public void CheckInvincibility(float _invincibleDuration)
	{
		// Handle invincibility
		if (_invincibleDuration > 0)
		{
			invincible = true;
			invincibilityFramesDuration = _invincibleDuration;
		}
	}

	private void HandleDeath(GameObject _dealer, GameObject _sender, bool _sendKilledEvent = true, bool _triggerEvents = true)
	{
		CurrentHealth = 0;
		IsDead = true;
		//Debug.Log($"{gameObject.name} is dead.");

		if (InterceptDeath) // Intercept death
		{
			if (_triggerEvents) onDeathIntercepted.Invoke(gameObject);
		}
		else
		{
			if (_triggerEvents && _sendKilledEvent) onKilled?.Invoke(gameObject, _sender);
			if (_triggerEvents) onDeath?.Invoke(gameObject);
			if (_triggerEvents) onDeath_Dealer?.Invoke(_dealer); // NOTE: DONT MOVE THIS ABOVE onDeath?.Invoke() !!!

			if (hurtSprite != null && ShowHurtSpriteOnDeath)
			{
				spriteRenderers[0].sprite = hurtSprite;
			}
		}

		if (destroyOnDeath)
		{
			Invoke(nameof(DestroySelf), despawnTime);
		}
	}

	public void DestroySelf()
	{
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Applies health to receivers current health
	/// </summary>
	/// <param name="_amount"></param>
	public void RestoreHealth(float _amount, bool _heartPickup = false)
	{
		if (_amount <= 0 || IsDead) { return; }

		if (_heartPickup && CurrentHealth + _amount < maxHealth)
		{
			_amount = maxHealth;
		}

		CurrentHealth += _amount;

		if (CurrentHealth >= maxHealth)
		{
			CurrentHealth = maxHealth;
		}

		OnHealthUpdated?.Invoke();
	}

	public void ReviveAndRestoreHealth(float _restoredHealth, bool _makeInvincible)
	{
		IsDead = false;
		CurrentHealth = _restoredHealth;
		OnRevive?.Invoke();
		OnHealthUpdated?.Invoke();

		if (_makeInvincible)
		{
			invincible = true;
			invincibilityFramesDuration = reviveInvincibilityDuration;
		}
	}

	void RemoveHurtSprite()
	{
		OnHurtSpriteRemoved?.Invoke(gameObject);
		//animator.enabled = true;
	}

	public void StartInvulnerability()
	{
		InvulnerabilityCount++;
	}

	public void StopInvulnerability()
	{
		InvulnerabilityCount--;
		if (InvulnerabilityCount < 0)
			InvulnerabilityCount = 0;
	}

	public void DamageObjectWithoutTriggeringEvents(float _damageAmount, GameObject _senderObject, GameObject _dealerObject)
	{
		HandleDamage(_damageAmount, _senderObject, false);
		if (CurrentHealth == 0) HandleDeath(_dealerObject, _senderObject, false, false);
	}

	public void Kill()
	{
		if (gameObject.name == "Dummy") return;

		InvulnerabilityCount = 0;
		invincible = false;
		HandleDamage(CurrentHealth + 1, gameObject);
		HandleDeath(gameObject, gameObject);
	}

	public void Kill(GameObject _sender, GameObject _dealer, bool _sendKilledEvent = true, bool _triggerEvents = true)
	{
		if (gameObject.name == "Dummy") return;

		InvulnerabilityCount = 0;
		invincible = false;
		HandleDamage(CurrentHealth + 1, _sender, _triggerEvents);
		HandleDeath(_dealer, _sender, _sendKilledEvent, _triggerEvents);
	}

	public void CheckIfDamageMiss(GameObject _damageDealer)
	{
		if (chanceToMiss > 0)
		{
			if (!MissedCheckerColliders.ContainsKey(_damageDealer))
			{
				float randomNumber = Random.Range(1f, 100f);

				bool missed = false;
				if (randomNumber < ChanceToMiss)
				{
					missed = true;
					OnDamageMissed.Invoke(this);
				}
				MissedCheckerColliders.Add(_damageDealer, missed);
			}
		}
	}

	public void OnObjectSpawn()
	{
		IsDead = false;
		CurrentHealth = MaxHealth;
		InvulnerabilityCount = 0;
		invincible = false;
		if (animator) animator.enabled = true;
	}
}
