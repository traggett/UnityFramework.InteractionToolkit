using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// Component that allows constraining interactible movement
		/// </summary>
		[DisallowMultipleComponent]
		[RequireComponent(typeof(XRAdvancedGrabInteractable))]
		public abstract class XRInteractableConstraint : MonoBehaviour
		{
			#region Public Interface
			public XRAdvancedGrabInteractable Interactable
			{
				get;
				private set;
			}
			#endregion

			#region Unity Messages
			protected virtual void Awake()
			{
				Interactable = GetComponent<XRAdvancedGrabInteractable>();
			}

			protected virtual void OnDrawGizmos()
			{
				Gizmos.color = new Color(0.75f, 0.75f, 0.75f);
				Gizmos.matrix = GetGizmoDrawMatrix();

				DebugDraw(false);
			}

			protected virtual void OnDrawGizmosSelected()
			{
				Gizmos.color = Color.green;
				Gizmos.matrix = GetGizmoDrawMatrix();
				
				DebugDraw(true);
			}

			private Matrix4x4 GetGizmoDrawMatrix()
			{
				Matrix4x4 matrix = this.transform.localToWorldMatrix;
				matrix = new Matrix4x4(matrix.GetColumn(0).normalized, matrix.GetColumn(1).normalized, matrix.GetColumn(2).normalized, matrix.GetColumn(3).normalized);
				return matrix;
			}
			#endregion

			#region Virtual Interface
			public virtual void ProcessConstraint(XRInteractionUpdateOrder.UpdatePhase updatePhase)
			{
				switch (updatePhase)
				{
					case XRInteractionUpdateOrder.UpdatePhase.Fixed:
						{
							if (!Interactable.isSelected)
							{
								Constrain();
							}
						}
						break;
				}
			}
			
			public abstract void ConstrainTargetTransform(ref Vector3 targetPosition, ref Quaternion targetRotation);

			public abstract void Constrain();

			public virtual void DebugDraw(bool selected)
			{

			}
			#endregion
		}
	}
}