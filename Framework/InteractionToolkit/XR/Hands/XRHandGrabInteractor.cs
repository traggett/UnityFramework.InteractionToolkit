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
				private XRHandPoser _currentSelectedPoser;
				private IXRInteractable _currentSelectedPoserInteractable;
				private XRHandPoser _currentHoveredPoser;
				private IXRInteractable _currentHoveredPoserInteractable;
				#endregion

				#region XRDirectInteractor
				public override bool CanHover(IXRHoverInteractable interactable)
				{
					//Don't allow hovering on returning from an override pose.
					//eg if a pose was too far way from the interacble and the interaction was cancelled, dont allow hovering objects whilst the pose is transitoned back to the interactor.
					return !_handVisuals.IsReturningFromOverridePose() && base.CanHover(interactable);
				}

				public override bool CanSelect(IXRSelectInteractable interactable)
				{
					//Don't allow selecting on returning from an override pose.
					//eg if a pose was too far way from the interacble and the interaction was cancelled, dont allow hovering objects whilst the pose is transitoned back to the interactor.
					return !_handVisuals.IsReturningFromOverridePose() && base.CanSelect(interactable) && (xrController.selectInteractionState.activatedThisFrame || selectTarget == interactable);
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

				public void ApplyHandPoseOnGrabbed(XRHandPoser poser, IXRInteractable interactable)
				{
					_currentSelectedPoser = poser;
					_currentSelectedPoserInteractable = interactable;

					//If interactable is a GrabInteractable then calculated the attach offsets to place the hand at the correct pose on grab
					if (interactable is XRAdvancedGrabInteractable grabInteractable)
					{
						XRHandPose pose = poser.GeneratePose(this, interactable);

						if (pose._hasRotation)
						{
							//Find rotation offset of pose in interactable's own space
							grabInteractable.InteractorLocalAttachRotation = Quaternion.Inverse(Quaternion.Inverse(grabInteractable.transform.rotation) * pose._worldRotation);
						}

						if (pose._hasPosition)
						{
							//Find position offset of pose in interactable's own space
							grabInteractable.InteractorLocalAttachPosition = -grabInteractable.transform.InverseTransformPoint(pose._worldPosition);
						}
					}
				}

				public void ClearHandPoseOnDropped(IXRInteractable interactable)
				{
					if (_currentSelectedPoserInteractable == interactable)
					{
						_handVisuals.ClearOverridePose();
						_currentSelectedPoser = null;
						_currentSelectedPoserInteractable = null;
					}
				}

				public void ApplyHandPoseOnHovered(XRHandPoser poser, IXRInteractable interactable)
				{
					_currentHoveredPoser = poser;
					_currentHoveredPoserInteractable = interactable;
				}

				public void ClearHandPoseOnHovered(IXRInteractable interactable)
				{
					if (_currentHoveredPoserInteractable == interactable)
					{
						_handVisuals.ClearOverridePose();
						_currentHoveredPoser = null;
						_currentHoveredPoserInteractable = null;
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
						if (_currentSelectedPoser != null)
						{
							XRHandPose handPose = GetPose(_currentSelectedPoser, _currentSelectedPoserInteractable);

							if (IsPosePositionOk(handPose, _maxSelectedOverridePoseDistance)
								&& IsPoseRotationOk(handPose, _maxSelectedOverridePoseRotation))
							{
								_handVisuals.ApplyOverridePose(handPose);
							}
							else
							{
								ClearSelectedPose();
							}
						}
						//If hovered over interactable with a hand poser...
						else if (_currentHoveredPoser != null)
						{
							XRHandPose handPose = GetPose(_currentHoveredPoser, _currentHoveredPoserInteractable);

							if (IsPosePositionOk(handPose, _maxHoveredOverridePoseDistance) 
								&& IsPoseRotationOk(handPose, _maxHoveredOverridePoseRotation))
							{
								_handVisuals.ApplyOverridePose(handPose);
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
				private bool IsPosePositionOk(XRHandPose handPose, float maxDist)
				{
					//Check distance
					if (handPose._hasPosition)
					{
						float distance = Vector3.Distance(handPose._worldPosition, this.attachTransform.position);

						if (distance > maxDist)
						{
							return false;
						}
					}

					return true;
				}

				/// <summary>
				/// Checks whether a target hand pose goes beyond the allowed rotation limits from the interactors actual current rotation.
				/// </summary>
				private bool IsPoseRotationOk(XRHandPose handPose, float maxAngle)
				{
					//Check rotation
					if (handPose._hasRotation)
					{
						float angle = Quaternion.Angle(handPose._worldRotation, this.attachTransform.rotation);

						if (angle > maxAngle)
						{
							return false;
						}
					}

					return true;
				}

				private void ClearSelectedPose()
				{
					_handVisuals.ClearOverridePose();

					_currentSelectedPoser = null;

					for (int i = 0; i < interactablesSelected.Count; i++)
					{
						interactionManager.SelectCancel(this, interactablesSelected[i]);
					}
				}

				private void ClearHoveredPose()
				{
					_handVisuals.ClearOverridePose();

					_currentHoveredPoser = null;

					for (int i = 0; i < interactablesHovered.Count; i++)
					{
						interactionManager.HoverCancel(this, interactablesHovered[i]);
					}
				}

				private XRHandPose GetPose(XRHandPoser handPoser, IXRInteractable interactable)
				{
					XRHandPose handPose = handPoser.GeneratePose(this, interactable);

					//If interactible is easing in (lerping to position) then ignore position and rotation until its there
					if (interactable is XRAdvancedGrabInteractable grabInteractable && grabInteractable.IsEasingIn())
					{
						handPose._hasPosition = false;
						handPose._hasRotation = false;
					}

					return handPose;
				}
				#endregion
			}
		}
	}
}