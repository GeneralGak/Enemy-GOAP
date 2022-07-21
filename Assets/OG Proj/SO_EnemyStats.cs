using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Custom/EnemyStats")]
public class SO_EnemyStats : ScriptableObject
{
    [Title("General", TitleAlignment = TitleAlignments.Centered)]
    public string enemyName;
    public float maxHealth = 3;
    public int currencyDropAmount = 1;
    public float speed = 0.5f;
    public float chaseSpeed = 1.5f;
    public float moveAwayFromPlayerDistance = 0.4f;
    public float maxRandomDistanceAdder = 0.2f;
    public float accelerationTime = 1f;
    public float attackWindUpTime = 0.4f;
    public float wanderRadius = 0.7f; // Radius to choose new location to wander to every idle
    public float searchRadius = 1f;

    [Title("AI Vision", TitleAlignment = TitleAlignments.Centered)]
    public float viewRadius = 1;
    public float alertViewRadius = 1.5f;
    public float turnSpeed = 200;
    [Range(0, 360)] public float viewAngle = 150f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Title("State Machine", TitleAlignment = TitleAlignments.Centered)]
    public RuntimeAnimatorController baseStateMachine;
    public RuntimeAnimatorController arenaStateMachine;

    public float attackRange;
    public float distanceFromDestination; // How close to destination before registered as reached
    public float idleTime; // Time standing still between walking (patrolling)
    public float loseTargetTime;
    public float searchTime; // Cooldown before enemy gives up searching
}
