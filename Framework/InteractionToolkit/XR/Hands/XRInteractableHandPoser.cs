using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
    namespace Interaction.Toolkit
    {
		namespace XR
		{
			/// <summary>
			/// Component that allows an IXRHandInteractor to grab an XRBaseInteractable using one or more different hand poses.
			/// These poses are chosen based on conditions like hand type (left/right) and the distance/rotation of the interactor.
			/// </summary>
			[RequireComponent(typeof(XRBaseInteractable))]
			public class XRInteractableHandPoser : MonoBehaviour
			{
				#region Protected Data
				protected XRBaseInteractable _interactable;
				[SerializeField]
				protected XRHandPose[] _poses;
				#endregion

				#region Unity Messages
				protected virtual void Awake()
				{
					if (TryGetComponent(out _interactable))
					{
						if (_interactable is XRAdvancedGrabInteractable grabInteractable)
						{
							grabInteractable.onGrab.AddListener(OnGrab);
							grabInteractable.onDrop.AddListener(OnDrop);

						}
						else
						{
							_interactable.selectEntered.AddListener(OnSelected);
							_interactable.selectExited.AddListener(OnDeselected);
						}

						_interactable.hoverEntered.AddListener(OnHoverEnter);
						_interactable.hoverExited.AddListener(OnHoverExit);
					}
				}

				protected virtual void OnDestroy()
				{
					if (_interactable != null)
					{
						if (_interactable is XRAdvancedGrabInteractable grabInteractable)
						{
							grabInteractable.onGrab.RemoveListener(OnGrab);
							grabInteractable.onDrop.RemoveListener(OnDrop);

						}
						else
						{
							_interactable.selectEntered.RemoveListener(OnSelected);
							_interactable.selectExited.RemoveListener(OnDeselected);
						}

						_interactable.hoverEntered.RemoveListener(OnHoverEnter);
						_interactable.hoverExited.RemoveListener(OnHoverExit);
					}
				}
				#endregion

				#region Private Functions
				private void OnGrab(GrabEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						XRHandPose handPoser = FindBestHandPose(handInteractor, HandInteractionFlags.Grab);

						if (handPoser != null)
						{
							handInteractor.ApplyHandPoseOnGrabbed(handPoser, _interactable);
						}
					}
				}


				private void OnDrop(DropEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						handInteractor.ClearHandPoseOnDropped(_interactable);
					}
				}

				private void OnSelected(SelectEnterEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						XRHandPose handPoser = FindBestHandPose(handInteractor, HandInteractionFlags.Grab);

						if (handPoser != null)
						{
							handInteractor.ApplyHandPoseOnGrabbed(handPoser, _interactable);
						}
					}
				}


				private void OnDeselected(SelectExitEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						handInteractor.ClearHandPoseOnDropped(_interactable);
					}
				}

				private void OnHoverEnter(HoverEnterEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						XRHandPose handPoser = FindBestHandPose(handInteractor, HandInteractionFlags.Hover);

						if (handPoser != null)
						{
							handInteractor.ApplyHandPoseOnHovered(handPoser, _interactable);
						}
					}
				}

				private void OnHoverExit(HoverExitEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						handInteractor.ClearHandPoseOnHovered(_interactable);
					}
				}
				#endregion

				#region Virtual Interface
				protected virtual XRHandPose FindBestHandPose(IXRHandInteractor interactor, HandInteractionFlags interactionFlag)
				{
					//TO DO! this should be done with rating system - all valid poses rated by closest distance and rotation and best returned
					XRHandPose bestPoser = null;
					float closestPoseDistSqr = float.MaxValue;

					Transform interactorTransform = interactor.Interactor.transform;
					Vector3 interactorPosition = interactorTransform.position;
					Quaternion interactorRotation = interactorTransform.rotation;
					HandFlags interactorHandFlags = interactor.HandFlags;

					for (int i=0; i<_poses.Length; i++)
					{
						XRHandPose handPose = _poses[i];
						HandInteractionFlags poseInteractionFlags = handPose.InteractionFlags;
						HandFlags poseHandFlags = handPose.CompatibleHands;

						//First check pose is compatible with interactor hand type (left/right/both etc)
						if (poseInteractionFlags.HasFlag(interactionFlag) && poseHandFlags.HasFlag(interactorHandFlags))
						{
							//Then rate according to distance and angle?
							handPose.PreparePose(interactor);

							if (handPose.HasPosition)
							{
								//Check distance is less than closest one
								float distance = Vector3.SqrMagnitude(handPose.transform.position - interactorPosition);

								if (bestPoser == null || distance < closestPoseDistSqr)
								{
									bestPoser = _poses[i];
								}
							}
							else
							{
								if (bestPoser == null)
								{
									bestPoser = _poses[i];
								}
							}

							//TO DO! also check rotation (favour poses closer to interactor rotation too)
						}
					}

					return bestPoser;
				}
				#endregion
			}
		}
	}
}