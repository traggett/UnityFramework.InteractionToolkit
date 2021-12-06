using UnityEngine;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			public class XRHandPose
			{
				public bool _hasPosition;
				public Vector3 _worldPosition;
				public bool _hasRotation;
				public Quaternion _worldRotation;
				public AnimationClip _animation;
			}
		}
	}
}