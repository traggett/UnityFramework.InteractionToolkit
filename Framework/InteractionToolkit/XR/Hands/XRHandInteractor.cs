using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public class XRHandInteractor : XRDirectInteractor, IXRGrabInteractor
			{
				#region Public Data
				public XRHandVisuals _visuals;

				public float _maxSelectedConstraintDistance = 0.25f;
				public float _maxSelectedConstraintRotation = 90f;

				public float _maxHoverConstraintDistance = 0.15f;
				public float _maxHoverConstraintRotation = 90f;
				#endregion

				#region Private Data
				private XRHandPoser _selectedPoser;
				private XRHandPoser _hoveredPoser;
				#endregion

				#region XRDirectInteractor
				public override bool CanHover(XRBaseInteractable interactable)
				{
					//Only allow selecting when not returning from constraints
					return base.CanHover(interactable);
				}

				public override bool CanSelect(XRBaseInteractable interactable)
				{
					//Only allow selecting when not currently press select and not returning from constraints
					return base.CanSelect(interactable) && (xrController.selectInteractionState.activatedThisFrame || selectTarget == interactable);
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
				public bool IsGrabbing()
				{
					//Only count as grabbing an object when constrained?
					return true;
				}
				#endregion

				#region Public Interface
				public void ApplyHandPoserOnSelected(XRHandPoser poser)
				{
					_selectedPoser = poser;
				}

				public void ClearHandPoserOnSelected(XRHandPoser poser)
				{
					if (_selectedPoser == poser)
					{
						_visuals.ClearOverrideHandPose(poser.GetPose(this));
						_selectedPoser = null;
					}
				}

				public void ApplyHandPoserOnHovered(XRHandPoser poser)
				{
					_hoveredPoser = poser;
				}

				public void ClearHandPoserOnHovered(XRHandPoser poser)
				{
					if (_hoveredPoser == poser)
					{
						_visuals.ClearOverrideHandPose(poser.GetPose(this));
						_hoveredPoser = null;
					}
				}
				#endregion

				#region Private Functions
				private void UpdateHandPoses()
				{
					if (_visuals != null)
					{
						//If selected interactable with a hand poser...
						if(_selectedPoser != null)
						{
							XRHandPose handPose = _selectedPoser.GetPose(this);
							
							if (IsPoseOk(handPose, _maxSelectedConstraintDistance, _maxSelectedConstraintRotation))
							{
								_visuals.ApplyOverrideHandPose(handPose);
							}
							else
							{
								_visuals.ClearOverrideHandPose(handPose);

								_selectedPoser = null;

								if (selectTarget != null)
								{
									interactionManager.SelectCancel(this, selectTarget);
								}
							}
						}
						//If hovered over interactable with a hand poser...
						else if (_hoveredPoser != null)
						{
							XRHandPose handPose = _hoveredPoser.GetPose(this);

							if (IsPoseOk(handPose, _maxHoverConstraintDistance, _maxHoverConstraintRotation))
							{
								_visuals.ApplyOverrideHandPose(handPose);
							}
							else
							{
								_visuals.ClearOverrideHandPose(handPose);

								_hoveredPoser = null;

								for (int i = 0; i < hoverTargets.Count; i++)
								{
									interactionManager.HoverCancel(this, hoverTargets[i]);
								}
							}
						}
					}
				}

				private bool IsPoseOk(XRHandPose handPose, float maxDist, float maxAngle)
				{
					//Check distance
					if (handPose._hasPosition)
					{
						float distance = Vector3.Distance(handPose._worldPosition, this.transform.position);

						if (distance > maxDist)
						{
							return false;
						}
					}

					//Check rotation
					if (handPose._hasRotation)
					{
						float angle = Quaternion.Angle(handPose._worldRotation, this.transform.rotation);

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