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
    Enemy enemy;
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
    public float JumpHeight { get; set; } = 1f;
    public Vector2 MoveDir { get; set; }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
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
            
            rb.MovePosition(pos);
            
            if(elapsedJumpTime < jumpDuration)
            {
                enemy.SpriteObject.transform.localPosition = new Vector3(0, y);
            }
            else
            {
                enemy.SpriteObject.transform.localPosition = Vector3.zero;
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

        MoveDir = targetPosition - (Vector2)transform.position;

        if (UseCBS)
        {
            MoveDir = enemy.CBS.GetDir(MoveDir);
        }

        MoveDir.Normalize();

        rb.MovePosition(transform.position + (Vector3)(MoveDir * MovementSpeed * Time.fixedDeltaTime));

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
        index = 0;
        jumpStartPos = transform.position;
        jumpTargetPos = _targetPos;
        jumpDuration = _jumpDuration;
        JumpHeight = _jumpHeight;
    }

    public void StopMovement()
    {
        move = false;
    }
}
