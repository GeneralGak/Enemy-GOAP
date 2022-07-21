using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_TestMovement : BaseAction
{
    [SerializeField] float jumpHeight = 1;
    [SerializeField] float jumpDuration = 1;

    public override float Cost()
    {
        return 0f;
    }
    
    protected override void Init()
    {
        
    }

    public override void Tick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            enemy.Movement.MoveToPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButtonDown(1))
        {
            enemy.Movement.JumpToPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), jumpHeight, jumpDuration);
        }
    }

    public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        return CBS_WeightHelper.GoTowards(_rayDirection, _goalDirection);
    }
}
