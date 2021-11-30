using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// <see cref="UnityEvent"/> that is invoked when Slider values changes.
		/// </summary>
		[Serializable]
		public sealed class SliderChangeEvent : UnityEvent<SliderChangeEventArgs>
		{
		}

		/// <summary>
		/// Event data associated with the event when an Interactor ends selecting an Interactable.
		/// </summary>
		public class SliderChangeEventArgs : BaseInteractionEventArgs
		{
			/// <summary>
			/// Normalised current value of slider (zero to one).
			/// </summary>
			public float Value { get; set; }
		}

		public class XRSimpleSliderConstraint : XRInteractableConstraint
		{
			#region Public Data
			public enum SlideAxis
			{
				X,
				Y,
				Z
			}
			public SlideAxis _sliderAxis = SlideAxis.X;
			public float _sliderSize = 1f;
				
			public SliderChangeEvent _sliderMovedEvent = new SliderChangeEvent();

			public float NormalisedPosition
			{
				get
				{
					float sliderspacePos = GetSliderSpacePos();
					return Mathf.Clamp01(sliderspacePos / _sliderSize);
				}
				set
				{
					SetSliderSpacePosInstantaneous(Mathf.Clamp01(value) * _sliderSize);
				}
			}
			#endregion

			#region XRInteractableConstraint
			public override void ConstrainTransform(ref Vector3 position, ref Quaternion rotation)
			{
				Vector3 sliderspacePos = ToSliderSpacePos(position);
				Vector3 localPos = this.transform.localPosition;

				switch (_sliderAxis)
				{
					case SlideAxis.X:
						{
							sliderspacePos.x = Mathf.Clamp(sliderspacePos.x, 0f, _sliderSize);
							sliderspacePos.y = localPos.y;
							sliderspacePos.z = localPos.z;
						}
						break;
					case SlideAxis.Y:
						{
							sliderspacePos.x = localPos.x;
							sliderspacePos.y = Mathf.Clamp(sliderspacePos.y, 0f, _sliderSize);
							sliderspacePos.z = localPos.z;
						}
						break;
					case SlideAxis.Z:
						{
							sliderspacePos.x = localPos.x;
							sliderspacePos.y = localPos.y;
							sliderspacePos.z = Mathf.Clamp(sliderspacePos.z, 0f, _sliderSize);
						}
						break;
				}

				position = FromSliderSpacePos(sliderspacePos);
				rotation = Quaternion.identity;
			}

			public override void ConstrainPhysics(Rigidbody rigidbody)
			{
				Vector3 localVelocity = ToSliderSpaceVector(rigidbody.velocity);

				switch (_sliderAxis)
				{
					case SlideAxis.X:
						{
							localVelocity.y = 0f;
							localVelocity.z = 0f;
						}
						break;
					case SlideAxis.Y:
						{
							localVelocity.x = 0f;
							localVelocity.z = 0f;
						}
						break;
					case SlideAxis.Z:
						{
							localVelocity.x = 0f;
							localVelocity.y = 0f;
						}
						break;
				}

				rigidbody.velocity = FromSliderSpaceVector(localVelocity);
			}
			#endregion

			#region Protected Functions
			protected virtual void SetSliderSpacePosInstantaneous(float sliderspacePos)
			{
				Vector3 localSpacePos = this.transform.localPosition;
				float previousSliderValue;

				switch (_sliderAxis)
				{
					case SlideAxis.X:
						{
							previousSliderValue = localSpacePos.x;
							localSpacePos.x = sliderspacePos;
						}
						break;
					case SlideAxis.Y:
						{
							previousSliderValue = localSpacePos.y;
							localSpacePos.y = sliderspacePos;
						}
						break;
					case SlideAxis.Z:
					default:
						{
							previousSliderValue = localSpacePos.z;
							localSpacePos.z = sliderspacePos;
						}
						break;
				}

				this.transform.localPosition = localSpacePos;

				if (Mathf.Approximately(previousSliderValue, sliderspacePos))
				{
					SliderChangeEventArgs eventArgs = new SliderChangeEventArgs()
					{
						Value = Mathf.Clamp01(sliderspacePos / _sliderSize)
					};

					_sliderMovedEvent?.Invoke(eventArgs);
				}
			}

			protected virtual void SetSliderSpacePosKinematic(Rigidbody rigidbody, float sliderspacePos)
			{
				Vector3 localSpacePos = this.transform.localPosition;
				float previousSliderValue;

				switch (_sliderAxis)
				{
					case SlideAxis.X:
						{
							previousSliderValue = localSpacePos.x;
							localSpacePos.x = sliderspacePos;
						}
						break;
					case SlideAxis.Y:
						{
							previousSliderValue = localSpacePos.y;
							localSpacePos.y = sliderspacePos;
						}
						break;
					case SlideAxis.Z:
					default:
						{
							previousSliderValue = localSpacePos.z;
							localSpacePos.z = sliderspacePos;
						}
						break;
				}

				Vector3 worldPos = FromSliderSpacePos(localSpacePos);

				var position = worldPos - rigidbody.worldCenterOfMass + rigidbody.position;
				rigidbody.velocity = Vector3.zero;
				rigidbody.MovePosition(position);

				if (Mathf.Approximately(previousSliderValue, sliderspacePos))
				{
					SliderChangeEventArgs eventArgs = new SliderChangeEventArgs()
					{
						Value = Mathf.Clamp01(sliderspacePos / _sliderSize)
					};

					_sliderMovedEvent?.Invoke(eventArgs);
				}
			}

			protected float GetSliderSpacePos()
			{
				Vector3 sliderspacePos = this.transform.localPosition;
				float sliderPos = 0f;

				switch (_sliderAxis)
				{
					case SlideAxis.X:
						{
							sliderPos = sliderspacePos.x;
						}
						break;
					case SlideAxis.Y:
						{
							sliderPos = sliderspacePos.y;
						}
						break;
					case SlideAxis.Z:
						{
							sliderPos = sliderspacePos.z;
						}
						break;
				}

				return sliderPos;
			}

			protected Vector3 ToSliderSpacePos(Vector3 worldspacePos)
			{
				if (this.transform.parent != null)
					return this.transform.parent.InverseTransformPoint(worldspacePos);

				return worldspacePos;
			}

			protected Vector3 FromSliderSpacePos(Vector3 sliderspacePos)
			{
				if (this.transform.parent != null)
					return this.transform.parent.TransformPoint(sliderspacePos);

				return sliderspacePos;
			}

			protected Vector3 ToSliderSpaceVector(Vector3 worldspaceVec)
			{
				if (this.transform.parent != null)
					return this.transform.parent.InverseTransformVector(worldspaceVec);

				return worldspaceVec;
			}

			protected Vector3 FromSliderSpaceVector(Vector3 sliderspaceVec)
			{
				if (this.transform.parent != null)
					return this.transform.parent.TransformVector(sliderspaceVec);

				return sliderspaceVec;
			}
			#endregion
		}
	}
}