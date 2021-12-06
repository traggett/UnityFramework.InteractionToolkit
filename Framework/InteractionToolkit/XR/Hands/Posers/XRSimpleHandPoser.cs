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
				/// <summary>
				/// Should the interacting Hand's position be clamped to this gameobjects world position?
				/// </summary>
				public bool _constrainPosition;
				/// <summary>
				/// Should the interacting Hand's rotation be clamped to this gameobjects world rotation?
				/// </summary>
				public bool _constrainRotation;
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