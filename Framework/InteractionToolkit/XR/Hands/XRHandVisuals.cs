using UnityEngine;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		namespace XR
		{
			/// <summary>
			/// Abstract class representing visuals for a single hand that can be used in interactions.
			/// It's pose (position/rotation/animation etc) can be overridden by some interactions.
			/// </summary>
			public abstract class XRHandVisuals : MonoBehaviour
			{
				public abstract void ApplyOverridePose(XRHandPose handPose);

				public abstract void ClearOverridePose(XRHandPose handPose);

				public abstract bool IsReturningFromOverridePose();
			}
		}
	}
}