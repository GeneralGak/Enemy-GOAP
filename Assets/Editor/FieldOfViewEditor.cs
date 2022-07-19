using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIVision))]
class FieldOfViewEditor : Editor
{
	private void OnSceneGUI()
	{
		AIVision detector = (AIVision)target;
		Handles.color = Color.yellow;

		Vector3 viewAngleA = Helper.DirectionFromAngle(-detector.ViewAngle / 2, detector.transform, false);
		Vector3 viewAngleB = Helper.DirectionFromAngle(detector.ViewAngle / 2, detector.transform, false);

		Handles.DrawWireArc(detector.transform.position, Vector3.forward, viewAngleA, -detector.ViewAngle, detector.ViewRadius);
		Handles.DrawLine(detector.transform.position, detector.transform.position + viewAngleA * detector.ViewRadius);
		Handles.DrawLine(detector.transform.position, detector.transform.position + viewAngleB * detector.ViewRadius);
	}
}
