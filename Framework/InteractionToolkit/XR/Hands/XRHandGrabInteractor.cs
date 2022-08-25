using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			/// <summary>
			/// Interactor used for directly interacting with interactables that are colliding with it.
			/// This is used by the hands to pick things up or interact with things directly (eg press a button)
			/// It can be forced into poses whilst hovering or selecting interactables (eg correct grab pose whilst grabbing a lever).
			/// </summary>
			public class XRHandGrabInteractor : XRDirectInteractor, IXRGrabInteractor, IXRHandInteractor
			{
				#region Public Data
				/// <summary>
				/// The hand visuals associated with this interactor.
				/// This interactor will set override hand poses on it whilst directly interacting with interactables.
				/// </summary>
				public XRHandVisuals HandVisuals
				{
					get { return _handVisuals; }
				}

				/// <summary>
				/// The max distance the hand can visually be from the interactors actual position whilst selecting an interactable.
				/// When an interactable wants the hand to be in a pose beyond this disance, the selection will be cancelled.
				/// </summary>
				public float _maxSelectedOverridePoseDistance = 0.25f;

				/// <summary>
				/// The max rotation difference the hand can visually be from the interactors actual rotation whilst selecting an interactable.
				/// When an interactable wants the hand to be in a pose beyond this rotation, the selection will be cancelled.
				/// </summary>
				public float _maxSelectedOverridePoseRotation = 90f;

				/// <summary>
				/// The max distance the hand can visually be from the interactors actual position whilst hovering over an interactable.
				/// When an interactable wants the hand to be in a pose beyond this disance, the hover will be cancelled.
				/// </summary>
				public float _maxHoveredOverridePoseDistance = 0.15f;

				/// <summary>
				/// The max rotation difference the hand can visually be from the interactors actual rotation whilst hovering over an interactable.
				/// When an interactable wants the hand to be in a pose beyond this rotation, the hover will be cancelled.
				/// </summary>
				public float _maxHoveredOverridePoseRotation = 90f;
				#endregion

				#region Private Data
				[SerializeField]
				private XRHandVisuals _handVisuals;
				private XRHandPose _currentSelectedPose;
				private IXRInteractable _currentSelectedPoseInteractable;
				private XRHandPose _currentHoveredPose;
				private IXRInteractable _currentHoveredPoseInteractable;
				#endregion

				#region XRDirectInteractor
				public override bool CanHover(IXRHoverInteractable interactable)
				{
					//Don't allow hovering on returning from an override pose.
					//eg if a pose was too far way from the interacble and the interaction was cancelled, dont allow hovering objects whilst the pose is transitoned back to the interactor.
					if (_handVisuals.IsReturningFromOverridePose())
						return false;

					return base.CanHover(interactable);
				}

				public override bool CanSelect(IXRSelectInteractable interactable)
				{
					//Don't allow selecting on returning from an override pose.
					//eg if a pose was too far way from the interacble and the interaction was cancelled, dont allow hovering objects whilst the pose is transitoned back to the interactor.
					if (_handVisuals.IsReturningFromOverridePose())
						return false;

					//Only allow selecting on pressing down this frame or if already selecting the interactible
					if (!xrController.selectInteractionState.activatedThisFrame && !interactablesSelected.Contains(interactable))
						return false;

					return base.CanSelect(interactable);
				}

				public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
				{
					base.ProcessInteractor(updatePhase);

					if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Late)
					{
						ApplyHandPoses();
					}
				}
				#endregion

				#region IXRGrabInteractor
				public bool IsGrabbedObjectAttached()
				{
					//Don't count as grabbing until fully blended to grab pose
					return !_handVisuals.IsEnteringOverridePose();
				}
				#endregion

				#region IXRHandInteractor
				public HandFlags HandFlags
				{
					get
					{
						return _handVisuals.HandFlags;
					}
				}

				public XRBaseInteractor Interactor
				{
					get
					{
						return this;
					}
				}

				public void ApplyHandPoseOnGrabbed(XRHandPose pose, IXRInteractable interactable)
				{
					_currentSelectedPose = pose;
					_currentSelectedPoseInteractable = interactable;

					//If interactable is a GrabInteractable then calculated the attach offsets to place the hand at the correct pose on grab
					if (interactable is XRAdvancedGrabInteractable grabInteractable)
					{
						if (pose.HasRotation)
						{
							//Find rotation offset of pose in interactable's own space
							grabInteractable.InteractorLocalAttachRotation = Quaternion.Inverse(Quaternion.Inverse(grabInteractable.transform.rotation) * pose.WorldRotation);
						}

						if (pose.HasPosition)
						{
							//Find position offset of pose in interactable's own space
							grabInteractable.InteractorLocalAttachPosition = -Vector3.Scale(grabInteractable.transform.InverseTransformPoint(pose.WorldPosition), grabInteractable.transform.lossyScale);
						}
					}
				}

				public void ClearHandPoseOnDropped(IXRInteractable interactable)
				{
					if (_currentSelectedPoseInteractable == interactable)
					{
						_handVisuals.ClearOverridePose(_currentSelectedPose);
						_currentSelectedPose = null;
						_currentSelectedPoseInteractable = null;
					}
				}

				public void ApplyHandPoseOnHovered(XRHandPose pose, IXRInteractable interactable)
				{
					_currentHoveredPose = pose;
					_currentHoveredPoseInteractable = interactable;
				}

				public void ClearHandPoseOnHovered(IXRInteractable interactable)
				{
					if (_currentHoveredPoseInteractable == interactable)
					{
						_handVisuals.ClearOverridePose(_currentHoveredPose);
						_currentHoveredPose = null;
						_currentHoveredPoseInteractable = null;
					}
				}
				#endregion

				#region Private Functions
				/// <summary>
				/// Applies any hand poses from current selected or hovered interactables.
				/// </summary>
				private void ApplyHandPoses()
				{
					if (_handVisuals != null)
					{
						//If selected interactable with a hand poser...
						if (_currentSelectedPose != null)
						{
							bool easingIn = IsEasingInToAttachment(_currentSelectedPoseInteractable);

							if (easingIn || IsPoseOk(_currentSelectedPose, _maxSelectedOverridePoseDistance, _maxSelectedOverridePoseRotation))
							{
								_handVisuals.ApplyOverridePose(_currentSelectedPose);
								_handVisuals.SetIgnorePosePosition(easingIn);
							}
							else
							{
								ClearSelectedPose();
							}
						}
						//If hovered over interactable with a hand poser...
						else if (_currentHoveredPose != null)
						{
							bool easingIn = IsEasingInToAttachment(_currentSelectedPoseInteractable);

							if (easingIn || IsPoseOk(_currentHoveredPose, _maxHoveredOverridePoseDistance, _maxHoveredOverridePoseRotation))
							{
								_handVisuals.ApplyOverridePose(_currentHoveredPose);
								_handVisuals.SetIgnorePosePosition(easingIn);
							}
							else
							{
								ClearHoveredPose();
							}
						}
					}
				}

				/// <summary>
				/// Checks whether a target hand pose goes beyond the allowed distance limits from the interactors actual current position.
				/// </summary>
				private bool IsPoseOk(XRHandPose handPose, float maxDist, float maxAngle)
				{
					//Check distance
					if (handPose.HasPosition)
					{
						float distance = Vector3.Distance(handPose.WorldPosition, this.attachTransform.position);

						if (distance > maxDist)
						{
							return false;
						}
					}

					//Check rotation
					if (handPose.HasRotation)
					{
						float angle = Quaternion.Angle(handPose.WorldRotation, this.attachTransform.rotation);

						if (angle > maxAngle)
						{
							return false;
						}
					}

					return true;
				}

				private void ClearSelectedPose()
				{
					_handVisuals.ClearOverridePose(_currentSelectedPose);

					_currentSelectedPose = null;

					for (int i = 0; i < interactablesSelected.Count; i++)
					{
						interactionManager.SelectCancel(this, interactablesSelected[i]);
					}
				}

				private void ClearHoveredPose()
				{
					_handVisuals.ClearOverridePose(_currentHoveredPose);

					_currentHoveredPose = null;

					for (int i = 0; i < interactablesHovered.Count; i++)
					{
						interactionManager.HoverCancel(this, interactablesHovered[i]);
					}
				}

				private bool IsEasingInToAttachment(IXRInteractable interactable)
				{
					//If interactible is easing in (lerping to position) then ignore position and rotation until its there
					if (interactable is XRAdvancedGrabInteractable grabInteractable && grabInteractable.IsEasingIn())
					{
						return true;
					}

					return false;
				}
				#endregion
			}
		}
	}
}