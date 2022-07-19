using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CBS_WeightHelper
{
    public static float GoTowards(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        return Mathf.Max(0, 0.5f + Vector2.Dot(_rayDirection, _goalDirection.normalized)) / 1.5f;
    }

    public static float GoAway(Vector2 _rayDirection, Vector2 _goalDirection)
    {
        return Mathf.Max(0, 0.5f + Vector2.Dot(_rayDirection, _goalDirection.normalized * -1)) / 1.5f;
    }

    public static float Strafe(Vector2 _rayDirection, Vector2 _strafeDir)
    {
        return Vector2.Dot(_rayDirection, _strafeDir.normalized);
    }
}
