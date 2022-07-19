using System.Collections.Generic;
using UnityEngine;

public class EnemyTargetSorting : MonoBehaviour, ITargetSorting
{
    /// <summary>
    /// Goes through a list of gameObjects to return a specific target
    /// </summary>
    /// <param name="_visibleObjects"></param>
    /// <param name="_target"></param>
    /// <returns></returns>
	public GameObject FindMainTarget(List<GameObject> _visibleObjects, GameObject _target)
	{
        GameObject tmpTarget = null;

        // Return NULL if given list is empty
        if (_visibleObjects.Count == 0)
		{
            return null;
		}

        // Goes through the list given in the parameter
        foreach (var item in _visibleObjects)
        {
            // If the given target is NULL then the first object in the list is returned
            if (_target == null)
            {
                return item;
            }
            else
            {
                // If the given target is not NULL, check if an object in the list is same as target and return it
                if (item == _target)
                {
                    return item;
                }
                // Add first object in list as potential new target
                else if(tmpTarget == null)
                {
                    tmpTarget = item;
                }
            }
        }

        // Returns a new target if old target is not found in list
        return tmpTarget;
    }
}
