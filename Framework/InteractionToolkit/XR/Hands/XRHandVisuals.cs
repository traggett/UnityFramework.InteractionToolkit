using UnityEngine;
using UnityEngine.XR;

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
				public abstract XRNode XRNode
				{
					get;
				}

				public abstract void ApplyOverridePose(XRHandPose handPose);

				public abstract void ClearOverridePose(XRHandPose handPose);

				public abstract bool IsEnteringOverridePose();

				public abstract bool IsReturningFromOverridePose();
			}
		}
	}
}