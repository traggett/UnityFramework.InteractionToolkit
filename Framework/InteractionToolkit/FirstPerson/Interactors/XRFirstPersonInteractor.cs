using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace FirstPerson
		{
			/// <summary>
			/// First person gesture based interactor
			/// </summary>
			public class XRFirstPersonInteractor : XRBaseControllerInteractor, IXRGrabInteractor
			{
				#region Public Data
				public Transform _visuals;

				public bool _constrainSelected = true;
				public bool _constrainHovered = true;

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

				private bool _returningFromConstraints;
				#endregion



				//Ok so when interacting....

				//Want transition time (eg move hand to door knob)
				//Then hand is positioned just on interactable.

				//So when start interacting, position on interactor.

				//Then do nothing? 
				//Allow gestures???

				//eg door - moving mouse in direction of door will open it??
				//maybe play animation and release with velocity - driven by gesture speed.

				//What about non constrained objects???
				//eg picking up paper..
				//Want transition but position object at ideal pick up pos?? (realtive to player camera)

				//Dont allow camera movement or controls whilst interacting???
				//So locked in, maybe pressing back will exit the interaction too.


				#region IXRGrabInteractor
				public XRNode GetGrabNode()
				{
					return XRNode.RightHand;
				}

				public bool CanGrab()
				{
					return true;
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

				protected override void OnSelectEntering(SelectEnterEventArgs args)
				{
					base.OnSelectEntering(args);
				}

				protected override void OnSelectExiting(SelectExitEventArgs args)
				{
					base.OnSelectExiting(args);

				}

				public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
				{
					base.ProcessInteractor(updatePhase);

					switch (updatePhase)
					{
						case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
							{
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
				#endregion
			}
		}
	}
}