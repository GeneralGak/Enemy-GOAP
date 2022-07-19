using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EAIState { Idle, Pursue, Flee, Attacking, Wander, InAttackRange }

public class EnemyAI : MonoBehaviour
{
	public float attackCooldown;
	public float attackRange;
	public Animator behaviourTimeline;
	
	public EAIState state;

	public UnityEvent OnDeath, OnShoot, OnHit;

	private AIVision vision;
	private ContextBasedSteeringBehavior steering;
	private float elapsedAttackCooldown = 0;
	private int stateNameHash;
	private GameObject fokusTarget;


	private void Awake()
	{
		vision = GetComponent<AIVision>();
		steering = GetComponent<ContextBasedSteeringBehavior>();
	}

	// Update is called once per frame
	void Update()
	{
		elapsedAttackCooldown += Time.deltaTime;

		if (stateNameHash != behaviourTimeline.GetCurrentAnimatorStateInfo(0).fullPathHash)
		{
			ResetTriggers();

			stateNameHash = behaviourTimeline.GetCurrentAnimatorStateInfo(0).fullPathHash;
		}
	}


	public void SwitchToState(EAIState _newState)
	{
		switch (_newState)
		{
			case EAIState.Idle:
				behaviourTimeline.SetTrigger("Idle");
				break;
			case EAIState.Pursue:
				behaviourTimeline.SetTrigger("Chase");
				break;
			case EAIState.Flee:
				behaviourTimeline.SetTrigger("Flee");
				break;
			case EAIState.Attacking:
				behaviourTimeline.SetTrigger("Attacking");
				break;
			case EAIState.Wander:
				behaviourTimeline.SetTrigger("Wander");
				break;
			case EAIState.InAttackRange:
				behaviourTimeline.SetTrigger("InRange");
				break;
			default:
				break;
		}
	}

	public void RandomStateSwitch(EAIState _newState) 
	{ 
		int number = Random.Range(0, 10);

		if(number >= 5)
		{
			SwitchToState(_newState);
		}
	}

	public void CheckIfTargetInVisionRange(EAIState _newState)
	{
		if (vision.Target != null)
		{
			fokusTarget = vision.Target;
			SwitchToState(_newState);
		}
	}

	public void CheckTargetInRange(EAIState _newState)
	{
		if(Vector2.Distance(fokusTarget.transform.position, transform.position) <= attackRange)
		{
			SwitchToState(_newState);
		}
	}

	public void CheckTargetOutOfRange(EAIState _newState)
	{
		if (Vector2.Distance(fokusTarget.transform.position, transform.position) >= attackRange)
		{
			SwitchToState(_newState);
		}
	}

	public void Death()
	{
		OnDeath?.Invoke();
	}

	public void Shoot()
	{
		OnShoot?.Invoke();
	}

	public void TakeDamage()
	{
		OnHit?.Invoke();

	}

	public void LightAttack()
	{

	}

	public void HeavyAttack()
	{

	}

	private void CheckIfCanAttack()
	{
		if(elapsedAttackCooldown >= attackCooldown)
		{
			behaviourTimeline.SetTrigger("Attacking");
			elapsedAttackCooldown = 0;
		}
	}

	public void ChaseMovement()
	{
		steering.TransformGoal = fokusTarget.transform;
		steering.SetNewWeights(ContextBasedWeights.CBS_NavMeshPath, ContextBasedWeights.CBS_ChaseWeight);
		steering.StartMovement();
	}

	public void CircleMovement()
	{
		steering.SetNewWeights(ContextBasedWeights.CBS_StrafeWeight, ContextBasedWeights.CBS_FleeWeight);
		steering.StartMovement();
	}

	public void WanderMovement()
	{
		steering.SetNewWeights(ContextBasedWeights.CBS_Wander);
		steering.StartMovement();
	}

	public void Shooting()
	{
		Debug.Log("BANG!!");
	}

	private void ResetTriggers()
	{
		behaviourTimeline.ResetTrigger("InRange");
		behaviourTimeline.ResetTrigger("Wander");
		behaviourTimeline.ResetTrigger("Flee");
		behaviourTimeline.ResetTrigger("Chase");
		behaviourTimeline.ResetTrigger("Attacking");
		behaviourTimeline.ResetTrigger("Idle");
	} 
}
