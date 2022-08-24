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
				/// Optional animation that will play on the interacting Hand
				/// </summary>
				public AnimationClip _handPoseAnimation;
				#endregion

				#region XRHandPoser
				public override XRHandPose GeneratePose(IXRHandInteractor interactor, IXRInteractable interactable)
				{
					XRHandPose pose = new XRHandPose();

					pose._hasPosition = _constrainPosition;

					if (_constrainPosition)
					{
						Vector3 toCentre = interactor.Interactor.attachTransform.position - this.transform.position;

						if (_constrainToSurface)
						{
							//Find closed point of surface of sphere
							pose._worldPosition = this.transform.position + (toCentre.normalized * _radius);
						}
						else
						{
							if (toCentre.sqrMagnitude > _radius * _radius)
							{
								pose._worldPosition = this.transform.position + (toCentre.normalized * _radius);
							}
						}
					}

					pose._hasRotation = _constrainRotation;

					if (_constrainRotation)
					{
						//Rotation faces towards centre?? Plus default rotation??
						pose._worldRotation = this.transform.rotation;
					}

					pose._animation = _handPoseAnimation;

					return pose;
				}
				#endregion
			}
		}
	}
}