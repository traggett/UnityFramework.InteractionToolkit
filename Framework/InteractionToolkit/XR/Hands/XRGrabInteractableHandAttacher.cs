using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
    namespace Interaction.Toolkit
    {
		namespace XR
		{
			/// <summary>
			/// Component that allows an IXRHandInteractor to grab an XRAdvancedGrabInteractable using one or more different hand poses.
			/// These poses are chosen based on conditions like hand type (left/right) and the distance/rotation of the interactor.
			/// </summary>
			[RequireComponent(typeof(XRAdvancedGrabInteractable))]
			public class XRGrabInteractableHandAttacher : MonoBehaviour
			{
				#region Protected Data
				protected XRAdvancedGrabInteractable _interactable;
				[SerializeField]
				protected XRHandPose[] _poses;
				#endregion

				#region Unity Messages
				protected virtual void Awake()
				{
					if (TryGetComponent(out _interactable))
					{
						_interactable.onGrab.AddListener(OnGrab);
						_interactable.onDrop.AddListener(OnDrop);

						_interactable.hoverEntered.AddListener(OnHoverEnter);
						_interactable.hoverExited.AddListener(OnHoverExit);
					}
				}

				protected virtual void OnDestroy()
				{
					if (_interactable != null)
					{
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
						XRHandPose handPoser = FindBestPoser(handInteractor, HandPoseFlags.Grab);

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

				private void OnHoverEnter(HoverEnterEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						XRHandPose handPoser = FindBestPoser(handInteractor, HandPoseFlags.Hover);

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
				protected virtual XRHandPose FindBestPoser(IXRHandInteractor interactor, HandPoseFlags flags)
				{
					//TO DO! this should be done with rating system - all valid poses rated by closest distance and rotation and best returned
					XRHandPose bestPoser = null;
					float closestPoseDistSqr = float.MaxValue;

					Transform interactorTransform = interactor.Interactor.transform;
					Vector3 interactorPosition = interactorTransform.position;
					Quaternion interactorRotation = interactorTransform.rotation;

					for (int i=0; i<_poses.Length; i++)
					{
						XRHandPose handPose = _poses[i];

						//First check pose is compatible with interactor hand type (left/right/both etc)
						if ((flags & handPose.PoseFlags) == flags && 
							(interactor.HandFlags & handPose.CompatibleHands) == interactor.HandFlags)
						{
							//Then rate according to distance and angle?
							handPose.GeneratePose(interactor);

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