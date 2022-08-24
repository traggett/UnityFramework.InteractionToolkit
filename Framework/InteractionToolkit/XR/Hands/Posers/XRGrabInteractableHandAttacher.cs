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
				protected XRHandPoser[] _posers;
				#endregion

				#region Unity Messages
				protected virtual void Awake()
				{
					_interactable = GetComponent<XRAdvancedGrabInteractable>();

					if (_interactable != null)
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
						XRHandPoser handPoser = FindBestPoser(handInteractor, HandPoseFlags.Grab);

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
						XRHandPoser handPoser = FindBestPoser(handInteractor, HandPoseFlags.Grab);

						if (handPoser != null)
						{
							handInteractor.ClearHandPoseOnDropped(_interactable);
						}
					}
				}

				private void OnHoverEnter(HoverEnterEventArgs args)
				{
					if (args.interactorObject is IXRHandInteractor handInteractor)
					{
						XRHandPoser handPoser = FindBestPoser(handInteractor, HandPoseFlags.Hover);

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
						XRHandPoser handPoser = FindBestPoser(handInteractor, HandPoseFlags.Hover);

						if (handPoser != null)
						{
							handInteractor.ClearHandPoseOnHovered(_interactable);
						}
					}
				}
				#endregion

				#region Virtual Interface
				protected virtual XRHandPoser FindBestPoser(IXRHandInteractor interactor, HandPoseFlags flags)
				{
					XRHandPoser bestPoser = null;
					float closestPoseDistSqr = float.MaxValue;

					Transform interactorTransform = interactor.Interactor.transform;
					Vector3 interactorPosition = interactorTransform.position;
					Quaternion interactorRotation = interactorTransform.rotation;

					for (int i=0; i<_posers.Length; i++)
					{
						//First check pose is compatible with interactor hand type (left/right/both etc)
						if ((_posers[i].PoseFlags & flags) == flags && 
							(_posers[i].CompatibleHands & interactor.HandFlags) == interactor.HandFlags)
						{
							//Then rate according to distance and angle?
							XRHandPose pose = _posers[i].GeneratePose(interactor, _interactable);

							//If pose has position then rate by distance?
							if (pose._hasPosition)
							{
								//Check distance is less than closest one
								float distance = Vector3.SqrMagnitude(pose._worldPosition - interactorPosition);

								if (bestPoser == null || distance < closestPoseDistSqr)
								{
									bestPoser = _posers[i];
								}
							}
							else
							{
								//Otherwise??
								if (bestPoser == null)
								{
									bestPoser = _posers[i];
								}
							}
						}
					}

					return bestPoser;
				}
				#endregion
			}
		}
	}
}