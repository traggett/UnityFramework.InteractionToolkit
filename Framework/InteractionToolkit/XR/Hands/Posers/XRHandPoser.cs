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
				/// Hook this function up to the OnGrab, SelectEnter or HoverEnter events on an XRBaseInteractable
				/// </summary>
				public void SetHandPose(BaseInteractionEventArgs args)
				{
					XRHandGrabInteractor handInteractor = args.interactor as XRHandGrabInteractor;

					if (handInteractor != null)
					{
						if (args is GrabEventArgs | args is SelectEnterEventArgs)
						{
							handInteractor.ApplyHandPoserOnSelected(this, args.interactable);
						}					
						else if (args is HoverEnterEventArgs)
						{
							handInteractor.ApplyHandPoserOnHovered(this, args.interactable);
						}
					}
				}

				/// <summary>
				/// Hook this function up to the OnDrop, SelectExit or HoverExit event on an XRBaseInteractable
				/// </summary>
				public void ClearHandPose(BaseInteractionEventArgs args)
				{
					XRHandGrabInteractor handInteractor = args.interactor as XRHandGrabInteractor;

					if (handInteractor != null)
					{
						if (args is DropEventArgs || args is SelectExitEventArgs)
						{
							handInteractor.ClearHandPoserOnSelected(this);
						}
						else if (args is HoverExitEventArgs)
						{
							handInteractor.ClearHandPoserOnHovered(this);
						}
					}
				}
				#endregion

				#region Virtual Interface
				public virtual void PreparePose(XRHandGrabInteractor interactor, XRBaseInteractable interactable)
				{
					//If interactible is a grab interactor then update grab offset so the object is grabbed at correct position for the hand pose
					if (interactable is XRAdvancedGrabInteractable grabInteractable)
					{
						XRHandPose pose = GetPose(interactor, interactable);

						if (pose._hasRotation)
						{
							//Find local rotation difference from interactable to this
							Quaternion interactableAttachOffset = Quaternion.Inverse(interactable.transform.rotation) * pose._worldRotation;

							//Find the local rotation difference from interactor to its attach transform
							Quaternion interactorAttachOffset = Quaternion.Inverse(interactor.transform.rotation) * interactor.attachTransform.rotation;

							//Local attach rotation is this realative to interactor attach transform rotation
							grabInteractable.InteractorLocalAttachRotation = Quaternion.Inverse(interactorAttachOffset) * interactableAttachOffset;
						}

						if (pose._hasPosition)
						{
							//Find position offset from interactors attach position in interactors attach transforms local space
							Vector3 attachOffset = interactable.transform.position - pose._worldPosition;
							grabInteractable.InteractorLocalAttachPosition = Quaternion.Inverse(grabInteractable.InteractorLocalAttachRotation) * interactor.attachTransform.InverseTransformDirection(attachOffset);
						}
					}
				}

				public abstract XRHandPose GetPose(XRHandGrabInteractor interactor, XRBaseInteractable interactable);
				#endregion
			}
		}
	}
}