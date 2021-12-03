using UnityEngine;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public class XRSphericalDistanceHandPoser : XRHandPoser
			{
				#region Public Data
				public Transform _attachTransform;
				public bool _constrainPosition;
				public bool _constrainRotation;
				public bool _constrainToSurface;
				public float _radius;
				public AnimationClip _animationPose;
				#endregion

				#region Private Data
				private XRHandPose _pose = new XRHandPose();
				#endregion

				#region XRHandPoser
				public override XRHandPose GetPose(XRHandInteractor interactor)
				{
					Transform attachTransform = interactor.attachTransform;

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

						//Apply interactor attach transform to position
						_pose._worldPosition -= (attachTransform.rotation * attachTransform.localPosition);
					}

					_pose._hasRotation = _constrainRotation;

					if (_constrainRotation)
					{
						//Rotation faces towards centre?? Plus default rotation??
						_pose._worldRotation = this.transform.rotation;

						//Apply interactor attach transform to rotation
						_pose._worldRotation *= attachTransform.localRotation;
					}

					_pose._pose = _animationPose;

					return _pose;
				}
				#endregion
			}
		}
	}
}