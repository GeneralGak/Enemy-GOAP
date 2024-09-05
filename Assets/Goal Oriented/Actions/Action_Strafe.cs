using UnityEngine;

public class Action_Strafe : BaseAction
{
    [SerializeField] private float baseCost = 10f;
    [SerializeField] private float flipStrafeDirRange = 0.4f;

    private bool strafingClockwise;
    private Vector2 strafeDir;

    public override void Init()
    {
        if (Random.Range(0, 2) == 0)
        {
            strafingClockwise = true;
        }
        else
        {
            strafingClockwise = false;
        }
    }

    public override float Cost()
    {
        return baseCost;
    }

    public override void Begin()
    {
        enemy.Animator.SetTrigger("DoWalk");
        enemy.Steering.CBS.OnDangerCheck.AddListener(CheckDangersWhileStrafing);
        enemy.Movement.FollowTarget(enemy.Target.transform, false);

        base.Begin();
    }

    public override void Tick()
    {

    }

    public override void End()
    {
        enemy.Movement.StopMovement();
        enemy.Steering.CBS.OnDangerCheck.RemoveListener(CheckDangersWhileStrafing);
        base.End();
    }

    public override float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        strafeDir = Helper.RotateVectorByAngleRadians(_goalDirection, (strafingClockwise ? -Mathf.PI : Mathf.PI) / 2f);
        return CBS_WeightHelper.Strafe(_rayDirection, strafeDir);
    }

    void CheckDangersWhileStrafing(Vector2 _directionToDanger, Vector2 _chosenDirection)
    {
        float angleBetweenChosenAndDanger = Vector2.Angle(_directionToDanger, _chosenDirection);

        if (_directionToDanger.magnitude < flipStrafeDirRange && angleBetweenChosenAndDanger < 45)
        {
            strafingClockwise = !strafingClockwise;
        }
    }
}
