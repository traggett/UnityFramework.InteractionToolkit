using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		public class XRHingedDoorHandleConstraint : XRInteractableConstraint
		{
			#region Public Data
			public float _minAngle;
			public float _maxAngle;
			#endregion

			#region XRInteractableConstraint
			public override void ConstrainTargetTransform(ref Vector3 position, ref Quaternion rotation)
			{
				//XY movement could move handle??
				//Or just rotation???

				//Then need to move door?

				//Work out movemnt in door plane (XY door local space)

				//Use it to move the handel (if hinged)

				//Also use rotation??

				//Then use XZ doorpositoin to move door
			}

			public override void Constrain()
			{
				Rigidbody rigidbody = Interactable.Rigidbody;

				//TO DO!
				//Allow rotation only!
				if (!rigidbody.isKinematic)
				{
					rigidbody.velocity = Vector3.zero;
					rigidbody.angularVelocity = Vector3.zero;
				}
			}
			#endregion

			#region Protected Functions
			protected Quaternion ToHandleSpace(Quaternion worldRotation)
			{
				if (this.transform.parent != null)
					return Quaternion.Inverse(this.transform.parent.rotation) * worldRotation;

				return worldRotation;
			}

			protected Quaternion ToWorldSpace(Quaternion doorSpaceRotation)
			{
				if (this.transform.parent != null)
					return this.transform.parent.transform.rotation * doorSpaceRotation;

				return doorSpaceRotation;
			}
			#endregion
		}
	}
}