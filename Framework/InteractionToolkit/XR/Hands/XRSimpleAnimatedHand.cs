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
				/// The node this hand represents (should either be XRNode.LeftHand or XRNode.RightHand)
				/// </summary>
				public XRNode _handNode = XRNode.RightHand;

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
				/// Input Action used to drive grab animations.
				/// </summary>
				public InputActionProperty _grabAmountAction;

				/// <summary>
				/// Optional Input Action used to drive fingers clapsed or not
				/// </summary>
				public InputActionProperty _grabTouchAction;

				/// <summary>
				/// Optional Input Action used to drive thumb up/down animations.
				/// </summary>
				public InputActionProperty _thumbTouchAction;

				/// <summary>
				/// Optional Input Action used to drive index finger up/down animations.
				/// </summary>
				public InputActionProperty _indexFingerTouchAction;
				
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

				/// <summary>
				/// The time taken to blend a binary hand state value (eg index finger touching) from zero to one
				/// </summary>
				public float _buttonToAxisTime = 0.25f;
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

				private float _pointBlend;
				private float _thumbsUpBlend;
				#endregion


				#region Unity Messages
				private void Awake()
				{
				   
				}

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
				public override XRNode XRNode
				{
					get
					{
						return _handNode;
					}
				}

				public override void ApplyOverridePose(XRHandPose handPose)
				{
					if (_overridePose != handPose)
					{
						_overridePose = handPose;
						_overridePoseLerp = 0f;
					}
				}

				public override void ClearOverridePose(XRHandPose handPose)
				{
					if (_overridePose == handPose)
					{
						_overridePose = null;
					}
				}

				public override bool IsEnteringOverridePose()
				{
					return _overridePose != null && _overridePoseLerp < 1f;
				}

				public override bool IsReturningFromOverridePose()
				{
					return _overridePose == null && _overridePoseLerp > 0f;
				}
				#endregion

				#region Private Functions
				private float InputValueRateChange(bool isDown, float value)
				{
					float rateDelta = Time.deltaTime / _buttonToAxisTime;
					float sign = isDown ? 1.0f : -1.0f;
					return Mathf.Clamp01(value + rateDelta * sign);
				}

				private void UpdateInputAnimations()
				{
					// Clench fist amount
					if (_animParamIndexFist != -1)
					{
						float grabAmount = GetFloat(_grabAmountAction.action);
						_animator.SetFloat(_animParamIndexFist, grabAmount);
					}

					// Index finger point amount
					bool isPointing = !IsPressed(_indexFingerTouchAction.action);
					_pointBlend = InputValueRateChange(isPointing, _pointBlend);

					if (_animParamIndexPoint != -1)
					{
						_animator.SetFloat(_animParamIndexPoint, _pointBlend);
					}
				}

				private void ApplyOverridePose()
				{
					//Transiton to / from override pose
					if (_overridePose != null)
					{
						//Set the override pose animation (if any)
						SetCurrentOverridePoseAnimation(_overridePose._animation);

						//Update pose lerp
						if (_overridePoseLerp < 1f)
						{
							_overridePoseLerp += Time.deltaTime / _transitionToOverridePoseTime;
						}

						//Apply rotation
						if (_overridePose._hasRotation)
						{
							if (_overridePoseLerp < 1f)
							{
								_visualsRoot.transform.rotation = Quaternion.Slerp(_visualsRoot.transform.rotation, _overridePose._worldRotation, _overridePoseLerp);
							}
							else
							{
								_visualsRoot.transform.rotation = _overridePose._worldRotation;
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
						if (_overridePose._hasPosition)
						{
							if (_overridePoseLerp < 1f)
							{
								_visualsRoot.transform.position = Vector3.Lerp(_visualsRoot.transform.position, _overridePose._worldPosition, _overridePoseLerp);
							}
							else
							{
								_visualsRoot.transform.position = _overridePose._worldPosition;
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

				private bool IsPressed(InputAction action)
				{
					if (action == null)
						return false;

#if INPUT_SYSTEM_1_1_OR_NEWER || INPUT_SYSTEM_1_1_PREVIEW // 1.1.0-preview.2 or newer, including pre-release
						return action.phase == InputActionPhase.Performed;
#else
					if (action.activeControl is ButtonControl buttonControl)
						return buttonControl.isPressed;

					if (action.activeControl is AxisControl)
						return action.ReadValue<float>() >= m_ButtonPressPoint;

					return action.triggered || action.phase == InputActionPhase.Performed;
#endif
				}

				private static float GetFloat(InputAction action)
				{
					if (action != null)
					{
						return action.ReadValue<float>();
					}

					return 0f;
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