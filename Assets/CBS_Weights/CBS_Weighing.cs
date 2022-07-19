using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CBS_Weighing : MonoBehaviour
{
    protected bool isUsingDanger;

    public bool IsUsingDanger { get { return isUsingDanger; } }
    public string Name { get; protected set; }

    public abstract void SetVariables(ContextBasedSteeringBehavior _steeringBehavior);
    public abstract void WeightUpdate(Vector3 _destination);
    public abstract float GetWeight(Vector2 _rayDirection, Vector2 _goalDirection);

    public abstract void CheckDangerCollision(Vector2 _directionToDanger, Vector2 _chosenDirection);
}
