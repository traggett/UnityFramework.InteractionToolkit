using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
    namespace Interaction.Toolkit
    {
		namespace XR
		{
			[Flags]
			public enum HandPoseFlags
			{
				None = 0,
				Grab = 1,
				Hover = 2,
				Both = Grab | Hover
			}

			/// <summary>
			/// Component that generates a Hand Pose for a IXRHandInteractor.
			/// </summary>
			public abstract class XRHandPoser : MonoBehaviour
			{
				#region Public Data
				public HandFlags CompatibleHands
				{
					get
					{
						return _compatibleHands;
					}
				}

				public HandPoseFlags PoseFlags
				{
					get
					{
						return _poseFlags;
					}
				}
				#endregion

				#region Private Data
				[SerializeField]
				private HandFlags _compatibleHands;

				[SerializeField]
				private HandPoseFlags _poseFlags;
				#endregion

				#region Virtual Interface
				public abstract XRHandPose GeneratePose(IXRHandInteractor interactor, IXRInteractable interactable);
				#endregion
			}
		}
	}
}