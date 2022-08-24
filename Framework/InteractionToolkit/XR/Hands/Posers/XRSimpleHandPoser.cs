using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
				/// Optional animation that will play on the interacting Hand
				/// </summary>
				public AnimationClip _handPoseAnimation;
				#endregion

				#region XRHandPoser
				public override XRHandPose GeneratePose(IXRHandInteractor interactor, IXRInteractable interactable)
				{
					XRHandPose pose = new XRHandPose();

					pose._hasRotation = _constrainRotation;

					if (_constrainRotation)
					{
						pose._worldRotation = this.transform.rotation;
					}

					pose._hasPosition = _constrainPosition;

					if (_constrainPosition)
					{
						pose._worldPosition = this.transform.position;
					}

					pose._animation = _handPoseAnimation;

					return pose;
				}
				#endregion
			}
		}
	}
}