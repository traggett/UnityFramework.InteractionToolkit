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
				/// <summary>
				/// The <see cref="Transform"/> that is used as the attach point for Interactables.
				/// </summary>
				public Transform _attachTransform;

				// <summary>
				/// Used to define which hand these visuals are relate to (left or right).
				/// </summary>
				public abstract XRNode XRNode
				{
					get;
				}

				#region Virtual Interface
				public abstract void ApplyOverridePose(XRHandPose handPose);

				public abstract void ClearOverridePose(XRHandPose handPose);

				public abstract bool IsEnteringOverridePose();

				public abstract bool IsReturningFromOverridePose();
				#endregion

				#region Protected Functions
				protected Quaternion GetVisualsWorldRotation(XRHandPose pose)
				{
					//The pose rotation is where the attach transform should be so need to take out the offset of this attach transform to get a target for the visuals
					Quaternion attachRotationOffset = Quaternion.Inverse(this.transform.rotation) * _attachTransform.rotation;
					return pose._worldRotation * Quaternion.Inverse(attachRotationOffset);
				}

				protected Vector3 GetVisualsWorldPosition(XRHandPose pose)
				{
					//The pose position is where the attach transform should be so need to take out the offset of this attach transform to get a target for the visuals
					Vector3 attachPositionOffset = _attachTransform.position - this.transform.position;
					return pose._worldPosition - attachPositionOffset;
				}
				#endregion
			}
		}
	}
}