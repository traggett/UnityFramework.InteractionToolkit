﻿using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace FirstPerson
		{
			[RequireComponent(typeof(CharacterController))]
			public class ActionBasedFirstPersonController : MonoBehaviour
			{
				[Header("Player")]
				[Tooltip("Move speed of the character in m/s")]
				public float MoveSpeed = 4.0f;
				[Tooltip("Sprint speed of the character in m/s")]
				public float SprintSpeed = 6.0f;
				[Tooltip("Rotation speed of the character")]
				public float RotationSpeed = 1.0f;
				[Tooltip("Acceleration and deceleration")]
				public float SpeedChangeRate = 10.0f;

				[Space(10)]
				[Tooltip("The height the player can jump")]
				public float JumpHeight = 1.2f;
				[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
				public float Gravity = -15.0f;

				[Space(10)]
				[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
				public float JumpTimeout = 0.1f;
				[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
				public float FallTimeout = 0.15f;

				[Header("Player Grounded")]
				[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
				public bool Grounded = true;
				[Tooltip("Useful for rough ground")]
				public float GroundedOffset = -0.14f;
				[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
				public float GroundedRadius = 0.5f;
				[Tooltip("What layers the character uses as ground")]
				public LayerMask GroundLayers;

				[Header("Cinemachine")]
				[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
				public GameObject CinemachineCameraTarget;
				[Tooltip("How far in degrees can you move the camera up")]
				public float TopClamp = 90.0f;
				[Tooltip("How far in degrees can you move the camera down")]
				public float BottomClamp = -90.0f;

				public InputActionProperty JumpAction
				{
					get => _jumpAction;
					set => SetInputActionProperty(ref _jumpAction, value);
				}

				public InputActionProperty MoveAction
				{
					get => _moveAction;
					set => SetInputActionProperty(ref _moveAction, value);
				}

				public InputActionProperty LookAction
				{
					get => _lookAction;
					set => SetInputActionProperty(ref _lookAction, value);
				}

				public InputActionProperty SprintAction
				{
					get => _sprintAction;
					set => SetInputActionProperty(ref _sprintAction, value);
				}

				[Header("Input Actions")]
				[SerializeField]
				private InputActionProperty _moveAction;
				[SerializeField]
				private InputActionProperty _lookAction;
				[SerializeField]
				private InputActionProperty _sprintAction;
				[SerializeField]
				private InputActionProperty _jumpAction;

				public bool _analogMovement;


				// cinemachine
				private float _cinemachineTargetPitch;

				// player
				private float _speed;
				private float _rotationVelocity;
				private float _verticalVelocity;
				private float _terminalVelocity = 53.0f;

				// timeout deltatime
				private float _jumpTimeoutDelta;
				private float _fallTimeoutDelta;

				private CharacterController _controller;
				private GameObject _mainCamera;

				private const float _threshold = 0.01f;

				private void Awake()
				{
					// get a reference to our main camera
					if (_mainCamera == null)
					{
						_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
					}
				}

				private void Start()
				{
					_controller = GetComponent<CharacterController>();

					// reset our timeouts on start
					_jumpTimeoutDelta = JumpTimeout;
					_fallTimeoutDelta = FallTimeout;
				}

				private void Update()
				{
					JumpAndGravity();
					GroundedCheck();
					Move();
				}

				private void LateUpdate()
				{
					CameraRotation();
				}

				private void GroundedCheck()
				{
					// set sphere position, with offset
					Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
					Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
				}

				private void CameraRotation()
				{
					if (TryRead2DAxis(_lookAction.action, out Vector2 lookInput) && lookInput.sqrMagnitude >= _threshold)
					{
						_cinemachineTargetPitch += lookInput.y * RotationSpeed * Time.deltaTime;
						_rotationVelocity = lookInput.x * RotationSpeed * Time.deltaTime;

						// clamp our pitch rotation
						_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

						// Update Cinemachine camera target pitch
						CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

						// rotate the player left and right
						transform.Rotate(Vector3.up * _rotationVelocity);
					}
				}

				private void Move()
				{
					if (TryRead2DAxis(_moveAction.action, out Vector2 movementInput))
					{
						// set target speed based on move speed, sprint speed and if sprint is pressed
						float targetSpeed = InputHelpers.IsPressed(_sprintAction.action) ? SprintSpeed : MoveSpeed;

						// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

						// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
						// if there is no input, set the target speed to 0
						if (movementInput == Vector2.zero) targetSpeed = 0.0f;

						// a reference to the players current horizontal velocity
						float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

						float speedOffset = 0.1f;
						float inputMagnitude = _analogMovement ? movementInput.magnitude : 1f;

						// accelerate or decelerate to target speed
						if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
						{
							// creates curved result rather than a linear one giving a more organic speed change
							// note T in Lerp is clamped, so we don't need to clamp our speed
							_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

							// round speed to 3 decimal places
							_speed = Mathf.Round(_speed * 1000f) / 1000f;
						}
						else
						{
							_speed = targetSpeed;
						}

						// normalise input direction
						Vector3 inputDirection = new Vector3(movementInput.x, 0.0f, movementInput.y).normalized;

						// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
						// if there is a move input rotate player when the player is moving
						if (movementInput != Vector2.zero)
						{
							// move
							inputDirection = transform.right * movementInput.x + transform.forward * movementInput.y;
						}

						// move the player
						_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
					}
				}

				private void JumpAndGravity()
				{
					if (Grounded)
					{
						// reset the fall timeout timer
						_fallTimeoutDelta = FallTimeout;

						// stop our velocity dropping infinitely when grounded
						if (_verticalVelocity < 0.0f)
						{
							_verticalVelocity = -2f;
						}

						// Jump
						if (InputHelpers.IsPressed(_jumpAction.action) && _jumpTimeoutDelta <= 0.0f)
						{
							// the square root of H * -2 * G = how much velocity needed to reach desired height
							_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
						}

						// jump timeout
						if (_jumpTimeoutDelta >= 0.0f)
						{
							_jumpTimeoutDelta -= Time.deltaTime;
						}
					}
					else
					{
						// reset the jump timeout timer
						_jumpTimeoutDelta = JumpTimeout;

						// fall timeout
						if (_fallTimeoutDelta >= 0.0f)
						{
							_fallTimeoutDelta -= Time.deltaTime;
						}

						// if we are not grounded, do not jump
						//_input.jump = false;
					}

					// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
					if (_verticalVelocity < _terminalVelocity)
					{
						_verticalVelocity += Gravity * Time.deltaTime;
					}
				}

				private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
				{
					if (lfAngle < -360f) lfAngle += 360f;
					if (lfAngle > 360f) lfAngle -= 360f;
					return Mathf.Clamp(lfAngle, lfMin, lfMax);
				}

				private void OnDrawGizmosSelected()
				{
					Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
					Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

					if (Grounded) Gizmos.color = transparentGreen;
					else Gizmos.color = transparentRed;

					// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
					Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
				}

				private void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
				{
					if (Application.isPlaying)
						property.DisableDirectAction();

					property = value;

					if (Application.isPlaying && isActiveAndEnabled)
						property.EnableDirectAction();
				}

				private static bool TryRead2DAxis(InputAction action, out Vector2 output)
				{
					if (action != null)
					{
						output = action.ReadValue<Vector2>();
						return true;
					}
					output = default;
					return false;
				}
			}
		}
	}
}