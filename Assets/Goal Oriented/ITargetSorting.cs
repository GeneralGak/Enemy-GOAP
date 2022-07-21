using System.Collections.Generic;
using UnityEngine;

public interface ITargetSorting
{
	public GameObject FindMainTarget(List<GameObject> _visibleObjects, GameObject _target);
}
