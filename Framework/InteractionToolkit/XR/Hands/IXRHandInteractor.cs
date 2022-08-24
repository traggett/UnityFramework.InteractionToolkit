using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			[Flags]
			public enum HandFlags
			{
				None = 0,
				Left = 1,
				Right = 2,
				Both = Left | Right
			}

			public interface IXRHandInteractor
			{
				#region Public Interface
				/// <summary>
				/// The XRNode associated with the controller driving this hand interaction.
				/// </summary>
				public HandFlags HandFlags
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
				/// </summary>
				void ApplyHandPoseOnGrabbed(XRHandPose pose, IXRInteractable interactable);

				/// <summary>
				/// Clears a hand pose for an interactable when it's dropped.
				/// </summary>
				public void ClearHandPoseOnDropped(IXRInteractable interactable);

				/// <summary>
				/// Applys a hand pose from an XRHandPoser when an interactable is hovered.
				/// </summary>
				public void ApplyHandPoseOnHovered(XRHandPose pose, IXRInteractable interactable);

				/// <summary>
				/// Clears a hand pose for an interactable when it's un-hovered.
				/// </summary>
				public void ClearHandPoseOnHovered(IXRInteractable interactable);
				#endregion
			}
		}
	}
}