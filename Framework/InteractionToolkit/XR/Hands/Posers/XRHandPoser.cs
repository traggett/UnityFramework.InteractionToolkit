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
				/// Hook this function up to the OnGrab or HoverEnter events on an XRBaseInteractable
				/// </summary>
				public void SetHandPose(BaseInteractionEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						if (args is GrabEventArgs)
						{
							handInteractor.ApplyHandPoseOnGrabbed(this, args.interactableObject);
						}
						else if (args is HoverEnterEventArgs)
						{
							handInteractor.ApplyHandPoseOnHovered(this, args.interactableObject);
						}
					}
				}

				/// <summary>
				/// Hook this function up to the OnDrop or HoverExit event on an XRBaseInteractable
				/// </summary>
				public void ClearHandPose(BaseInteractionEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						if (args is DropEventArgs)
						{
							handInteractor.ClearHandPoseOnDropped(this);
						}
						else if (args is HoverExitEventArgs)
						{
							handInteractor.ClearHandPoseOnHovered(this);
						}
					}
				}
				#endregion

				#region Virtual Interface
				public abstract XRHandPose GetPose(IXRHandInteractor interactor, IXRInteractable interactable);
				#endregion
			}
		}
	}
}