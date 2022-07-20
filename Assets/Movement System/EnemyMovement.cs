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
            Vector2 moveDir = targetPos;

            if (followTarget)
            {
                moveDir = followTarget.position - transform.position;
            }

            if (UseCBS)
            {
                moveDir = cbs.GetDir(moveDir);
            }

            moveDir.Normalize();

            rb.MovePosition(transform.position + (Vector3)(moveDir * MovementSpeed * Time.fixedDeltaTime));
        }
        else if (doJump)
        {
            elapsedJumpTime += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(jumpStartPos, jumpTargetPos, elapsedJumpTime / jumpDuration));
            index += Time.deltaTime;
            float y = JumpHeight * Mathf.Sin(index * (Mathf.PI / jumpDuration));
            //spriteObject.transform.position = transform.position + new Vector3(0, (y + hoverHeight / 2f) + hoverHeightOffset, 0);
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
