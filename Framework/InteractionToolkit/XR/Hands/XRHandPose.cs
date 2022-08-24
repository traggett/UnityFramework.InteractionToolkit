using UnityEngine;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public struct XRHandPose
			{
				#region Public data
				public bool _hasPosition;
				public Vector3 _worldPosition;

				public bool _hasRotation;
				public Quaternion _worldRotation;

				public AnimationClip _animation;
				#endregion
			}
		}
	}
}