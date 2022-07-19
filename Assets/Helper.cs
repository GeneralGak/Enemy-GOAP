using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Helper
{
	/// <summary>
	/// Creates a direction Vector based on angle around target
	/// </summary>
	/// <param name="_angleInDegrees"></param>
	/// <param name="_angleIsGlobal"></param>
	/// <returns></returns>
	public static Vector2 DirectionFromAngle(float _angleInDegrees, Transform _target, bool _angleIsGlobal)
	{
		if (!_angleIsGlobal)
		{
			_angleInDegrees -= _target.eulerAngles.z;
		}
		return new Vector2(Mathf.Sin(_angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(_angleInDegrees * Mathf.Deg2Rad));
	}

	public static Vector3 DirectionFromRotation(Quaternion _rotation)
    {
		return _rotation * Vector3.right;
    }

	/// <summary>
	/// Rotates gameObject towards a target Vector
	/// </summary>
	public static void RotateObjectInTargetDirection(Vector3 _targetVector, Transform _target, float _rotationSpeed)
	{
		_targetVector.z = _target.root.position.z;

		Vector3 directionVector = _targetVector - _target.root.position;

		Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionVector);

		_target.rotation = Quaternion.RotateTowards(_target.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
	}

	public static Vector2 RotateVectorByAngleRadians(Vector2 _startDir, float _angle)
	{
		Vector2 newDir;
		newDir.x = _startDir.x * Mathf.Cos(_angle) - _startDir.y * Mathf.Sin(_angle);
		newDir.y = _startDir.x * Mathf.Sin(_angle) + _startDir.y * Mathf.Cos(_angle);

		return newDir;
	}

	public static bool PredictAim(Vector3 _targetPos, Vector3 _shooterPos, Vector3 _targetVelocity, float _projectileSpeed, out Vector2 result, GameObject aimPredictionPoint)
	{
		_targetVelocity.Normalize();

		if (_targetVelocity.sqrMagnitude <= 0f)
		{
			result = _targetPos - _shooterPos;
			aimPredictionPoint.transform.position = _targetPos;
			return false;
		}
		else
		{
			Vector3 targetToShooter = _shooterPos - _targetPos;
			float distanceToTargetSquared = targetToShooter.sqrMagnitude;
			float distanceToTarget = targetToShooter.magnitude;
			Vector3 targetToBulletNormal = targetToShooter / distanceToTarget;
			float targetSpeed = _targetVelocity.magnitude;
			float targetSpeedSquared = _targetVelocity.sqrMagnitude;
			Vector3 targetVelocityNormal = _targetVelocity / targetSpeed;
			float projectileSpeedSquared = _projectileSpeed * _projectileSpeed;
			float cosTheta = Vector3.Dot(targetToBulletNormal, targetVelocityNormal);

			float offsetSquaredPart = 2 * distanceToTarget * targetSpeed * cosTheta;
			offsetSquaredPart *= offsetSquaredPart;
			float offset = Mathf.Sqrt(offsetSquaredPart + 4 * (projectileSpeedSquared - targetSpeedSquared) * distanceToTargetSquared);

			float estimatedTravelTime = (-2 * distanceToTarget * targetSpeed * cosTheta + offset) / (2 * (projectileSpeedSquared - targetSpeedSquared));

			if (estimatedTravelTime < 0 || estimatedTravelTime == float.NaN || float.IsInfinity(estimatedTravelTime))
			{
				result = _targetPos - _shooterPos;
				aimPredictionPoint.transform.position = _targetPos;
				return false;
			}
			else
			{
				//predictionInfluence = Mathf.Clamp(predictionInfluence, 0, 1);
				Vector3 predictionPos = _targetPos + targetVelocityNormal * targetSpeed * estimatedTravelTime;
				result = predictionPos - _shooterPos;
				aimPredictionPoint.transform.position = predictionPos;
				return true;
				//return Vector3.Lerp(projectileDirection.normalized, predictionDir.normalized, predictionInfluence);
			}
		}
	}

	/// <summary>
	/// Method for predicting where player will go when shooting projectiles
	/// Source: https://www.youtube.com/watch?v=2zVwug_agr0
	/// </summary>
	/// <param name="a">Target position</param>
	/// <param name="b">Shooter position</param>
	/// <param name="vA">Target velocity</param>
	/// <param name="sB">Projectile speed</param>
	/// <param name="result">Normalized interception direction</param>
	/// <returns></returns>
	public static bool InterceptionDirection(Vector2 a, Vector2 b, Vector2 vA, float sB, out Vector2 result, GameObject aimPredictionPoint, out Vector2 interceptionPos)
    {
		interceptionPos = Vector2.zero;
		var sA = vA.magnitude;

		// Handle projectile slower than target
		if (sB <= sA)
        {
			sB = sA * 1.1f;
		}

		var aToB = b - a;
		var dC = aToB.magnitude;
		var alpha = Vector2.Angle(aToB, vA) * Mathf.Deg2Rad;
		var r = sA / sB;

		if (SolveQuadratic(1 - r * r, 2 * r * dC * Mathf.Cos(alpha), -(dC * dC), out var root1, out var root2) == 0)
		{
			result = Vector2.zero;
			return false;
        }

		var dA = Mathf.Max(root1, root2);
		var t = dA / sB;
		var c = a + vA * t;

		interceptionPos = c;

		if (aimPredictionPoint) aimPredictionPoint.transform.position = c;

		result = (c - b).normalized;
		return true;
    }

	public static int SolveQuadratic(float a, float b, float c, out float root1, out float root2)
    {
		var descriminant = b * b - 4 * a * c;

		if(descriminant < 0)
        {
			root1 = Mathf.Infinity;
			root2 = -root1;
			return 0;
        }

		root1 = (-b + Mathf.Sqrt(descriminant)) / (2 * a);
		root2 = (-b - Mathf.Sqrt(descriminant)) / (2 * a);

		return descriminant > 0 ? 2 : 1;
	}

	public static Vector2 Lerp3(Vector2 a, Vector2 b, Vector2 c, float t)
	{
		if (t <= 0.5f)
		{
			return Vector2.Lerp(a, b, t * 2f);
		}
		else
		{
			return Vector2.Lerp(b, c, (t * 2f) - 1);
		}
	}

	public static float Lerp3(float a, float b, float c, float t)
	{
		if (t <= 0.5f)
		{
			return Mathf.Lerp(a, b, t * 2f);
		}
		else
		{
			return Mathf.Lerp(b, c, (t * 2f) - 1);
		}
	}

	public static void SetEventSystemSelectedObject(GameObject go)
	{
		EventSystem.current.SetSelectedGameObject(go);
	}
		
	/// <summary>
	/// Custom method for aiming stuff, probably dont use for anything else!
	/// </summary>
	/// <param name="_vector2"></param>
	/// <returns></returns>
	public static float Vector2ToDegrees(Vector2 _vector2)
	{
		if (_vector2.x < 0)
		{
			return 360 - (Mathf.Atan2(_vector2.x, _vector2.y) * Mathf.Rad2Deg * -1) - 90;
		}
		else
		{
			return Mathf.Atan2(_vector2.x, _vector2.y) * Mathf.Rad2Deg - 90;
		}
	}

	public static int WrapInt(int _value, int _listSize)
    {
		int result = _value;
		if(_value > _listSize - 1) { result = 0; }
		else if(_value < 0) { result = _listSize - 1; }
		return result;
    }
}
