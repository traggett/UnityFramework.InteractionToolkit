using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
    namespace Interaction.Toolkit
    {
		namespace XR
		{
			/// <summary>
			/// Component that forces a hand into a pose or position/rotation whilst
			/// </summary>
			public abstract class XRHandPoser : MonoBehaviour
			{
				#region Public Interface
				/// <summary>
				/// Hook this function up to the OnGrab or OnHoverEnter or Activate events on an XRBaseInteractable
				/// </summary>
				public void SetHandPose(BaseInteractionEventArgs args)
				{
					XRHandGrabInteractor handInteractor = args.interactor as XRHandGrabInteractor;

					if (handInteractor != null)
					{
						if (args is GrabEventArgs | args is SelectEnterEventArgs)
							handInteractor.ApplyHandPoserOnSelected(this);
						else if (args is HoverEnterEventArgs)
							handInteractor.ApplyHandPoserOnHovered(this);
					}
				}

				/// <summary>
				/// Hook this function up to the OnDrop or OnHoverExit or Deactivate event on an XRBaseInteractable
				/// </summary>
				public void ClearHandPose(BaseInteractionEventArgs args)
				{
					XRHandGrabInteractor handInteractor = args.interactor as XRHandGrabInteractor;

					if (handInteractor != null)
					{
						if (args is DropEventArgs || args is SelectExitEventArgs)
							handInteractor.ClearHandPoserOnSelected(this);
						else if (args is HoverExitEventArgs)
							handInteractor.ClearHandPoserOnHovered(this);
					}
				}
				#endregion

				#region Virtual Interface
				public abstract XRHandPose GetPose(XRHandGrabInteractor interactor);
				#endregion
			}
		}
	}
}