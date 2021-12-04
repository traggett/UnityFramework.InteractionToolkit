using UnityEngine;
using UnityEngine.InputSystem;

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
				/// Animation state name for override pose animation.
				/// </summary>
				public const string ANIM_NAME_OVERRIDE_POSE = "Pose";
				/// <summary>
				/// Animation layer name for index finger pointing
				/// </summary>
				public const string ANIM_LAYER_NAME_POINT = "Point Layer";
				/// <summary>
				/// Animation layer name for thumb pointing
				/// </summary>
				public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
				/// <summary>
				/// Animation param name for a float used for clenching the fist.
				/// </summary>
				public const string ANIM_PARAM_NAME_FLEX = "Flex";
				/// <summary>
				/// Animation param name for a bool used for applying an override pose.
				/// </summary>
				public const string ANIM_PARAM_NAME_POSE = "Pose";
				
				public const float INPUT_RATE_CHANGE = 20.0f;

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
				public InputActionProperty _grabAction;

				/// <summary>
				/// Optional Input Action used to drive thumb up/down animations.
				/// </summary>
				public InputActionProperty _thumbTouchAction;

				/// <summary>
				/// Optional Input Action used to drive index finger up/down animations.
				/// </summary>
				public InputActionProperty _indexFingerTouchAction;
				
				/// <summary>
				/// The time taken to lerp to an override pose.
				/// </summary>
				public float _transitionToOverridePoseTime = 0.2f;
				/// <summary>
				/// The time taken to lerp from an override pose back to normal.
				/// </summary>
				public float _transitionFromOverridePoseTime = 0.3f;
				#endregion

				#region Private Data
				private int _animLayerIndexThumb = -1;
				private int _animLayerIndexPoint = -1;
				private int _animParamIndexClenchFist = -1;
				private int _animParamIndexPose = -1;

				private AnimatorOverrideController _animatorOverrideController;

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
					_animLayerIndexPoint = _animator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
					_animLayerIndexThumb = _animator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
					_animParamIndexClenchFist = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
					_animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);

					_animatorOverrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
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

				public override bool IsReturningFromOverridePose()
				{
					return _overridePose == null && _overridePoseLerp > 0f;
				}
				#endregion

				#region Private Functions
				private float InputValueRateChange(bool isDown, float value)
				{
					float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
					float sign = isDown ? 1.0f : -1.0f;
					return Mathf.Clamp01(value + rateDelta * sign);
				}

				private void UpdateInputAnimations()
				{
					// Clench fist amount
					float grabAmount = GetFloat(_grabAction.action);
					_animator.SetFloat(_animParamIndexClenchFist, grabAmount);


					//TO DO! optional grab is touching????


					// Index finger point amount
					bool isPointing = !IsPressed(_indexFingerTouchAction.action);
					_pointBlend = InputValueRateChange(isPointing, _pointBlend);

					if (_animLayerIndexPoint != -1)
					{
						// Multiply by grab amount so don't point whilst clenching fist.
						float point = _pointBlend * grabAmount;
						_animator.SetLayerWeight(_animLayerIndexPoint, point);
					}

					// Thumbs up amount
					bool isGivingThumbsUp = !IsPressed(_thumbTouchAction.action);
					_thumbsUpBlend = InputValueRateChange(isGivingThumbsUp, _thumbsUpBlend);

					if (_animLayerIndexThumb != -1)
					{
						// Multiply by grab amount so don't thumb up whilst clenching fist.
						float thumbsUp = _pointBlend * grabAmount;
						_animator.SetLayerWeight(_animLayerIndexThumb, thumbsUp);
					}
				}

				private void ApplyOverridePose()
				{
					//Transiton to / from override pose
					if (_overridePose != null)
					{
						//Apply animation
						if (_overridePose._animation != null)
						{
							_animatorOverrideController[ANIM_NAME_OVERRIDE_POSE] = _overridePose._animation;
							_animator.SetBool(_animParamIndexPose, true);
						}
						else
						{
							_animator.SetBool(_animParamIndexPose, false);
						}

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
					else
					{
						//Cancel override pose animation
						_animator.SetBool(_animParamIndexPose, false);

						//Update pose lerp
						if (_overridePoseLerp > 0f)
						{
							_overridePoseLerp -= Time.deltaTime / _transitionFromOverridePoseTime;
						}

						//Lerp positon and rotation back to neutral
						if (_overridePoseLerp > 0f)
						{
							_visualsRoot.transform.localPosition = Vector3.Lerp(Vector3.zero, _visualsRoot.transform.localPosition, _overridePoseLerp);
							_visualsRoot.transform.localRotation = Quaternion.Slerp(Quaternion.identity, _visualsRoot.transform.localRotation, _overridePoseLerp);
						}
					}
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
				#endregion
			}
		}
	}
}