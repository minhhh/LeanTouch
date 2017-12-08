using UnityEngine;
using System.Collections.Generic;

namespace Lean.Touch
{
	// This script allows you to select multiple LeanSelectable components while a finger is down
	public class LeanPressSelect : MonoBehaviour
	{
		public enum SelectType
		{
			Raycast3D,
			Overlap2D
		}

		public enum SearchType
		{
			GetComponent,
			GetComponentInParent,
			GetComponentInChildren
		}

		[Tooltip("Ignore fingers with StartedOverGui?")]
		public bool IgnoreGuiFingers = true;

		[Tooltip("Should the selected object automatically deselect if the selecting finger moves off it?")]
		public bool DeselectOnExit;

		public SelectType SelectUsing;

		[Tooltip("This stores the layers we want the raycast/overlap to hit (make sure this GameObject's layer is included!)")]
		public LayerMask LayerMask = Physics.DefaultRaycastLayers;

		[Tooltip("How should the selected GameObject be searched for the LeanSelectable component?")]
		public SearchType Search;

		[Tooltip("The currently selected LeanSelectables")]
		public List<LeanSelectable> CurrentSelectables;

		protected virtual void OnEnable()
		{
			// Hook events
			LeanTouch.OnFingerDown += FingerDown;
			LeanTouch.OnFingerSet  += FingerSet;
			LeanTouch.OnFingerUp   += FingerUp;
		}

		protected virtual void OnDisable()
		{
			// Unhook events
			LeanTouch.OnFingerDown -= FingerDown;
			LeanTouch.OnFingerSet  -= FingerSet;
			LeanTouch.OnFingerUp   -= FingerUp;
		}

		private void FingerDown(LeanFinger finger)
		{
			// Ignore this finger?
			if (IgnoreGuiFingers == true && finger.StartedOverGui == true)
			{
				return;
			}

			// Find the component under the finger
			var component = FindComponentUnder(finger);

			// Find the selectable associated with this component
			var selectable = FindSelectableFrom(component);

			// Select the found selectable with the selecting finger
			Select(finger, selectable);
		}

		private void FingerSet(LeanFinger finger)
		{
			if (DeselectOnExit == true)
			{
				// Run through all selected objects
				for (var i = CurrentSelectables.Count - 1; i >= 0; i--)
				{
					var currentSelectable = CurrentSelectables[i];

					// Is it valid?
					if (currentSelectable != null)
					{
						// Is this object associated with this finger?
						if (currentSelectable.SelectingFinger == finger)
						{
							// Find the component under the finger
							var component = FindComponentUnder(finger);

							// Find the selectable associated with this component
							var selectable = FindSelectableFrom(component);

							// If the associated object is no longer under the finger, deselect it
							if (selectable != currentSelectable)
							{
								Deselect(currentSelectable);
							}
						}
						// Deselect in case the association was lost
						else if (currentSelectable.SelectingFinger == null)
						{
							Deselect(currentSelectable);
						}
					}
					// Remove invalid
					else
					{
						CurrentSelectables.RemoveAt(i);
					}
				}
			}
		}

		private void FingerUp(LeanFinger finger)
		{
			for (var i = CurrentSelectables.Count - 1; i >= 0; i--)
			{
				var currentSelectable = CurrentSelectables[i];

				if (currentSelectable != null)
				{
					if (currentSelectable.SelectingFinger == finger || currentSelectable.SelectingFinger == null)
					{
						Deselect(currentSelectable);
					}
				}
				else
				{
					CurrentSelectables.RemoveAt(i);
				}
			}
		}

		public void Select(LeanFinger finger, LeanSelectable selectable)
		{
			// Something was selected?
			if (selectable != null && selectable.isActiveAndEnabled == true)
			{
				if (CurrentSelectables == null)
				{
					CurrentSelectables = new List<LeanSelectable>();
				}

				// Loop through all current selectables
				for (var i = CurrentSelectables.Count - 1; i >= 0; i--)
				{
					var currentSelectable = CurrentSelectables[i];

					if (currentSelectable != null)
					{
						// Already selected?
						if (currentSelectable == selectable)
						{
							return;
						}
					}
					else
					{
						CurrentSelectables.RemoveAt(i);
					}
				}

				// Not selected yet, so select it
				CurrentSelectables.Add(selectable);

				selectable.Select(finger);
			}
		}

		[ContextMenu("Deselect All")]
		public void DeselectAll()
		{
			// Loop through all current selectables and deselect if not null
			if (CurrentSelectables != null)
			{
				for (var i = CurrentSelectables.Count - 1; i >= 0; i--)
				{
					var currentSelectable = CurrentSelectables[i];

					if (currentSelectable != null)
					{
						currentSelectable.Deselect();
					}
				}

				// Clear
				CurrentSelectables.Clear();
			}
		}

		// Deselect the specified selectable, if it exists
		public void Deselect(LeanSelectable selectable)
		{
			// Loop through all current selectables
			if (CurrentSelectables != null)
			{
				for (var i = CurrentSelectables.Count - 1; i >= 0; i--)
				{
					var currentSelectable = CurrentSelectables[i];

					if (currentSelectable != null)
					{
						// Match?
						if (currentSelectable == selectable)
						{
							selectable.Deselect();

							CurrentSelectables.Remove(selectable);

							return;
						}
					}
					else
					{
						CurrentSelectables.RemoveAt(i);
					}
				}
			}
		}

		private Component FindComponentUnder(LeanFinger finger)
		{
			var component = default(Component);

			switch (SelectUsing)
			{
				case SelectType.Raycast3D:
				{
					// Get ray for finger
					var ray = finger.GetRay();

					// Stores the raycast hit info
					var hit = default(RaycastHit);

					// Was this finger pressed down on a collider?
					if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask) == true)
					{
						component = hit.collider;
					}
				}
				break;

				case SelectType.Overlap2D:
				{
					// Find the position under the current finger
					var point = finger.GetWorldPosition(1.0f);

					// Find the collider at this position
					component = Physics2D.OverlapPoint(point, LayerMask);
				}
				break;
			}

			return component;
		}

		private LeanSelectable FindSelectableFrom(Component component)
		{
			var selectable = default(LeanSelectable);

			if (component != null)
			{
				switch (Search)
				{
					case SearchType.GetComponent:           selectable = component.GetComponent          <LeanSelectable>(); break;
					case SearchType.GetComponentInParent:   selectable = component.GetComponentInParent  <LeanSelectable>(); break;
					case SearchType.GetComponentInChildren: selectable = component.GetComponentInChildren<LeanSelectable>(); break;
				}
			}

			return selectable;
		}
	}
}