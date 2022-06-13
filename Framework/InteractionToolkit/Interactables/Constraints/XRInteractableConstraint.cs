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
			#region Public Data
			public XRAdvancedGrabInteractable Interactable
			{
				get
				{
					if (_interactable == null)
					{
						_interactable = GetComponent<XRAdvancedGrabInteractable>();
					}

					return _interactable;
				}
			}
			#endregion

			#region Private Data
			private XRAdvancedGrabInteractable _interactable;
			#endregion

			#region Unity Messages
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
				if (this.transform.parent != null)
					return this.transform.parent.localToWorldMatrix;

				return this.transform.localToWorldMatrix;
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

			#region Protected Functions
			protected Vector3 WorldToConstraintSpacePos(Vector3 worldPos)
			{
				if (this.transform.parent != null)
					return this.transform.parent.InverseTransformPoint(worldPos);

				return worldPos;
			}

			protected Vector3 ConstraintToWorldSpacePos(Vector3 constraintSpacePos)
			{
				if (this.transform.parent != null)
					return this.transform.parent.TransformPoint(constraintSpacePos);

				return constraintSpacePos;
			}

			protected Vector3 WorldToConstraintSpaceVector(Vector3 worldVec)
			{
				if (this.transform.parent != null)
					return this.transform.parent.InverseTransformVector(worldVec);

				return worldVec;
			}

			protected Vector3 ConstraintToWorldSpaceVector(Vector3 constraintSpaceVector)
			{
				if (this.transform.parent != null)
					return this.transform.parent.TransformVector(constraintSpaceVector);

				return constraintSpaceVector;
			}

			protected Quaternion WorldToConstraintSpaceRotation(Quaternion worldRotation)
			{
				if (this.transform.parent != null)
					return Quaternion.Inverse(this.transform.parent.transform.rotation) * worldRotation;

				return worldRotation;
			}

			protected Quaternion ConstraintToWorldSpaceRotation(Quaternion constraintSpaceRotation)
			{
				if (this.transform.parent != null)
					return this.transform.parent.transform.rotation * constraintSpaceRotation;

				return constraintSpaceRotation;
			}
			#endregion
		}
	}
}