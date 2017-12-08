using UnityEngine;

namespace Lean.Touch
{
	// This script allows you to change the color of the Renderer attached to the current GameObject
	public class LeanSelectableRendererColor : LeanSelectableBehaviour
	{
		[Tooltip("Automatically read the DefaultColor from the Renderer.material?")]
		public bool AutoGetDefaultColor;

		[Tooltip("The default color given to the Renderer.material")]
		public Color DefaultColor = Color.white;

		[Tooltip("The color given to the Renderer.material when selected")]
		public Color SelectedColor = Color.green;

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Start();
		}
#endif

		protected virtual void Start()
		{
			if (AutoGetDefaultColor == true)
			{
				var renderer = GetComponent<Renderer>();

				DefaultColor = renderer.sharedMaterial.color;
			}
		}

		protected override void OnSelect(LeanFinger finger)
		{
			ChangeColor(SelectedColor);
		}

		protected override void OnDeselect()
		{
			ChangeColor(DefaultColor);
		}

		private void ChangeColor(Color color)
		{
			var renderer = GetComponent<Renderer>();

			// Clone material and change color
			renderer.material.color = color;
		}
	}
}