using UnityEngine.XR;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		public interface IXRGrabInteractor
		{
			#region Public Interface
			/// <summary>
			/// Returns if the grab interactor can actually grab an avialable interactible.
			/// </summary>
			bool CanGrab();

			/// <summary>
			/// Returns if the grab interactor is currently grabbing an interactible.
			/// </summary>
			bool IsGrabbing();

			/// <summary>
			/// Returns if the grab interactor is currently hovering over an interactible.
			/// </summary>
			bool IsHoveringOverGrabbable();
			#endregion
		}
	}
}