using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIVision : MonoBehaviour
{
	[SerializeField] private LayerMask targetMask;
	[SerializeField] private LayerMask obstacleMask;
	[SerializeField] [Range(0, 360)] private float viewAngle;
	[SerializeField] private float viewRadius = 3f;
	[SerializeField] private float CoroutineDelayTime = 0.2f;

	private List<GameObject> visibleTargets = new List<GameObject>();
	private ITargetSorting targetSorting;
	private bool breakLoop;
	private AwarenessSystem awarenesSystem;

	public UnityEvent<GameObject> OnTargetSpotted { get; set; }
	public List<GameObject> VisibleTargets { get { return visibleTargets; } }
	public float ViewAngle { get { return viewAngle; } }
	public float ViewRadius { get { return viewRadius; } }
	public bool IsVisionActive { get; private set; } = true;
	public GameObject Target { get; private set; }


	private void OnEnable()
	{
		targetSorting = new EnemyTargetSorting();
		breakLoop = false;
		StartCoroutine(FindTargetsWithDelay(CoroutineDelayTime));
	}

	/// <summary>
	/// Used to run a methode like in FixedUpdate, but where you set the time frame
	/// </summary>
	/// <param name="_delay"></param>
	/// <returns></returns>
	IEnumerator FindTargetsWithDelay(float _delay)
	{
		while (!breakLoop)
		{
			yield return new WaitForSeconds(_delay);
			FindVisibleTargets();
		}
	}

	public void ResetVariables()
	{
		Target = null;
		visibleTargets.Clear();
	}

	/// <summary>
	/// Set or change the layer used to find GameObjects
	/// </summary>
	/// <param name="_targetLayer"></param>
	public void SetTargetLayer(LayerMask _targetLayer)
	{
		if(_targetLayer == obstacleMask)
		{
			Debug.LogError("Target layer should not be the same as obstacle layer");
		}
		else
		{
			targetMask = _targetLayer;
		}
	}

	/// <summary>
	/// Set or change the layer used for obstacles to "Block vision"
	/// </summary>
	/// <param name="_obstacleLayer"></param>
	public void SetObstacleLayer(LayerMask _obstacleLayer)
	{
		if (_obstacleLayer == targetMask)
		{
			Debug.LogError("Obstacle layer should not be the same as target layer");
		}
		else
		{
			obstacleMask = _obstacleLayer;
		}
	}

	/// <summary>
	/// For changing AI view range under runtime
	/// Can also make Attack range the same as the view range
	/// </summary>
	/// <param name="_newViewRadius"></param>
	/// <param name="_attackRadiusSameAsView"></param>
	public void SetViewRadius(float _newViewRadius)
	{
		if (_newViewRadius > 0)
		{
			viewRadius = _newViewRadius;
		}
		else
		{
			viewRadius = 0;
		}
	}

	/// <summary>
	/// For changing AI view angle under runtime
	/// </summary>
	/// <param name="_newViewAngle"></param>
	public void SetViewAngle(float _newViewAngle)
	{
		if (_newViewAngle > 0)
		{
			viewAngle = _newViewAngle;

			if (viewAngle > 360)
			{
				viewAngle = 360;
			}
		}
		else
		{
			_newViewAngle = 0;
		}
	}

	public void StopAIVision()
	{
		breakLoop = true;
		StopAllCoroutines();
		IsVisionActive = false;
	}

	public void StartAIVision()
	{
		breakLoop = false;
		StartCoroutine(FindTargetsWithDelay(CoroutineDelayTime));
		IsVisionActive = true;
	}

	/// <summary>
	/// Checks if other gameObjects are whitin a detection radius and can be hit by a RayCast
	/// </summary>
	private void FindVisibleTargets()
	{
		visibleTargets.Clear();
		Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);

		// Goes through all target gameObjects within detection circle
		for (int i = 0; i < targetsInViewRadius.Length; i++)
		{
			GameObject target = targetsInViewRadius[i].gameObject;

			if(visibleTargets.Contains(target))
			{
				continue;
			}

			Vector2 directionToTarget = (target.transform.position - transform.position).normalized;

			// Checks if target is within a set view angle
			if (Vector2.Angle(transform.up, directionToTarget) < viewAngle / 2)
			{
				float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

				// Checks if another object is in front of target
				if (!CheckIfTargetObstructed(directionToTarget, distanceToTarget))
				{
					// TODO: Find a better way to work around child components
					visibleTargets.Add(target.transform.root.gameObject);
				}
			}
		}

		GameObject tmpTarget = targetSorting.FindMainTarget(visibleTargets, Target);

		if (visibleTargets.Count > 0 && Target == null)
		{
			OnTargetSpotted?.Invoke(tmpTarget);
		}

		Target = tmpTarget;
	}

	public bool CheckIfTargetObstructed(Vector2 _directionToTarget, float _distanceToTarget)
	{
		return Physics2D.Raycast(transform.position, _directionToTarget, _distanceToTarget, obstacleMask);
	}
}
