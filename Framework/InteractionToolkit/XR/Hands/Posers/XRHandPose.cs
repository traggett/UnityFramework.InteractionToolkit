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
			/// Component that represents a hand pose.
			/// </summary>
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

				public HandPoseFlags PoseFlags
				{
					get
					{
						return _poseFlags;
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
				private HandPoseFlags _poseFlags;

				[SerializeField]
				private bool _hasRotation;

				[SerializeField]
				private AnimationClip _animation;
				#endregion

				#region Virtual Interface
				public virtual void GeneratePose(IXRHandInteractor interactor)
				{

				}
				#endregion
			}
		}
	}
}