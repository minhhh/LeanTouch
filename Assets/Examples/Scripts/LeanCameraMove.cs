using UnityEngine;

namespace Lean.Touch
{
	// This script allows you to track & pedestral this GameObject (e.g. Camera) based on finger drags
	public class LeanCameraMove : MonoBehaviour
	{
		[Tooltip("The camera the movement will be done relative to")]
		public Camera Camera;

		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreGuiFingers = true;

		[Tooltip("Ignore fingers if the finger count doesn't match? (0 = any)")]
		public int RequiredFingerCount;

		[Tooltip("The distance from the camera the world drag delta will be calculated from (this only matters for perspective cameras)")]
		public float Distance = 1.0f;

		[Tooltip("The sensitivity of the movement, use -1 to invert")]
		public float Sensitivity = 1.0f;

		protected virtual void LateUpdate()
		{
			// Make sure the camera exists
			if (LeanTouch.GetCamera(ref Camera, gameObject) == true)
			{
				// Get the fingers we want to use
				var fingers = LeanTouch.GetFingers(IgnoreGuiFingers, RequiredFingerCount);

				// Get the world delta of all the fingers
				var worldDelta = LeanGesture.GetWorldDelta(fingers, Distance, Camera);

				// Pan the camera based on the world delta
				transform.position -= worldDelta * Sensitivity;
			}
		}
	}
}