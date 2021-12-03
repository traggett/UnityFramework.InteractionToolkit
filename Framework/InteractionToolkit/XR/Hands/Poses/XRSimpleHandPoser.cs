using UnityEngine;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public class XRSimpleHandPoser : XRHandPoser
			{
				#region Public Data
				public bool _constrainPosition;
				public bool _constrainRotation;
				public AnimationClip _animationPose;
				#endregion

				#region Private Data
				private XRHandPose _pose = new XRHandPose();
				#endregion

				#region XRHandPoser
				public override XRHandPose GetPose(XRHandInteractor interactor)
				{
					Transform attachTransform = interactor.attachTransform;

					_pose._hasRotation = _constrainRotation;

					if (_constrainRotation)
					{
						_pose._worldRotation = this.transform.rotation;

						//Apply interactor attach transform to rotation
						_pose._worldRotation *= attachTransform.localRotation;
					}

					_pose._hasPosition = _constrainPosition;

					if (_constrainPosition)
					{
						_pose._worldPosition = this.transform.position;

						//Apply interactor attach transform to position
						_pose._worldPosition -= (attachTransform.rotation * attachTransform.localPosition);
					}

					_pose._pose = _animationPose;

					return _pose;
				}
				#endregion
			}
		}
	}
}