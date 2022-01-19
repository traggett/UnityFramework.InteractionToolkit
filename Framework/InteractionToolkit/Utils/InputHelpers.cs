using UnityEngine.InputSystem;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		public static class InputHelpers
		{
			#region Public Interface
			public static bool IsPressed(InputAction action)
			{
				if (action == null)
					return false;

#if INPUT_SYSTEM_1_1_OR_NEWER || INPUT_SYSTEM_1_1_PREVIEW // 1.1.0-preview.2 or newer, including pre-release
				return action.phase == InputActionPhase.Performed;
#else
				if (action.activeControl is ButtonControl buttonControl)
					return buttonControl.isPressed;

				if (action.activeControl is AxisControl)
					return action.ReadValue<float>() >= m_ButtonPressPoint;

				return action.triggered || action.phase == InputActionPhase.Performed;
#endif
			}

			public static float GetFloat(InputAction action)
			{
				if (action != null)
				{
					return action.ReadValue<float>();
				}

				return 0f;
			}
			#endregion
		}
	}
}