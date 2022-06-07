using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public interface IXRHandInteractor
			{
				#region Public Interface
				/// <summary>
				/// The XRNode associated with the controller driving this hand interaction.
				/// </summary>
				public XRNode HandNode
				{
					get;
				}

				/// <summary>
				/// The XRBaseInteractor this interface is attached to.
				/// </summary>
				public XRBaseInteractor Interactor
				{
					get;
				}

				/// <summary>
				/// Applys a hand pose from an XRHandPoser when an interactable is grabbed.
				/// Should be called by a XRHandPoser via a OnGrab event.
				/// </summary>
				void ApplyHandPoseOnGrabbed(XRHandPoser poser, XRBaseInteractable interactable);

				/// <summary>
				/// Clears a hand pose from an XRHandPoser when an interactable is dropped.
				/// Should be called by a XRHandPoser via a OnDrop event.
				/// </summary>
				public void ClearHandPoseOnDropped(XRHandPoser poser);

				/// <summary>
				/// Applys a hand pose from an XRHandPoser when an interactable is hovered.
				/// Should be called by a XRHandPoser via a HoverEnterEventArgs.
				/// </summary>
				public void ApplyHandPoseOnHovered(XRHandPoser poser, XRBaseInteractable interactable);

				/// <summary>
				/// Clears a hand pose from an XRHandPoser when an interactable is un-hovered.
				/// Should be called by a XRHandPoser via a HoverExitEventArgs.
				/// </summary>
				public void ClearHandPoseOnHovered(XRHandPoser poser);
				#endregion
			}
		}
	}
}