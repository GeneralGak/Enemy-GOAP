using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementType
{
    Walking,
    Jumping
}

public class EnemyMovement : MonoBehaviour
{
    ContextBasedSteeringBehavior cbs;
    Rigidbody2D rb;
    bool move;
    bool doJump;
    Transform followTarget;
    Vector2 targetPos;
    Vector2 jumpTargetPos;
    Vector2 jumpStartPos;
    float elapsedJumpTime;
    float jumpDuration;
    float index;

    public float CloseEnoughThresholdDistance { get; set; } = 0.05f;
    public bool UseCBS { get; set; } = true;
    public float MovementSpeed { get; set; } = 2f;
    public float MaxJumpLength { get; set; }
    public float JumpHeight { get; set; }

    private void Awake()
    {
        cbs = GetComponent<ContextBasedSteeringBehavior>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (move)
        {
            Move();
        }
        else if (doJump)
        {
            elapsedJumpTime += Time.deltaTime;
            index += Time.deltaTime;
            float y = JumpHeight * Mathf.Sin(index * (Mathf.PI / jumpDuration));
            
            Vector2 pos = Vector2.Lerp(jumpStartPos, jumpTargetPos, elapsedJumpTime / jumpDuration);
            
            if(elapsedJumpTime < jumpDuration)
            {
                rb.MovePosition(new Vector2(pos.x, pos.y + y));
            }
            else
            {
                rb.MovePosition(pos);
                doJump = false;
            }
        }
    }

    void Move()
    {
        Vector2 targetPosition;

        if (followTarget)
        {
            targetPosition = followTarget.position;
        }
        else
        {
            targetPosition = targetPos;
        }

        Vector2 moveDir = targetPosition - (Vector2)transform.position;

        if (UseCBS)
        {
            moveDir = cbs.GetDir(moveDir);
        }

        moveDir.Normalize();

        rb.MovePosition(transform.position + (Vector3)(moveDir * MovementSpeed * Time.fixedDeltaTime));

        float distToTarget;

        if (followTarget)
        {
            distToTarget = Vector2.Distance(followTarget.transform.position, transform.position);
        }
        else
        {
            distToTarget = Vector2.Distance(targetPos, transform.position);
        }

        if(distToTarget < CloseEnoughThresholdDistance)
        {
            rb.MovePosition(targetPosition);
            move = false;
        }
    }

    public void MoveToPosition(Vector2 _targetPos)
    {
        move = true;
        followTarget = null;
        targetPos = _targetPos;
    }

    public void FollowTarget(Transform _followTarget)
    {
        move = true;
        followTarget = _followTarget; 
    }

    public void WalkTowards(Vector2 _targetPos)
    {
        WalkInDirection(_targetPos - (Vector2)transform.position);
    }

    public void WalkInDirection(Vector2 _dir)
    {
        
    }

    public void JumpToPosition(Vector2 _targetPos, float _jumpHeight, float _jumpDuration)
    {
        doJump = true;
        elapsedJumpTime = 0;
        jumpStartPos = transform.position;
        jumpTargetPos = _targetPos;
        jumpDuration = _jumpDuration;
    }

    public void StopMovement()
    {
        move = false;
    }
}
