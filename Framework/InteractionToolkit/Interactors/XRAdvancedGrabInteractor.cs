using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// Extention of direct interactor that can be told to attach to interactable.
		/// Has a max attacment ranch - beyond which it will automatically detatch.
		/// </summary>
		public class XRAdvancedGrabInteractor : XRDirectInteractor
		{
			#region Public Data
			public Transform _visuals;

			public float _maxSelectedConstraintDistance = 0.25f;
			public float _maxSelectedConstraintRotation = 90f;

			public float _maxHoverConstraintDistance = 0.15f;
			public float _maxHoverConstraintRotation = 90f;

			public float _snapToConstraintsTime = 0.2f;
			public float _releaseFromConstraintsTime = 0.3f;
			#endregion

			#region Private Data
			private float _constrainAmount;
			private bool _returningFromConstraints;
			#endregion

			#region Public Interface
			public void SetAttachedPosition(Vector3 worldPosition)
			{
				_visuals.transform.position = worldPosition;
			}

			public void SetAttachedRotation(Quaternion worldRotation)
			{
				_visuals.transform.rotation = worldRotation;
			}
			#endregion

			#region XRDirectInteractor
			public override bool CanHover(XRBaseInteractable interactable)
			{
				return !_returningFromConstraints && base.CanHover(interactable);
			}

			public override bool CanSelect(XRBaseInteractable interactable)
			{
				//Only allow selecting when not currently press select
				return !_returningFromConstraints && base.CanSelect(interactable) && (xrController.selectInteractionState.activatedThisFrame || selectTarget == interactable);
			}

			protected override void OnSelectEntered(SelectEnterEventArgs args)
			{
				base.OnSelectEntered(args);

				_constrainAmount = 0f;
			}

			public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
			{
				base.ProcessInteractor(updatePhase);

				switch (updatePhase)
				{
					case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
					case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
						{
							UpdateConstraining(Time.deltaTime);
						}
						break;
				}
			}
			#endregion

			#region Private Functions
			private void UpdateConstraining(float deltaTime)
			{
				if (_visuals != null)
				{
					if (selectTarget is XRAdvancedGrabInteractable selectedGrabInteractable && selectedGrabInteractable.SelectedInteractorConstraint != null)
					{
						ConstrainToInteractable(selectedGrabInteractable.SelectedInteractorConstraint, deltaTime);

						//If constraints are no longer ok cancel constraining
						if (_constrainAmount > 0f && !AreConstraintsOk(_maxSelectedConstraintDistance, _maxSelectedConstraintRotation))
						{
							CancelConstraining();
						}
					}
					else if (GetNearestHoverTarget() is XRAdvancedGrabInteractable hoveredGrabInteractable && hoveredGrabInteractable.HoverInteractorConstraint != null)
					{
						ConstrainToInteractable(hoveredGrabInteractable.HoverInteractorConstraint, deltaTime);

						//If constraints are no longer ok cancel constraining
						if (_constrainAmount > 0f && !AreConstraintsOk(_maxHoverConstraintDistance, _maxHoverConstraintRotation))
						{
							CancelConstraining();
						}
					}
					else
					{
						FreeFromConstraints(deltaTime);
					}
				}
			}

			private XRBaseInteractable GetNearestHoverTarget()
			{
				if (hoverTargets.Count > 0)
				{
					return hoverTargets[0];
				}

				return null;
			}

			private void ConstrainToInteractable(XRInteractorConstraint interactorConstraint, float deltaTime)
			{
				if (_snapToConstraintsTime > 0f && _constrainAmount < 1f)
				{
					_constrainAmount += deltaTime / _snapToConstraintsTime;
				}
				else
				{
					_constrainAmount = 1f;
				}

				Vector3 constrainedPosition = this.transform.position;
				Quaternion constrainedRotation = this.transform.rotation;

				interactorConstraint.ConstrainInteractor(this, out bool constrainPosition, ref constrainedPosition, out bool constrainRotation, ref constrainedRotation);

				//Adjust constriants so attach transform is placed at position rather than root
				if (constrainRotation)
				{
					constrainedRotation *= attachTransform.localRotation;
				}

				if (constrainPosition)
				{
					constrainedPosition -= (attachTransform.rotation * attachTransform.localPosition);
				}

				if (_constrainAmount >= 1f)
				{
					_visuals.transform.position = constrainedPosition;
					_visuals.transform.rotation = constrainedRotation;
				}
				else
				{
					_visuals.transform.position = Vector3.Lerp(_visuals.transform.position, constrainedPosition, _constrainAmount);
					_visuals.transform.rotation = Quaternion.Slerp(_visuals.transform.rotation, constrainedRotation, _constrainAmount);
				}				
			}

			private void FreeFromConstraints(float deltaTime)
			{
				if (_constrainAmount > 0f)
				{
					if (_releaseFromConstraintsTime > 0f)
					{
						_constrainAmount -= deltaTime / _releaseFromConstraintsTime;

						if (_constrainAmount <= 0f)
						{
							_constrainAmount = 0f;
							_returningFromConstraints = false;
						}
					}
					else
					{
						_constrainAmount = 0f;
						_returningFromConstraints = false;
					}

					_visuals.transform.localPosition = Vector3.Lerp(Vector3.zero, _visuals.transform.localPosition, _constrainAmount);
					_visuals.transform.localRotation = Quaternion.Slerp(Quaternion.identity, _visuals.transform.localRotation, _constrainAmount);
				}
			}

			private bool AreConstraintsOk(float maxDist, float maxAngle)
			{
				//Check distance
				float distance = Vector3.Distance(_visuals.transform.position, this.transform.position);

				if (distance > maxDist)
				{
					return false;
				}

				//Check rotation
				float angle = Quaternion.Angle(_visuals.rotation, this.transform.rotation);

				if (angle > maxAngle)
				{
					return false;
				}

				return true;
			}

			private void CancelConstraining()
			{
				_returningFromConstraints = true;

				if (selectTarget != null)
				{
					interactionManager.SelectCancel(this, selectTarget);
				}

				for (int i = 0; i < hoverTargets.Count; i++)
				{
					interactionManager.HoverCancel(this, hoverTargets[i]);
				}
			}
			#endregion
		}
	}
}