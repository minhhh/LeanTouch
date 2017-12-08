using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
	// This component allows you to select this GameObject via another component
	public class LeanSelectable : MonoBehaviour
	{
		// Event signature
		[System.Serializable] public class LeanFingerEvent : UnityEvent<LeanFinger> {}

		[Tooltip("Should IsSelected temporarily return false if the selecting finger is still being held?")]
		public bool HideWithFinger;

		public bool IsSelected
		{
			get
			{
				// Hide IsSelected?
				if (HideWithFinger == true && isSelected == true && SelectingFinger != null)
				{
					return false;
				}

				return isSelected;
			}
		}

		// This stores the finger that began selection of this LeanSelectable
		// This will become null as soon as that finger releases, which you can detect via OnSelectUp
		[System.NonSerialized]
		public LeanFinger SelectingFinger;

		// Called when selection begins (finger = the finger that selected this)
		public LeanFingerEvent OnSelect;

		// Called when the selecting finger goes up (finger = the finger that selected this)
		public LeanFingerEvent OnSelectUp;

		// Called when this is deselected, if OnSelectUp hasn't been called yet, it will get called first
		public UnityEvent OnDeselect;

		// Is this selectable selected?
		[SerializeField]
		private bool isSelected;

		[ContextMenu("Select")]
		public void Select()
		{
			Select(null);
		}

		public void Select(LeanFinger finger)
		{
			isSelected      = true;
			SelectingFinger = finger;

			OnSelect.Invoke(finger);
		}

		[ContextMenu("Deselect")]
		public void Deselect()
		{
			if (SelectingFinger != null)
			{
				OnSelectUp.Invoke(SelectingFinger);

				SelectingFinger = null;
			}

			isSelected = false;

			OnDeselect.Invoke();
		}

		protected virtual void OnEnable()
		{
			// Hook events
			LeanTouch.OnFingerUp += OnFingerUp;
		}

		protected virtual void OnDisable()
		{
			// Unhook events
			LeanTouch.OnFingerUp -= OnFingerUp;

			if (isSelected == true)
			{
				Deselect();
			}
		}

		private void OnFingerUp(LeanFinger finger)
		{
			// If the finger went up, it's no longer selecting anything
			if (finger == SelectingFinger)
			{
				OnSelectUp.Invoke(SelectingFinger);

				SelectingFinger = null;
			}
		}
	}
}