using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public class XRHandPose
			{
				public bool _hasPosition;
				public Vector3 _worldPosition;
				public bool _hasRotation;
				public Quaternion _worldRotation;
				public AnimationClip _pose;
			}

			public class XRHandVisuals : MonoBehaviour
			{
				public const string ANIM_NAME_OVERRIDE_POSE = "Pose";
				public const string ANIM_LAYER_NAME_POINT = "Point Layer";
				public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
				public const string ANIM_PARAM_NAME_FLEX = "Flex";
				public const string ANIM_PARAM_NAME_POSE = "Pose";
				public const float THRESH_COLLISION_FLEX = 0.9f;

				public const float INPUT_RATE_CHANGE = 20.0f;

				public const float COLLIDER_SCALE_MIN = 0.01f;
				public const float COLLIDER_SCALE_MAX = 1.0f;
				public const float COLLIDER_SCALE_PER_SECOND = 1.0f;

				public const float TRIGGER_DEBOUNCE_TIME = 0.05f;
				public const float THUMB_DEBOUNCE_TIME = 0.15f;

				public Transform _visualsRoot;
				public XRHandInteractor _directInteractor;
				public Animator _animator;

				public InputActionProperty _grabAction;
				public InputActionProperty _thumbTouchAction;
				public InputActionProperty _indexFingerTouchAction;

				public float _snapToConstraintsTime = 0.2f;
				public float _releaseFromConstraintsTime = 0.3f;

				private int _animLayerIndexThumb = -1;
				private int _animLayerIndexPoint = -1;
				private int _animParamIndexFlex = -1;
				private int _animParamIndexPose = -1;

				private AnimatorOverrideController _animatorOverrideController;

				private float _grabAmount = 0f;
				private bool _isPointing = false;
				private bool _isGivingThumbsUp = false;
				private float _pointBlend = 0.0f;
				private float _thumbsUpBlend = 0.0f;

				private XRHandPose _overridePose;
				private float _overridePoseLerp;

				[SerializeField]
				private float m_ButtonPressPoint = 0.5f;


				private void Awake()
				{
				   
				}

				private void Start()
				{
					// Get animator layer indices by name, for later use switching between hand visuals
					_animLayerIndexPoint = _animator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
					_animLayerIndexThumb = _animator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
					_animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
					_animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);

					_animatorOverrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
				}
				private void Update()
				{
					_grabAmount = _grabAction.action.ReadValue<float>();
					_isPointing = !IsPressed(_indexFingerTouchAction.action);
					_isGivingThumbsUp = !IsPressed(_thumbTouchAction.action);
					_pointBlend = InputValueRateChange(_isPointing, _pointBlend);
					_thumbsUpBlend = InputValueRateChange(_isGivingThumbsUp, _thumbsUpBlend);

					UpdateAnimStates();
				}

				private void LateUpdate()
				{
					if (_overridePose != null)
					{
						if (_overridePoseLerp < 1f)
						{
							_overridePoseLerp += Time.deltaTime / _snapToConstraintsTime;
						}

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

						//TO DO also apply animation
					}
					else
					{
						if (_overridePoseLerp > 0f)
						{
							_overridePoseLerp -= Time.deltaTime / _releaseFromConstraintsTime;
						}

						if (_overridePoseLerp > 0f)
						{
							_visualsRoot.transform.localPosition = Vector3.Lerp(Vector3.zero, _visualsRoot.transform.localPosition, _overridePoseLerp);
							_visualsRoot.transform.localRotation = Quaternion.Slerp(Quaternion.identity, _visualsRoot.transform.localRotation, _overridePoseLerp);
						}
					}
				}

				public void ApplyOverrideHandPose(XRHandPose handPose)
				{
					if (_overridePose != handPose)
					{
						_overridePose = handPose;
						_overridePoseLerp = 0f;
					}
				}

				public void ClearOverrideHandPose(XRHandPose handPose)
				{
					if (_overridePose == handPose)
					{
						_overridePose = null;
					}
				}

				private float InputValueRateChange(bool isDown, float value)
				{
					float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
					float sign = isDown ? 1.0f : -1.0f;
					return Mathf.Clamp01(value + rateDelta * sign);
				}

				private void UpdateAnimStates()
				{
					//TO DO!
					bool grabbing = _grabAmount > 0.5f;

					// Flex
					// blend between open hand and fully closed fist
					_animator.SetFloat(_animParamIndexFlex, _grabAmount);

					// Point
					bool canPoint = !grabbing;
					float point = canPoint ? _pointBlend : 0.0f;
					_animator.SetLayerWeight(_animLayerIndexPoint, point);

					// Thumbs up
					bool canThumbsUp = !grabbing;
					float thumbsUp = canThumbsUp ? _thumbsUpBlend : 0.0f;
					_animator.SetLayerWeight(_animLayerIndexThumb, thumbsUp);
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
			}
		}
	}
}