using System;
using UnityEngine;

namespace Framework
{
    namespace Interaction.Toolkit
    {
		namespace XR
		{
			[Flags]
			public enum HandInteractionFlags
			{
				None = 0,
				Grab = 1,
				Hover = 2,
			}

			/// <summary>
			/// Component that represents a hand pose.
			/// </summary>
			[ExecuteInEditMode]			
			public class XRHandPose : MonoBehaviour
			{
				#region Public Data
				public HandFlags CompatibleHands
				{
					get
					{
						return _compatibleHands;
					}
				}

				public HandInteractionFlags InteractionFlags
				{
					get
					{
						return _interactionFlags;
					}
				}

				public bool HasPosition
				{
					get
					{
						return _hasPosition;
					}
				}

				public bool HasRotation
				{
					get
					{
						return _hasRotation;
					}
				}

				public AnimationClip PoseAnimation
				{
					get
					{
						return _animation;
					}
				}

				public Vector3 WorldPosition
				{
					get
					{
						return this.transform.position;
					}
				}

				public Quaternion WorldRotation
				{
					get
					{
						return this.transform.rotation;
					}
				}
				#endregion

				#region Private Data
				[SerializeField]
				private HandFlags _compatibleHands;

				[SerializeField]
				private HandInteractionFlags _interactionFlags;

				[SerializeField]
				private bool _hasPosition;

				[SerializeField]
				private bool _hasRotation;

				[SerializeField]
				private AnimationClip _animation;

				[Header("Previewing")]
				[SerializeField]
				private bool _previewPose;

				[SerializeField]
				private GameObject _previewGameObject;

#if UNITY_EDITOR
				private XRHandVisuals _previewVisuals;
				private Animator _previewAnimator;
#endif			
				#endregion

				#region Virtual Interface
				public virtual void PreparePose(IXRHandInteractor interactor)
				{

				}
				#endregion

				#region Unity Messages
#if UNITY_EDITOR
				private void Update()
				{
					if (_previewPose && !Application.isPlaying && UnityEditor.Selection.Contains(this.gameObject.GetInstanceID()))
					{
						if (_previewGameObject != null && _previewVisuals == null)
						{
							CreatePreviewVisuals();
						}

						if (_previewVisuals != null)
						{
							UpdatePreviewVisuals();
						}
					}
					else
					{
						DestroyPreviewVisuals();
					}
				}

				private void OnDestroy()
				{
					DestroyPreviewVisuals();
				}
#endif
				#endregion


				#region Private Functions
#if UNITY_EDITOR
				private void CreatePreviewVisuals()
				{
					if (_previewGameObject != null)
					{
						GameObject gameObject = Instantiate(_previewGameObject, this.transform);

						if (gameObject.TryGetComponent(out _previewVisuals))
						{
							gameObject.name = "PREVIEW HAND POSE VISUALS";
							gameObject.hideFlags = HideFlags.HideAndDontSave;
							_previewAnimator = _previewVisuals.GetComponentInChildren<Animator>();
						}
						else
						{
							DestroyImmediate(gameObject);
						}
					}
				}

				private void UpdatePreviewVisuals()
				{
					//Position so attach transform is on this 
					//Find offset from interactors attach rotation and local rotation
					Quaternion attachRotation = Quaternion.Inverse(_previewVisuals._attachTransform.rotation) * _previewVisuals.transform.rotation;

					//Find position offset from interactors attach position in interactors attach transforms local space
					Vector3 attachOffset = _previewVisuals.transform.position - _previewVisuals._attachTransform.position;

					_previewVisuals.transform.rotation = this.transform.rotation * attachRotation;
					_previewVisuals.transform.position = this.transform.position + attachOffset;

					if (_previewAnimator != null && _animation != null)
					{
						_animation.SampleAnimation(_previewAnimator.gameObject, _animation.length * 0.5f);
					}
				}

				private void DestroyPreviewVisuals()
				{
					if (_previewVisuals != null)
					{
						DestroyImmediate(_previewVisuals.gameObject);
						_previewVisuals = null;
						_previewAnimator = null;
					}
				}
#endif
				#endregion
			}
		}
	}
}