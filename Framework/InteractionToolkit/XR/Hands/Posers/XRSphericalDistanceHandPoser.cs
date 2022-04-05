using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public class XRSphericalDistanceHandPoser : XRHandPoser
			{
				#region Public Data
				/// <summary>
				/// Should the interacting Hand's position be clamped to this gameobjects world position?
				/// </summary>
				public bool _constrainPosition;
				/// <summary>
				/// Should the interacting Hand's rotation be clamped to this gameobjects world rotation?
				/// </summary>
				public bool _constrainRotation;
				/// <summary>
				/// Should the interacting Hand be constrained to the surface of the sphere? If not it will be constrained to be inside it instead.
				/// </summary
				public bool _constrainToSurface;
				/// <summary>
				/// The radius of the sphere.
				/// </summary
				public float _radius;
				/// <summary>
				/// Optional animation that will play on the interacting Hand if it's the left hand.
				/// </summary>
				public AnimationClip _leftHandPoseAnimation;
				/// <summary>
				/// Optional animation that will play on the interacting Hand if it's the right hand.
				/// </summary>
				public AnimationClip _rightHandPoseAnimation;
				#endregion

				#region Private Data
				private readonly XRHandPose _pose = new XRHandPose();
				#endregion

				#region XRHandPoser
				public override XRHandPose GetPose(XRHandGrabInteractor interactor, XRBaseInteractable interactable)
				{
					_pose._hasPosition = _constrainPosition;

					if (_constrainPosition)
					{
						Vector3 toCentre = interactor.attachTransform.position - this.transform.position;

						if (_constrainToSurface)
						{
							//Find closed point of surface of sphere
							_pose._worldPosition = this.transform.position + (toCentre.normalized * _radius);
						}
						else
						{
							if (toCentre.sqrMagnitude > _radius * _radius)
							{
								_pose._worldPosition = this.transform.position + (toCentre.normalized * _radius);
							}
						}
					}

					_pose._hasRotation = _constrainRotation;

					if (_constrainRotation)
					{
						//Rotation faces towards centre?? Plus default rotation??
						_pose._worldRotation = this.transform.rotation;
					}

					if (interactor.HandVisuals.XRNode == UnityEngine.XR.XRNode.LeftHand)
					{
						_pose._animation = _leftHandPoseAnimation;
					}
					else
					{
						_pose._animation = _rightHandPoseAnimation;
					}

					return _pose;
				}
				#endregion
			}
		}
	}
}