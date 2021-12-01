using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// First person gesture based interactor
		/// </summary>
		public class XRFirstPersonInteractor : XRBaseControllerInteractor
		{
			#region Public Data
			public Transform _visuals;

			public float _maxSelectedConstraintDistance = 0.25f;
			public float _maxSelectedConstraintRotation = 90f;

			public float _maxHoverConstraintDistance = 0.15f;
			public float _maxHoverConstraintRotation = 90f;

			public float _snapToConstraintsTime = 0.2f;
			public float _releaseFromConstraintsTime = 0.3f;


			public LayerMask _raycastMask = -1;
			public QueryTriggerInteraction _raycastTriggerInteraction;

			/// <summary>
			/// Objects are only usable if they lie within a circle in the centre of the screen with a radius defined as a fraction of the screen width.
			/// </summary>
			[Range(0.001f, 1f)]
			public float _centreOfScreenUseAreaRadius;

			/// <summary>
			/// Objects are only usable if are less than this distance away.
			/// </summary>
			public float _maxInteractionDistance;

			public Camera _firstPersonCamera;
			#endregion

			#region Private Data
			private const int k_MaxRaycastHits = 10;

			private readonly List<XRBaseInteractable> m_ValidTargets = new List<XRBaseInteractable>();
			private int _raycastHitsCount;
			private readonly RaycastHit[] _raycastHits = new RaycastHit[k_MaxRaycastHits];

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

			#region XRBaseControllerInteractor
			protected override List<XRBaseInteractable> validTargets => m_ValidTargets;

			public override void GetValidTargets(List<XRBaseInteractable> targets)
			{
				targets.Clear();

				if (selectTarget != null)
				{
					targets.Add(selectTarget);
				}
				else
				{
					for (var i = 0; i < _raycastHitsCount; ++i)
					{
						XRBaseInteractable interactable = interactionManager.GetInteractableForCollider(_raycastHits[i].collider);

						if (interactable != null)
						{
							//First find interaction point or line in screen space

							//Check it overlaps with screen space interaciton zone (circle at centre of screen)./

							//Then Raycast to this point?? Just to check this point is visible?

							//If both are ok then is valid target


							targets.Add(interactable);
							break;
						}
					}
				}
			}

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
						{
							UpdateConstraining(Time.deltaTime);
							
							if (selectTarget == null)
							{
								UpdateRaycastHits();
							}
							else
							{
								ClearRaycastHits();
							}
						}
						break;
					case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
						{
							UpdateConstraining(Time.deltaTime);
						}
						break;
				}
			}
			#endregion



			#region Private Functions
			private static int SortRayCasts(RaycastHit a, RaycastHit b)
			{
				var aDistance = a.collider != null ? a.distance : float.MaxValue;
				var bDistance = b.collider != null ? b.distance : float.MaxValue;
				return aDistance.CompareTo(bDistance);
			}

			private void ClearRaycastHits()
			{
				if (_raycastHitsCount != 0)
				{
					Array.Clear(_raycastHits, 0, k_MaxRaycastHits);
					_raycastHitsCount = 0;
				}
			}

			private void UpdateRaycastHits()
			{
				//Work out radius at max interaction distance
				float frustumHeight = 2.0f * _maxInteractionDistance * Mathf.Tan(_firstPersonCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
				float frustumWidth = frustumHeight * _firstPersonCamera.aspect;

				float maxInteractionRadius = _centreOfScreenUseAreaRadius * frustumWidth;

				Vector3 cameraWorldDir = _firstPersonCamera.transform.forward;
				Vector3 cameraWorldPos = _firstPersonCamera.transform.position + cameraWorldDir * (_firstPersonCamera.nearClipPlane + maxInteractionRadius);

				//Find physics hits within sphere
				_raycastHitsCount = Physics.SphereCastNonAlloc(cameraWorldPos, maxInteractionRadius, cameraWorldDir, _raycastHits, _maxInteractionDistance - maxInteractionRadius, _raycastMask, _raycastTriggerInteraction);

				//Sory by distance
				Array.Sort(_raycastHits, SortRayCasts);
			}

			private static RaycastHit[] ConeCastAll(Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle)
			{
				RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - new Vector3(0, 0, maxRadius), maxRadius, direction, maxDistance);
				List<RaycastHit> coneCastHitList = new List<RaycastHit>();

				if (sphereCastHits.Length > 0)
				{
					for (int i = 0; i < sphereCastHits.Length; i++)
					{
						sphereCastHits[i].collider.gameObject.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f);
						Vector3 hitPoint = sphereCastHits[i].point;
						Vector3 directionToHit = hitPoint - origin;
						float angleToHit = Vector3.Angle(direction, directionToHit);

						if (angleToHit < coneAngle)
						{
							coneCastHitList.Add(sphereCastHits[i]);
						}
					}
				}

				RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
				coneCastHits = coneCastHitList.ToArray();

				return coneCastHits;
			}

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
					else
					{
						FreeFromConstraints(deltaTime);
					}
				}
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