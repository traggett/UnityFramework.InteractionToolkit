using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public class XRSimpleAnimatedHand : XRHandVisuals
			{
				#region Public Data
				/// <summary>
				/// Animation param name for a float used for clenching the fist.
				/// </summary>
				public const string ANIM_PARAM_NAME_FIST = "Fist";
				/// <summary>
				/// Animation param name for a float used for point index finger.
				/// </summary>
				public const string ANIM_PARAM_NAME_POINT = "Point";
				/// <summary>
				/// Animation param name for a index to the current override pose animation (should either be 0 or 1)
				/// </summary>
				public const string ANIM_PARAM_NAME_POSE = "Pose";
				/// <summary>
				/// The layer name used to blend the override pose.
				/// </summary>
				public const string ANIM_LAYER_NAME_POSE = "Override Pose";

				/// <summary>
				/// Flags for which hand this represents (should either be HandFlags.Left or HandFlags.Right)
				/// </summary>
				public HandFlags _handFlags = HandFlags.Right;

				/// <summary>
				/// The root of the hands visuals.
				/// These will be moved away from the controller if an override pose is applied so should be different to the controller root.
				/// </summary>
				public Transform _visualsRoot;

				/// <summary>
				/// The animator for this hand.
				/// </summary>
				public Animator _animator;

				/// <summary>
				///  Optional Input Action Input Action used to drive fingers clapsed animations.
				/// </summary>
				public InputActionProperty _gripAmountAction;

				/// <summary>
				/// Optional Input Action used to drive fingers clapsed animations.
				/// </summary>
				public InputActionProperty _gripTouchAction;

				/// <summary>
				/// Optional Input Action used to drive thumb up/down animations.
				/// </summary>
				public InputActionProperty _thumbTouchAction;

				/// <summary>
				/// Optional Input Action used to drive index finger up/down animations.
				/// </summary>
				public InputActionProperty _indexFingerAmountAction;
				
				/// <summary>
				/// The time taken to blend in an override pose.
				/// </summary>
				public float _transitionToOverridePoseTime = 0.2f;
				/// <summary>
				/// The time taken to blend from an override pose back to normal.
				/// </summary>
				public float _transitionFromOverridePoseTime = 0.3f;

				/// <summary>
				/// Animation state names for override pose animations (Need two so can transition from one to another).
				/// </summary>
				public AnimationClip _overridePoseClipA;
				public AnimationClip _overridePoseClipB;
				#endregion

				#region Private Data
				private int _animLayerIndexPose = -1;
				private int _animParamIndexFist = -1;
				private int _animParamIndexPoint = -1;
				private int _animParamIndexPose = -1;

				private AnimatorOverrideController _animatorOverrideController;
				private List<KeyValuePair<AnimationClip, AnimationClip>> _animatorClipOverrides;

				private XRHandPose _overridePose;
				private float _overridePoseLerp;

				private bool _allowOverrideMovement;

				private float _pointBlend;
				private float _thumbsUpBlend;
				#endregion

				#region Unity Messages
				private void Start()
				{
					// Get animator layer indices by name, for later use switching between hand visuals
					_animLayerIndexPose = _animator.GetLayerIndex(ANIM_LAYER_NAME_POSE);
					_animParamIndexFist = Animator.StringToHash(ANIM_PARAM_NAME_FIST);
					_animParamIndexPoint = Animator.StringToHash(ANIM_PARAM_NAME_POINT);
					_animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);

					_animatorOverrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
					_animator.runtimeAnimatorController = _animatorOverrideController;

					_animatorClipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(_animatorOverrideController.overridesCount);
					_animatorOverrideController.GetOverrides(_animatorClipOverrides);
				}

				private void Update()
				{
					UpdateInputAnimations();
				}

				private void LateUpdate()
				{
					ApplyOverridePose();
				}
				#endregion

				#region XRHand
				public override HandFlags HandFlags
				{
					get
					{
						return _handFlags;
					}
				}

				public override void ApplyOverridePose(XRHandPose handPose, bool allowMovement)
				{
					_overridePose = handPose;
					_overridePoseLerp = 0f;
					_allowOverrideMovement = allowMovement;
				}

				public override void ClearOverridePose()
				{
					_overridePose = null;
				}

				public override bool IsEnteringOverridePose()
				{
					return _overridePose != null && _overridePoseLerp < 1f;
				}

				public override bool IsReturningFromOverridePose()
				{
					return _overridePose != null && _overridePoseLerp > 0f;
				}
				#endregion

				#region Private Functions
				private void UpdateInputAnimations()
				{
					// Clench fist amount
					if (_animParamIndexFist != -1)
					{
						float grabAmount = InputHelpers.GetFloat(_gripAmountAction.action);
						_animator.SetFloat(_animParamIndexFist, grabAmount);
					}

					// Index finger point amount
					if (_animParamIndexPoint != -1)
					{
						float pointAmount = InputHelpers.GetFloat(_indexFingerAmountAction.action);
						_animator.SetFloat(_animParamIndexPoint, pointAmount);
					}
				}
				
				private void ApplyOverridePose()
				{
					//Transiton to / from override pose
					if (_overridePose != null)
					{
						//Set the override pose animation (if any)
						SetCurrentOverridePoseAnimation(_overridePose.PoseAnimation);

						//Update pose lerp
						if (_overridePoseLerp < 1f)
						{
							_overridePoseLerp += Time.deltaTime / _transitionToOverridePoseTime;
						}

						//Apply rotation
						if (_allowOverrideMovement && _overridePose.HasRotation)
						{
							Quaternion targetRotation = GetVisualsWorldRotation(_overridePose);

							if (_overridePoseLerp < 1f)
							{
								_visualsRoot.transform.rotation = Quaternion.Slerp(_visualsRoot.transform.rotation, targetRotation, _overridePoseLerp);
							}
							else
							{
								_visualsRoot.transform.rotation = targetRotation;
							}
						}
						else
						{
							if (_overridePoseLerp < 1f)
							{
								_visualsRoot.transform.localRotation = Quaternion.Slerp(_visualsRoot.transform.localRotation, Quaternion.identity, _overridePoseLerp);
							}
							else
							{
								_visualsRoot.transform.localRotation = Quaternion.identity;
							}
						}

						//Apply position
						if (_allowOverrideMovement)
						{
							Vector3 targetPosition = GetVisualsWorldPosition(_overridePose);

							if (_overridePoseLerp < 1f)
							{
								_visualsRoot.transform.position = Vector3.Lerp(_visualsRoot.transform.position, targetPosition, _overridePoseLerp);
							}
							else
							{
								_visualsRoot.transform.position = targetPosition;
							}
						}
						else
						{
							if (_overridePoseLerp < 1f)
							{
								_visualsRoot.transform.localPosition = Vector3.Lerp(_visualsRoot.transform.localPosition, Vector3.zero, _overridePoseLerp);
							}
							else
							{
								_visualsRoot.transform.localPosition = Vector3.zero;
							}
						}
					}
					//No override pose, lerp back to neutral
					else
					{
						//Fade out override pose
						if (_overridePoseLerp > 0f)
						{
							_overridePoseLerp -= Time.deltaTime / _transitionFromOverridePoseTime;

							//If now zero then clear any animation override clips and put visuals back in neutral position
							if (_overridePoseLerp <= 0f)
							{
								OverrideClip(_overridePoseClipA, null);
								OverrideClip(_overridePoseClipB, null);
								_animatorOverrideController.ApplyOverrides(_animatorClipOverrides);

								_visualsRoot.transform.localRotation = Quaternion.identity;
								_visualsRoot.transform.localPosition = Vector3.zero;
							}
							//Otherwise lerp positon and rotation back to neutral
							else
							{
								_visualsRoot.transform.localRotation = Quaternion.Slerp(Quaternion.identity, _visualsRoot.transform.localRotation, _overridePoseLerp);
								_visualsRoot.transform.localPosition = Vector3.Lerp(Vector3.zero, _visualsRoot.transform.localPosition, _overridePoseLerp);
							}
						}
					}

					_animator.SetLayerWeight(_animLayerIndexPose, _overridePoseLerp);
				}

				private AnimationClip GetOverrideClip(AnimationClip animationClip)
				{
					return _animatorClipOverrides.Find(x => x.Key == animationClip).Value;
				}

				private void OverrideClip(AnimationClip animationClip, AnimationClip overrideClip)
				{
					int index = _animatorClipOverrides.FindIndex(x => x.Key == animationClip);

					if (index != -1)
					{
						_animatorClipOverrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(animationClip, overrideClip);
					}
				}

				private void SetCurrentOverridePoseAnimation(AnimationClip animationClip)
				{
					if (animationClip != null)
					{
						float layerWeight = _animator.GetLayerWeight(_animLayerIndexPose);

						//If not currently playing an override pose...
						if (layerWeight <= 0f)
						{
							//Set to pose A and set the animation clip
							_animator.SetInteger(_animParamIndexPose, 0);

							if (GetOverrideClip(_overridePoseClipA) != animationClip)
							{
								OverrideClip(_overridePoseClipA, animationClip);
								_animatorOverrideController.ApplyOverrides(_animatorClipOverrides);
							}
						}
						else
						{
							//Otherwise already showing an override pose, check already showing required animation...
							int poseIndex = _animator.GetInteger(_animParamIndexPose);

							if (poseIndex == 0 && GetOverrideClip(_overridePoseClipA) != animationClip)
							{
								//...If not set the new animation to be the next pose and trigger transition
								_animator.SetInteger(_animParamIndexPose, 1);
								OverrideClip(_overridePoseClipB, animationClip);
								_animatorOverrideController.ApplyOverrides(_animatorClipOverrides);
							}
							else if (poseIndex == 1 && GetOverrideClip(_overridePoseClipB) != animationClip)
							{
								//...If not set the new animation to be the next pose and trigger transition
								_animator.SetInteger(_animParamIndexPose, 0);
								OverrideClip(_overridePoseClipA, animationClip);
								_animatorOverrideController.ApplyOverrides(_animatorClipOverrides);
							}
						}
					}
				}
				#endregion
			}
		}
	}
}