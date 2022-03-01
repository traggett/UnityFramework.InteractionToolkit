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

				return action.phase == InputActionPhase.Performed;
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