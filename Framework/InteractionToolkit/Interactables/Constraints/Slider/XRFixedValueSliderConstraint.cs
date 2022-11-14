using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		#region Events
		/// <summary>
		/// <see cref="UnityEvent"/> that is invoked when Slider values changes.
		/// </summary>
		[Serializable]
		public sealed class FixedValueSliderChangeEvent : UnityEvent<FixedValueSliderChangeEventArgs>
		{
		}

		public class FixedValueSliderChangeEventArgs : BaseInteractionEventArgs
		{
			/// <summary>
			/// The current index of the slider (ie the index in the _allowedSliderPositions array)
			/// </summary>
			public int Index { get; set; }
		}
		#endregion

		public class XRFixedValueSliderConstraint : XRSimpleSliderConstraint
		{
			#region Public Data
			public float[] _allowedSliderPositions = new float[] { 0f, 0.5f, 1f };
			public AnimationCurve _movementCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.3f, 0.2f, 1.74f, 1.74f), new Keyframe(0.6f, 0.8f, 1.74f, 1.74f), new Keyframe(1f, 1f));
			public float _snapToPositionTime = 0.1f;

			public FixedValueSliderChangeEvent _fixedValueSliderChanged = new FixedValueSliderChangeEvent();

			public int SliderIndex
			{
				get
				{
					return GetNearestFixedSliderPos(NormalisedPosition);
				}
				set
				{
					if (0 <= value && value < _allowedSliderPositions.Length)
					{
						float normalisedPos = _allowedSliderPositions[value];
						SetSliderSpacePosInstantaneous(normalisedPos * _sliderSize);
					}
				}
			}
			#endregion

			#region Private Data
			private float _moveToFixedPosVelocity;
			private int _previousSliderIndex = -1;
			#endregion

			#region XRInteractableConstraint
			public override void ProcessConstraint(XRInteractionUpdateOrder.UpdatePhase updatePhase)
			{
				switch (updatePhase)
				{
					case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
					case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
						{
							if (!Interactable.isSelected && Interactable.movementType == XRBaseInteractable.MovementType.Instantaneous)
							{
								//Move towards nearest allowed position
								SetSliderSpacePosInstantaneous(MoveSliderTowardsNearestAllowedPosition());
							}
						}
						break;
					case XRInteractionUpdateOrder.UpdatePhase.Fixed:
						{
							if (!Interactable.isSelected && Interactable.movementType == XRBaseInteractable.MovementType.Kinematic)
							{
								//Move towards nearest allowed position
								SetSliderSpacePosKinematic(Interactable.Rigidbody, MoveSliderTowardsNearestAllowedPosition());
							}
							else if (!Interactable.isSelected && Interactable.movementType == XRBaseInteractable.MovementType.VelocityTracking)
							{
								//TO DO!! Velocity tracking based movement
							}

							Constrain();
						}
						break;
					case XRInteractionUpdateOrder.UpdatePhase.Late:
						{
							CheckForSliderChange();
						}
						break;
				}
			}

			public override void ConstrainTargetTransform(ref Vector3 position, ref Quaternion rotation)
			{
				Vector3 sliderspacePos = WorldToConstraintSpacePos(position);
				Vector3 localPos = this.transform.localPosition;

				switch (_sliderAxis)
				{
					case SlideAxis.X:
						{
							sliderspacePos.x = ConstrainSliderPosition(sliderspacePos.x);
							sliderspacePos.y = localPos.y;
							sliderspacePos.z = localPos.z;
						}
						break;
					case SlideAxis.Y:
						{
							sliderspacePos.x = localPos.x;
							sliderspacePos.y = ConstrainSliderPosition(sliderspacePos.y);
							sliderspacePos.z = localPos.z;
						}
						break;
					case SlideAxis.Z:
						{
							sliderspacePos.x = localPos.x;
							sliderspacePos.y = localPos.y;
							sliderspacePos.z = ConstrainSliderPosition(sliderspacePos.z);
						}
						break;
				}

				position = ConstraintToWorldSpacePos(sliderspacePos);
				rotation = Quaternion.identity;
			}
			
			public override void DebugDraw(bool selected)
			{
				Vector3 sliderAxis;
				Vector3 notchAxis;

				switch (_sliderAxis)
				{
					case SlideAxis.X:
						sliderAxis = Vector3.right;
						notchAxis = Vector3.forward;
						break;
					case SlideAxis.Y:
						sliderAxis = Vector3.up;
						notchAxis = Vector3.right;
						break;
					case SlideAxis.Z:
					default:
						sliderAxis = Vector3.forward;
						notchAxis = Vector3.right;
						break;
				}

				Gizmos.DrawLine(Vector3.zero, sliderAxis * _sliderSize);

				float notchSize = _sliderSize * 0.1f;

				for (int i = 0; i < _allowedSliderPositions.Length; i++)
				{
					Vector3 pos = sliderAxis * _sliderSize * _allowedSliderPositions[i];

					Gizmos.DrawLine(pos - notchAxis * notchSize, pos + notchAxis * notchSize);
				}
			}

			protected override void CheckForSliderChange()
			{
				base.CheckForSliderChange();

				int sliderIndex = SliderIndex;

				if (sliderIndex != _previousSliderIndex)
				{
					_previousSliderIndex = sliderIndex;

					FixedValueSliderChangeEventArgs eventArgs = new FixedValueSliderChangeEventArgs()
					{
						Index = sliderIndex
					};

					_fixedValueSliderChanged?.Invoke(eventArgs);
				}
			}
			#endregion

			#region Private Functions
			private float MoveSliderTowardsNearestAllowedPosition()
			{
				float sliderPos = GetSliderSpacePos();
				float normalisedPos = Mathf.Clamp01(sliderPos / _sliderSize);
				int nearestFixedPosIndex = GetNearestFixedSliderPos(normalisedPos);
				float idealValue = _allowedSliderPositions[nearestFixedPosIndex] * _sliderSize;

				return Mathf.SmoothDamp(sliderPos, idealValue, ref _moveToFixedPosVelocity, _snapToPositionTime);
			}

			private float ConstrainSliderPosition(float sliderspacePos)
			{
				float normalisedPos = Mathf.Clamp01(sliderspacePos / _sliderSize);
					
				//Before first position
				if (normalisedPos <= _allowedSliderPositions[0])
				{
					normalisedPos = _allowedSliderPositions[0];
				}
				//After last position
				else if (normalisedPos >= _allowedSliderPositions[_allowedSliderPositions.Length -1])
				{
					normalisedPos = _allowedSliderPositions[_allowedSliderPositions.Length - 1];
				}
				//Between two positions
				else
				{
					for (int i = 0; i < _allowedSliderPositions.Length - 1; i++)
					{
						if (normalisedPos < _allowedSliderPositions[i + 1])
						{
							float dist = _allowedSliderPositions[i + 1] - _allowedSliderPositions[i];
							float frac = (normalisedPos - _allowedSliderPositions[i]) / dist;

							normalisedPos = _allowedSliderPositions[i] + (_movementCurve.Evaluate(frac) * dist);
							break;
						}
					}
				}

				return normalisedPos * _sliderSize;
			}
			private int GetNearestFixedSliderPos(float normalisedPosition)
			{
				//Before first position
				if (normalisedPosition <= _allowedSliderPositions[0])
				{
					return 0;
				}
				//After last position
				else if (normalisedPosition >= _allowedSliderPositions[_allowedSliderPositions.Length - 1])
				{
					return _allowedSliderPositions.Length - 1;
				}
				//Between two positions
				else
				{
					int nearestPoint = -1;
					float nearestDist = 0f;

					for (int i = 0; i < _allowedSliderPositions.Length; i++)
					{
						float toPoint = Mathf.Abs(_allowedSliderPositions[i] - normalisedPosition);

						if (nearestPoint == -1 || toPoint < nearestDist)
						{
							nearestPoint = i;
							nearestDist = toPoint;
						}
					}

					return nearestPoint;
				}
			}
			#endregion
		}
	}
}