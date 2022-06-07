using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// Component based on XRGrabInteractable which allows for constrained movement.
		/// Good for things like doors, levers etc
		/// </summary>
		[SelectionBase]
		[DisallowMultipleComponent]
		[RequireComponent(typeof(Rigidbody))]
		public class XRAdvancedGrabInteractable : XRBaseInteractable
		{
			protected const float k_DefaultTighteningAmount = 0.5f;
			protected const float k_DefaultSmoothingAmount = 5f;
			protected const float k_VelocityDamping = 1f;
			protected const float k_VelocityScale = 1f;
			protected const float k_AngularVelocityDamping = 1f;
			protected const float k_AngularVelocityScale = 1f;
			protected const int k_ThrowSmoothingFrameCount = 20;
			protected const float k_DefaultAttachEaseInTime = 0.15f;
			protected const float k_DefaultThrowSmoothingDuration = 0.25f;
			protected const float k_DefaultThrowVelocityScale = 1.5f;
			protected const float k_DefaultThrowAngularVelocityScale = 1f;

			[SerializeField]
			private float m_AttachEaseInTime = k_DefaultAttachEaseInTime;

			/// <summary>
			/// Time in seconds to ease in the attach when selected (a value of 0 indicates no easing).
			/// </summary>
			public float attachEaseInTime
			{
				get => m_AttachEaseInTime;
				set => m_AttachEaseInTime = value;
			}

			[SerializeField]
			private MovementType m_MovementType = MovementType.Instantaneous;

			/// <summary>
			/// Specifies how this object is moved when selected, either through setting the velocity of the <see cref="Rigidbody"/>,
			/// moving the kinematic <see cref="Rigidbody"/> during Fixed Update, or by directly updating the <see cref="Transform"/> each frame.
			/// </summary>
			/// <seealso cref="XRBaseInteractable.MovementType"/>
			public MovementType movementType
			{
				get => m_MovementType;
				set => m_MovementType = value;
			}

			[SerializeField, Range(0f, 1f)]
			private float m_VelocityDamping = k_VelocityDamping;

			/// <summary>
			/// Scale factor of how much to dampen the existing velocity when tracking the position of the Interactor.
			/// The smaller the value, the longer it takes for the velocity to decay.
			/// </summary>
			/// <remarks>
			/// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
			/// </remarks>
			/// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
			/// <seealso cref="trackPosition"/>
			public float velocityDamping
			{
				get => m_VelocityDamping;
				set => m_VelocityDamping = value;
			}

			[SerializeField]
			private float m_VelocityScale = k_VelocityScale;

			/// <summary>
			/// Scale factor applied to the tracked velocity while updating the <see cref="Rigidbody"/>
			/// when tracking the position of the Interactor.
			/// </summary>
			/// <remarks>
			/// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
			/// </remarks>
			/// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
			/// <seealso cref="trackPosition"/>
			public float velocityScale
			{
				get => m_VelocityScale;
				set => m_VelocityScale = value;
			}

			[SerializeField, Range(0f, 1f)]
			private float m_AngularVelocityDamping = k_AngularVelocityDamping;

			/// <summary>
			/// Scale factor of how much to dampen the existing angular velocity when tracking the rotation of the Interactor.
			/// The smaller the value, the longer it takes for the angular velocity to decay.
			/// </summary>
			/// <remarks>
			/// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
			/// </remarks>
			/// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
			/// <seealso cref="trackRotation"/>
			public float angularVelocityDamping
			{
				get => m_AngularVelocityDamping;
				set => m_AngularVelocityDamping = value;
			}

			[SerializeField]
			private float m_AngularVelocityScale = k_AngularVelocityScale;

			/// <summary>
			/// Scale factor applied to the tracked angular velocity while updating the <see cref="Rigidbody"/>
			/// when tracking the rotation of the Interactor.
			/// </summary>
			/// <remarks>
			/// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
			/// </remarks>
			/// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
			/// <seealso cref="trackRotation"/>
			public float angularVelocityScale
			{
				get => m_AngularVelocityScale;
				set => m_AngularVelocityScale = value;
			}

			[SerializeField]
			private bool m_TrackPosition = true;

			/// <summary>
			/// Whether this object should follow the position of the Interactor when selected.
			/// </summary>
			/// <seealso cref="trackRotation"/>
			public bool trackPosition
			{
				get => m_TrackPosition;
				set => m_TrackPosition = value;
			}

			[SerializeField]
			private bool m_SmoothPosition;

			/// <summary>
			/// Apply smoothing while following the position of the Interactor when selected.
			/// </summary>
			/// <seealso cref="smoothPositionAmount"/>
			/// <seealso cref="tightenPosition"/>
			public bool smoothPosition
			{
				get => m_SmoothPosition;
				set => m_SmoothPosition = value;
			}

			[SerializeField, Range(0f, 20f)]
			private float m_SmoothPositionAmount = k_DefaultSmoothingAmount;

			/// <summary>
			/// Scale factor for how much smoothing is applied while following the position of the Interactor when selected.
			/// The larger the value, the closer this object will remain to the position of the Interactor.
			/// </summary>
			/// <seealso cref="smoothPosition"/>
			/// <seealso cref="tightenPosition"/>
			public float smoothPositionAmount
			{
				get => m_SmoothPositionAmount;
				set => m_SmoothPositionAmount = value;
			}

			[SerializeField, Range(0f, 1f)]
			private float m_TightenPosition = k_DefaultTighteningAmount;

			/// <summary>
			/// Reduces the maximum follow position difference when using smoothing.
			/// </summary>
			/// <remarks>
			/// Fractional amount of how close the smoothed position should remain to the position of the Interactor when using smoothing.
			/// The value ranges from 0 meaning no bias in the smoothed follow distance,
			/// to 1 meaning effectively no smoothing at all.
			/// </remarks>
			/// <seealso cref="smoothPosition"/>
			/// <seealso cref="smoothPositionAmount"/>
			public float tightenPosition
			{
				get => m_TightenPosition;
				set => m_TightenPosition = value;
			}

			[SerializeField]
			private bool m_TrackRotation = true;

			/// <summary>
			/// Whether this object should follow the rotation of the Interactor when selected.
			/// </summary>
			/// <seealso cref="trackPosition"/>
			public bool trackRotation
			{
				get => m_TrackRotation;
				set => m_TrackRotation = value;
			}

			[SerializeField]
			bool m_SmoothRotation;

			/// <summary>
			/// Apply smoothing while following the rotation of the Interactor when selected.
			/// </summary>
			/// <seealso cref="smoothRotationAmount"/>
			/// <seealso cref="tightenRotation"/>
			public bool smoothRotation
			{
				get => m_SmoothRotation;
				set => m_SmoothRotation = value;
			}

			[SerializeField, Range(0f, 20f)]
			private float m_SmoothRotationAmount = k_DefaultSmoothingAmount;

			/// <summary>
			/// Scale factor for how much smoothing is applied while following the rotation of the Interactor when selected.
			/// The larger the value, the closer this object will remain to the rotation of the Interactor.
			/// </summary>
			/// <seealso cref="smoothRotation"/>
			/// <seealso cref="tightenRotation"/>
			public float smoothRotationAmount
			{
				get => m_SmoothRotationAmount;
				set => m_SmoothRotationAmount = value;
			}

			[SerializeField, Range(0f, 1f)]
			private float m_TightenRotation = k_DefaultTighteningAmount;

			/// <summary>
			/// Reduces the maximum follow rotation difference when using smoothing.
			/// </summary>
			/// <remarks>
			/// Fractional amount of how close the smoothed rotation should remain to the rotation of the Interactor when using smoothing.
			/// The value ranges from 0 meaning no bias in the smoothed follow rotation,
			/// to 1 meaning effectively no smoothing at all.
			/// </remarks>
			/// <seealso cref="smoothRotation"/>
			/// <seealso cref="smoothRotationAmount"/>
			public float tightenRotation
			{
				get => m_TightenRotation;
				set => m_TightenRotation = value;
			}

			[SerializeField]
			private bool m_ThrowOnDetach = true;

			/// <summary>
			/// Whether this object inherits the velocity of the Interactor when released.
			/// </summary>
			public bool throwOnDetach
			{
				get => m_ThrowOnDetach;
				set => m_ThrowOnDetach = value;
			}

			[SerializeField]
			private float m_ThrowSmoothingDuration = k_DefaultThrowSmoothingDuration;

			/// <summary>
			/// Time period to average thrown velocity over.
			/// </summary>
			/// <seealso cref="throwOnDetach"/>
			public float throwSmoothingDuration
			{
				get => m_ThrowSmoothingDuration;
				set => m_ThrowSmoothingDuration = value;
			}

			[SerializeField]
			private AnimationCurve m_ThrowSmoothingCurve = AnimationCurve.Linear(1f, 1f, 1f, 0f);

			/// <summary>
			/// The curve to use to weight thrown velocity smoothing (most recent frames to the right).
			/// </summary>
			/// <seealso cref="throwOnDetach"/>
			public AnimationCurve throwSmoothingCurve
			{
				get => m_ThrowSmoothingCurve;
				set => m_ThrowSmoothingCurve = value;
			}

			[SerializeField]
			private float m_ThrowVelocityScale = k_DefaultThrowVelocityScale;

			/// <summary>
			/// Scale factor applied to this object's inherited velocity of the Interactor when released.
			/// </summary>
			/// <seealso cref="throwOnDetach"/>
			public float throwVelocityScale
			{
				get => m_ThrowVelocityScale;
				set => m_ThrowVelocityScale = value;
			}

			[SerializeField]
			private float m_ThrowAngularVelocityScale = k_DefaultThrowAngularVelocityScale;

			/// <summary>
			/// Scale factor applied to this object's inherited angular velocity of the Interactor when released.
			/// </summary>
			/// <seealso cref="throwOnDetach"/>
			public float throwAngularVelocityScale
			{
				get => m_ThrowAngularVelocityScale;
				set => m_ThrowAngularVelocityScale = value;
			}

			[SerializeField]
			private bool m_ForceGravityOnDetach;

			/// <summary>
			/// Force this object to have gravity when released
			/// (will still use pre-grab value if this is <see langword="false"/>).
			/// </summary>
			public bool forceGravityOnDetach
			{
				get => m_ForceGravityOnDetach;
				set => m_ForceGravityOnDetach = value;
			}

			/// <summary>
			/// Gets or sets the event that is called when the interactible is grabbed.
			/// </summary>
			/// <remarks>
			/// The <see cref="GrabEventArgs"/> passed to each listener is only valid while the event is invoked,
			/// do not hold a reference to it.
			/// </remarks>
			public GrabEvent onGrab
			{
				get => m_grabEvent;
				set => m_grabEvent = value;
			}

			[SerializeField]
			GrabEvent m_grabEvent = new GrabEvent();

			/// <summary>
			/// Gets or sets the event that is called when the interactible is dropped.
			/// </summary>
			/// <remarks>
			/// The <see cref="DropEventArgs"/> passed to each listener is only valid while the event is invoked,
			/// do not hold a reference to it.
			/// </remarks>
			public DropEvent onDrop
			{
				get => m_dropEvent;
				set => m_dropEvent = value;
			}

			[SerializeField]
			DropEvent m_dropEvent = new DropEvent();

			public XRInteractableConstraint Constraint
			{
				get => m_Constraint;
			}

			public Rigidbody Rigidbody
			{
				get => m_Rigidbody;
			}

			// Point we are attaching to on this Interactable (in Interactor's attach coordinate space)
			public Vector3 InteractorLocalAttachPosition
			{
				get => m_InteractorLocalPosition;
				set => m_InteractorLocalPosition = value;
			}

			// Point we are moving towards each frame (eventually will be at Interactor's attach point)
			public Quaternion InteractorLocalAttachRotation
			{
				get => m_InteractorLocalRotation;
				set => m_InteractorLocalRotation = value;
			}

			private Vector3 m_InteractorLocalPosition;
			private Quaternion m_InteractorLocalRotation;

			private Vector3 m_TargetWorldPosition;
			private Quaternion m_TargetWorldRotation;

			private float m_CurrentAttachEaseTime;
			protected MovementType m_CurrentMovementType;

			private bool m_DetachInLateUpdate;
			private Vector3 m_DetachVelocity;
			private Vector3 m_DetachAngularVelocity;

			private int m_ThrowSmoothingCurrentFrame;
			private readonly float[] m_ThrowSmoothingFrameTimes = new float[k_ThrowSmoothingFrameCount];
			private readonly Vector3[] m_ThrowSmoothingVelocityFrames = new Vector3[k_ThrowSmoothingFrameCount];
			private readonly Vector3[] m_ThrowSmoothingAngularVelocityFrames = new Vector3[k_ThrowSmoothingFrameCount];

			private Rigidbody m_Rigidbody;
			private Vector3 m_LastPosition;
			private Quaternion m_LastRotation;

			private XRInteractableConstraint m_Constraint;
				
			// Rigidbody's settings upon select, kept to restore these values when dropped
			private bool m_WasKinematic;
			private bool m_UsedGravity;
			private float m_OldDrag;
			private float m_OldAngularDrag;

			public Vector3 GetDetachVelocity()
			{
				return m_DetachVelocity;
			}

			public bool IsEasingIn()
			{
				return isSelected && m_AttachEaseInTime > 0f && m_CurrentAttachEaseTime < m_AttachEaseInTime;
			}

			#region Unity Messages
			protected override void Awake()
			{
				base.Awake();

				m_CurrentMovementType = m_MovementType;
				m_Rigidbody = GetComponent<Rigidbody>();
				if (m_Rigidbody == null)
					UnityEngine.Debug.LogError("Grab Interactable does not have a required Rigidbody.", this);

				m_Constraint = GetComponent<XRInteractableConstraint>();
			}
			#endregion

			#region Virtual Interface
			/// <summary>
			/// Updates the state of the object due to being grabbed.
			/// Automatically called when entering the Select state.
			/// </summary>
			/// <seealso cref="Drop"/>
			protected virtual void Grab()
			{
				// Special case where the interactor will override this objects movement type (used for Sockets and other absolute interactors)
				m_CurrentMovementType = selectingInteractor.selectedInteractableMovementTypeOverride ?? m_MovementType;

				SetupRigidbodyGrab(m_Rigidbody);

				// Reset detach velocities
				m_DetachVelocity = Vector3.zero;
				m_DetachAngularVelocity = Vector3.zero;

				// Initialize target pose for easing and smoothing
				m_TargetWorldPosition = transform.position;
				m_TargetWorldRotation = transform.rotation;

				m_CurrentAttachEaseTime = 0f;

				//Work out where to attach the object to.
				UpdateInteractorLocalPose(selectingInteractor);

				SmoothVelocityStart();
			}

			/// <summary>
			/// Updates the state of the object due to being dropped and schedule to finish the detach during the end of the frame.
			/// Automatically called when exiting the Select state.
			/// </summary>
			/// <seealso cref="Detach"/>
			/// <seealso cref="Grab"/>
			protected virtual void Drop()
			{
				SetupRigidbodyDrop(m_Rigidbody);

				m_CurrentMovementType = m_MovementType;
				m_DetachInLateUpdate = true;

				SmoothVelocityEnd();
			}

			/// <summary>
			/// Updates the state of the object to finish the detach after being dropped.
			/// Automatically called during the end of the frame after being dropped.
			/// </summary>
			/// <remarks>
			/// This method will update the velocity of the Rigidbody if configured to do so.
			/// </remarks>
			/// <seealso cref="Drop"/>
			protected virtual void Detach()
			{
				if (m_ThrowOnDetach)
				{
					m_Rigidbody.velocity = m_DetachVelocity;
					m_Rigidbody.angularVelocity = Vector3.zero;
					m_Rigidbody.AddTorque(m_DetachAngularVelocity, ForceMode.VelocityChange);
				}
			}

			/// <summary>
			/// Setup the <see cref="Rigidbody"/> on this object due to being grabbed.
			/// Automatically called when entering the Select state.
			/// </summary>
			/// <param name="rigidbody">The <see cref="Rigidbody"/> on this object.</param>
			/// <seealso cref="SetupRigidbodyDrop"/>
			// ReSharper disable once ParameterHidesMember
			protected virtual void SetupRigidbodyGrab(Rigidbody rigidbody)
			{
				// Remember Rigidbody settings and setup to move
				m_WasKinematic = rigidbody.isKinematic;
				m_UsedGravity = rigidbody.useGravity;
				m_OldDrag = rigidbody.drag;
				m_OldAngularDrag = rigidbody.angularDrag;
				rigidbody.isKinematic = m_CurrentMovementType == MovementType.Kinematic || m_CurrentMovementType == MovementType.Instantaneous;
				rigidbody.useGravity = false;
				rigidbody.drag = 0f;
				rigidbody.angularDrag = 0f;
			}

			/// <summary>
			/// Setup the <see cref="Rigidbody"/> on this object due to being dropped.
			/// Automatically called when exiting the Select state.
			/// </summary>
			/// <param name="rigidbody">The <see cref="Rigidbody"/> on this object.</param>
			/// <seealso cref="SetupRigidbodyGrab"/>
			// ReSharper disable once ParameterHidesMember
			protected virtual void SetupRigidbodyDrop(Rigidbody rigidbody)
			{
				// Restore Rigidbody settings
				rigidbody.isKinematic = m_WasKinematic;
				rigidbody.useGravity = m_UsedGravity | m_ForceGravityOnDetach;
				rigidbody.drag = m_OldDrag;
				rigidbody.angularDrag = m_OldAngularDrag;
			}
			#endregion

			#region XRBaseInteractable
			public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
			{
				base.ProcessInteractable(updatePhase);

				if (m_Constraint != null)
				{
					m_Constraint.ProcessConstraint(updatePhase);
				}
				
				switch (updatePhase)
				{
					// During Fixed update we want to perform any physics-based updates (e.g., Kinematic or VelocityTracking).
					case XRInteractionUpdateOrder.UpdatePhase.Fixed:
						{
							if (IsAttachedToInteractor())
							{
								if (m_CurrentMovementType == MovementType.Kinematic)
									PerformKinematicUpdate(updatePhase);
								else if (m_CurrentMovementType == MovementType.VelocityTracking)
									PerformVelocityTrackingUpdate(Time.deltaTime, updatePhase);
							}
						}
						break;

					// During Dynamic update we want to perform any Transform-based manipulation (e.g., Instantaneous).
					case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
						{
							if (IsAttachedToInteractor())
							{
								UpdateTarget(Time.deltaTime);
								SmoothVelocityUpdate();

								if (m_CurrentMovementType == MovementType.Instantaneous)
									PerformInstantaneousUpdate(updatePhase);
							}
						}
						break;

					// During OnBeforeRender we want to perform any last minute Transform position changes before rendering (e.g., Instantaneous).
					case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
						{
							if (IsAttachedToInteractor())
							{
								UpdateTarget(Time.deltaTime);

								if (m_CurrentMovementType == MovementType.Instantaneous)
									PerformInstantaneousUpdate(updatePhase);
							}
						}                   
						break;

					// Late update is only used to handle detach as late as possible.
					case XRInteractionUpdateOrder.UpdatePhase.Late:
						{
							if (m_DetachInLateUpdate)
							{
								if (selectingInteractor == null)
									Detach();

								m_DetachInLateUpdate = false;
							}
						}
						break;
				}
			}

			/// <inheritdoc />
			protected override void OnSelectEntering(SelectEnterEventArgs args)
			{
				base.OnSelectEntering(args);

				Grab();

				//Trigger grab event (after finished selected event and grabbed item)
				GrabEventArgs grabEventArgs = new GrabEventArgs()
				{
					interactable = this,
					interactor = selectingInteractor,
				};
				onGrab?.Invoke(grabEventArgs);
			}

			/// <inheritdoc />
			protected override void OnSelectExiting(SelectExitEventArgs args)
			{
				base.OnSelectExiting(args);

				Drop();

				//Trigger drop event (after finished selected exit event and dropped item)
				DropEventArgs dropEventArgs = new DropEventArgs()
				{
					interactable = this,
					interactor = args.interactor,
					velocity = m_DetachVelocity,
					angularVelocity = m_DetachAngularVelocity,
				};
				onDrop?.Invoke(dropEventArgs);
			}
			#endregion

			#region Private Functions
			private bool IsAttachedToInteractor()
			{
				if (isSelected)
				{
					if (selectingInteractor is IXRGrabInteractor grabInteractor)
					{
						return grabInteractor.IsGrabbedObjectAttached();
					}

					return true;
				}

				return false;
			}

			private Vector3 GetWorldAttachPosition(XRBaseInteractor interactor)
			{
				return interactor.attachTransform.position + GetWorldAttachRotation(interactor) * m_InteractorLocalPosition;
			}

			private Quaternion GetWorldAttachRotation(XRBaseInteractor interactor)
			{
				return interactor.attachTransform.rotation * m_InteractorLocalRotation;
			}

			private void UpdateTarget(float timeDelta)
			{
				// Compute the unsmoothed target world position and rotation
				var rawTargetWorldPosition = GetWorldAttachPosition(selectingInteractor);
				var rawTargetWorldRotation = GetWorldAttachRotation(selectingInteractor);

				//If we have a constraint, constrian the target position and rotation
				if (m_Constraint != null)
				{
					m_Constraint.ConstrainTargetTransform(ref rawTargetWorldPosition, ref rawTargetWorldRotation);
				}

				// Apply smoothing (if configured)
				if (m_AttachEaseInTime > 0f && m_CurrentAttachEaseTime < m_AttachEaseInTime)
				{
					m_CurrentAttachEaseTime += timeDelta;

					if (m_CurrentAttachEaseTime >= m_AttachEaseInTime)
					{
						m_TargetWorldPosition = rawTargetWorldPosition;
						m_TargetWorldRotation = rawTargetWorldRotation;
					}
					else
					{
						var easePercent = Mathf.Clamp01(m_CurrentAttachEaseTime / m_AttachEaseInTime);
						m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, rawTargetWorldPosition, easePercent);
						m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, rawTargetWorldRotation, easePercent);
					}
				}
				else
				{
					if (m_SmoothPosition)
					{
						m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, rawTargetWorldPosition, m_SmoothPositionAmount * timeDelta);
						m_TargetWorldPosition = Vector3.Lerp(m_TargetWorldPosition, rawTargetWorldPosition, m_TightenPosition);
					}
					else
					{
						m_TargetWorldPosition = rawTargetWorldPosition;
					}

					if (m_SmoothRotation)
					{
						m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, rawTargetWorldRotation, m_SmoothRotationAmount * timeDelta);
						m_TargetWorldRotation = Quaternion.Slerp(m_TargetWorldRotation, rawTargetWorldRotation, m_TightenRotation);
					}
					else
					{
						m_TargetWorldRotation = rawTargetWorldRotation;
					}
				}
			}

			private void PerformInstantaneousUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
			{
				if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic ||
					updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
				{
					if (m_TrackPosition)
					{
						transform.position = m_TargetWorldPosition;
					}

					if (m_TrackRotation)
					{
						transform.rotation = m_TargetWorldRotation;
					}
				}
			}

			private void PerformKinematicUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
			{
				if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
				{
					if (m_TrackPosition)
					{
						var position = m_TargetWorldPosition - m_Rigidbody.worldCenterOfMass + m_Rigidbody.position;
						m_Rigidbody.velocity = Vector3.zero;
						m_Rigidbody.MovePosition(position);

					}

					if (m_TrackRotation)
					{
						m_Rigidbody.angularVelocity = Vector3.zero;
						m_Rigidbody.MoveRotation(m_TargetWorldRotation);
					}
				}
			}

			private void PerformVelocityTrackingUpdate(float timeDelta, XRInteractionUpdateOrder.UpdatePhase updatePhase)
			{
				if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
				{
					// Do velocity tracking
					if (m_TrackPosition)
					{
						// Scale initialized velocity by prediction factor
						m_Rigidbody.velocity *= (1f - m_VelocityDamping);
						var positionDelta = m_TargetWorldPosition - m_Rigidbody.worldCenterOfMass;
						var velocity = positionDelta / timeDelta;

						if (!float.IsNaN(velocity.x))
							m_Rigidbody.velocity += (velocity * m_VelocityScale);
					}

					// Do angular velocity tracking
					if (m_TrackRotation)
					{
						// Scale initialized velocity by prediction factor
						m_Rigidbody.angularVelocity *= (1f - m_AngularVelocityDamping);
						var rotationDelta = m_TargetWorldRotation * Quaternion.Inverse(transform.rotation);
						rotationDelta.ToAngleAxis(out var angleInDegrees, out var rotationAxis);
						if (angleInDegrees > 180f)
							angleInDegrees -= 360f;

						if (Mathf.Abs(angleInDegrees) > Mathf.Epsilon)
						{
							var angularVelocity = (rotationAxis * (angleInDegrees * Mathf.Deg2Rad)) / timeDelta;
							if (!float.IsNaN(angularVelocity.x))
								m_Rigidbody.angularVelocity += (angularVelocity * m_AngularVelocityScale);
						}
					}
				}
			}

			private void UpdateInteractorLocalPose(XRBaseInteractor interactor)
			{
				//Find offset from interactors attach rotation and local rotation
				InteractorLocalAttachRotation = Quaternion.Inverse(interactor.attachTransform.rotation) * this.transform.rotation;

				//Find position offset from interactors attach position in interactors attach transforms local space
				Vector3 attachOffset = this.transform.position - interactor.attachTransform.position;
				InteractorLocalAttachPosition = Quaternion.Inverse(InteractorLocalAttachRotation) * interactor.attachTransform.InverseTransformDirection(attachOffset);
			}

			private void SmoothVelocityStart()
			{
				m_LastPosition = this.transform.position;
				m_LastRotation = this.transform.rotation;
				Array.Clear(m_ThrowSmoothingFrameTimes, 0, m_ThrowSmoothingFrameTimes.Length);
				Array.Clear(m_ThrowSmoothingVelocityFrames, 0, m_ThrowSmoothingVelocityFrames.Length);
				Array.Clear(m_ThrowSmoothingAngularVelocityFrames, 0, m_ThrowSmoothingAngularVelocityFrames.Length);
				m_ThrowSmoothingCurrentFrame = 0;
			}

			private void SmoothVelocityEnd()
			{
				if (m_ThrowOnDetach)
				{
					var smoothedVelocity = GetSmoothedVelocityValue(m_ThrowSmoothingVelocityFrames);
					var smoothedAngularVelocity = GetSmoothedVelocityValue(m_ThrowSmoothingAngularVelocityFrames);
					m_DetachVelocity = smoothedVelocity * m_ThrowVelocityScale;
					m_DetachAngularVelocity = smoothedAngularVelocity * m_ThrowAngularVelocityScale;
				}
				else
				{
					m_DetachVelocity = Vector3.zero;
					m_DetachAngularVelocity = Vector3.zero;
				}
			}

			private void SmoothVelocityUpdate()
			{
				m_ThrowSmoothingFrameTimes[m_ThrowSmoothingCurrentFrame] = Time.time;
				m_ThrowSmoothingVelocityFrames[m_ThrowSmoothingCurrentFrame] = (this.transform.position - m_LastPosition) / Time.deltaTime;

				var velocityDiff = (this.transform.rotation * Quaternion.Inverse(m_LastRotation));
				m_ThrowSmoothingAngularVelocityFrames[m_ThrowSmoothingCurrentFrame] =
					(new Vector3(Mathf.DeltaAngle(0f, velocityDiff.eulerAngles.x),
							Mathf.DeltaAngle(0f, velocityDiff.eulerAngles.y),
							Mathf.DeltaAngle(0f, velocityDiff.eulerAngles.z))
						/ Time.deltaTime) * Mathf.Deg2Rad;

				m_ThrowSmoothingCurrentFrame = (m_ThrowSmoothingCurrentFrame + 1) % k_ThrowSmoothingFrameCount;
				m_LastPosition = this.transform.position;
				m_LastRotation = this.transform.rotation;
			}

			private Vector3 GetSmoothedVelocityValue(Vector3[] velocityFrames)
			{
				var calcVelocity = new Vector3();
				var totalWeights = 0f;
				for (var frameCounter = 0; frameCounter < k_ThrowSmoothingFrameCount; ++frameCounter)
				{
					var frameIdx = (((m_ThrowSmoothingCurrentFrame - frameCounter - 1) % k_ThrowSmoothingFrameCount) + k_ThrowSmoothingFrameCount) % k_ThrowSmoothingFrameCount;
					if (m_ThrowSmoothingFrameTimes[frameIdx] == 0f)
						break;

					var timeAlpha = (Time.time - m_ThrowSmoothingFrameTimes[frameIdx]) / m_ThrowSmoothingDuration;
					var velocityWeight = m_ThrowSmoothingCurve.Evaluate(Mathf.Clamp(1f - timeAlpha, 0f, 1f));
					calcVelocity += velocityFrames[frameIdx] * velocityWeight;
					totalWeights += velocityWeight;
					if (Time.time - m_ThrowSmoothingFrameTimes[frameIdx] > m_ThrowSmoothingDuration)
						break;
				}

				if (totalWeights > 0f)
					return calcVelocity / totalWeights;

				return Vector3.zero;
			}
			#endregion
		}
	}
}