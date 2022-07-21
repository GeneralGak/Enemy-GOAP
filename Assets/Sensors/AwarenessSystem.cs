using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrackedTarget
{
    public GameObject Detectable;
    public Vector3 RawPosition;

    public float LastSensedTime = -1f;
    public float Awareness; // 0     = not aware (will be culled); 
                            // 0-1   = rough idea (no set location); 
                            // 1-2   = likely target (location)
                            // 2     = fully detected

    public bool UpdateAwareness(GameObject target, Vector3 position, float awareness, float minAwareness)
    {
        var oldAwareness = Awareness;

        if (target != null)
            Detectable = target;
        RawPosition = position;
        LastSensedTime = Time.time;
        Awareness = Mathf.Clamp(Mathf.Max(Awareness, minAwareness) + awareness, 0f, 2f);

        if (oldAwareness < 2f && Awareness >= 2f)
            return true;
        if (oldAwareness < 1f && Awareness >= 1f)
            return true;
        if (oldAwareness <= 0f && Awareness >= 0f)
            return true;

        return false;
    }

    public bool DecayAwareness(float decayTime, float amount)
    {
        // detected too recently - no change
        if ((Time.time - LastSensedTime) < decayTime)
            return false;

        var oldAwareness = Awareness;

        Awareness -= amount;

        if (oldAwareness >= 2f && Awareness < 2f)
            return true;
        if (oldAwareness >= 1f && Awareness < 1f)
            return true;
        return Awareness <= 0f;
    }
}

public class AwarenessSystem : MonoBehaviour
{
    [SerializeField] AnimationCurve visionSensitivity;
    [SerializeField] float visionMinimumAwareness = 1f;
    [SerializeField] float visionAwarenessBuildRate = 5f;

    [SerializeField] float beingDamagedMinimumAwareness = 0f;
    [SerializeField] float beingDamagedAwarenessBuildRate = 10f;

    [SerializeField] float awarenessDecayDelay = 0.1f;
    [SerializeField] float awarenessDecayRate = 0.1f;

    Dictionary<GameObject, TrackedTarget> targets = new Dictionary<GameObject, TrackedTarget>();

    public Dictionary<GameObject, TrackedTarget> ActiveTargets { get { return targets; } }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> toCleanup = new List<GameObject>();
        foreach (var targetGO in targets.Keys)
        {
            if (targets[targetGO].DecayAwareness(awarenessDecayDelay, awarenessDecayRate * Time.deltaTime))
            {
                if (targets[targetGO].Awareness <= 0f)
                {
                    toCleanup.Add(targetGO);
                }
            }
        }

        // cleanup targets that are no longer detected
        foreach (var target in toCleanup)
            targets.Remove(target);
    }

    void UpdateAwareness(GameObject targetGO, Vector3 position, float awareness, float minAwareness)
    {
        // not in targets
        if (!targets.ContainsKey(targetGO))
            targets[targetGO] = new TrackedTarget();

        // update target awareness
        targets[targetGO].UpdateAwareness(targetGO, position, awareness, minAwareness);
    }

    public void ReportCanSee(GameObject seenGO)
    {
        // determine where the target is in the field of view
        var vectorToTarget = (seenGO.transform.position - transform.position).normalized;
        var dotProduct = Vector3.Dot(vectorToTarget, transform.up);

        // determine the awareness contribution
        var awareness = visionSensitivity.Evaluate(dotProduct) * visionAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(seenGO.gameObject, seenGO.transform.position, awareness, visionMinimumAwareness);
    }

    public void ReportBeingDamaged()
	{

	}
}
