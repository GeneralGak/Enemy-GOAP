using UnityEngine;

public class Action_TestMovement : BaseAction
{
    [SerializeField] float jumpHeight = 1;
    [SerializeField] float jumpDuration = 1;

    public override float Cost()
    {
        return 0f;
    }
    
    public override void Init()
    {
        
    }

    public override void Tick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            enemy.Movement.MoveToPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        return CBS_WeightHelper.GoTowards(_rayDirection, _goalDirection);
    }
}
