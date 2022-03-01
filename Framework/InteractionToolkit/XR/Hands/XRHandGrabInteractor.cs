using UnityEngine;
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
			public class XRHandGrabInteractor : XRDirectInteractor, IXRGrabInteractor
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
				private XRHandPoser _currentHoveredPoser;
				#endregion

				#region XRDirectInteractor
				public override bool CanHover(XRBaseInteractable interactable)
				{
					//Don't allow hovering on returning from an override pose.
					//eg if a pose was too far way from the interacble and the interaction was cancelled, dont allow hovering objects whilst the pose is transitoned back to the interactor.
					return !_handVisuals.IsReturningFromOverridePose() && base.CanHover(interactable);
				}

				public override bool CanSelect(XRBaseInteractable interactable)
				{
					//Don't allow selecting on returning from an override pose.
					//eg if a pose was too far way from the interacble and the interaction was cancelled, dont allow hovering objects whilst the pose is transitoned back to the interactor.
					return !_handVisuals.IsReturningFromOverridePose() && base.CanSelect(interactable) && (xrController.selectInteractionState.activatedThisFrame || selectTarget == interactable);
				}

				public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
				{
					base.ProcessInteractor(updatePhase);

					if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic || updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
					{
						UpdateHandPoses();
					}
				}
				#endregion

				#region IXRGrabInteractor
				public bool CanGrab()
				{
					//Only count as grabbing when not moving from or to and override pose (otherwise will pick up object whilst lerp back fom another interaction)
					return !_handVisuals.IsEnteringOverridePose() && !_handVisuals.IsEnteringOverridePose();
				}

				public bool IsGrabbing()
				{
					return selectTarget != null && CanGrab();
				}

				public bool IsHoveringOverGrabbable()
				{
					return hoverTargets.Count > 0 && CanGrab();
				}
				#endregion

				#region Public Interface
				/// <summary>
				/// Should be called by a XRHandPoser via a SelectEnterEvent
				/// </summary>
				public void ApplyHandPoserOnSelected(XRHandPoser poser)
				{
					_currentSelectedPoser = poser;
				}

				/// <summary>
				/// Should be called by a XRHandPoser via a SelectExitEventArgs
				/// </summary>
				public void ClearHandPoserOnSelected(XRHandPoser poser)
				{
					if (_currentSelectedPoser == poser)
					{
						_handVisuals.ClearOverridePose(poser.GetPose(this));
						_currentSelectedPoser = null;
					}
				}

				/// <summary>
				/// Should be called by a XRHandPoser via a HoverEnterEventArgs
				/// </summary>
				public void ApplyHandPoserOnHovered(XRHandPoser poser)
				{
					_currentHoveredPoser = poser;
				}

				/// <summary>
				/// Should be called by a XRHandPoser via a HoverExitEventArgs
				/// </summary>
				public void ClearHandPoserOnHovered(XRHandPoser poser)
				{
					if (_currentHoveredPoser == poser)
					{
						_handVisuals.ClearOverridePose(poser.GetPose(this));
						_currentHoveredPoser = null;
					}
				}
				#endregion

				#region Private Functions
				/// <summary>
				/// Applies any hand poses from current selected or hovered interactables.
				/// </summary>
				private void UpdateHandPoses()
				{
					if (_handVisuals != null)
					{
						//If selected interactable with a hand poser...
						if(_currentSelectedPoser != null)
						{
							XRHandPose handPose = _currentSelectedPoser.GetPose(this);
							
							if (IsPoseOk(handPose, _maxSelectedOverridePoseDistance, _maxSelectedOverridePoseRotation))
							{
								_handVisuals.ApplyOverridePose(handPose);
							}
							else
							{
								_handVisuals.ClearOverridePose(handPose);

								_currentSelectedPoser = null;

								if (selectTarget != null)
								{
									interactionManager.SelectCancel(this, selectTarget);
								}
							}
						}
						//If hovered over interactable with a hand poser...
						else if (_currentHoveredPoser != null)
						{
							XRHandPose handPose = _currentHoveredPoser.GetPose(this);

							if (IsPoseOk(handPose, _maxHoveredOverridePoseDistance, _maxHoveredOverridePoseRotation))
							{
								_handVisuals.ApplyOverridePose(handPose);
							}
							else
							{
								_handVisuals.ClearOverridePose(handPose);

								_currentHoveredPoser = null;

								for (int i = 0; i < hoverTargets.Count; i++)
								{
									interactionManager.HoverCancel(this, hoverTargets[i]);
								}
							}
						}
					}
				}

				/// <summary>
				/// Checks whether a target hand pose goes beyond the allowed distance/rotation limits from the interactors actual current position/rotation.
				/// </summary>
				private bool IsPoseOk(XRHandPose handPose, float maxDist, float maxAngle)
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
				#endregion
			}
		}
	}
}