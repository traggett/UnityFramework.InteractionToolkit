using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		public class XRHingedDoorConstraint : XRInteractableConstraint
		{
			#region Public Data
			public float _minAngle;
			public float _maxAngle;
			#endregion

			#region XRInteractableConstraint
			public override void ProcessConstraint(XRInteractionUpdateOrder.UpdatePhase updatePhase)
			{
				
			}

			public override void Constrain(ref Vector3 position, ref Quaternion rotation)
			{
				//Ignore inputted rotation,

				//Work out a rotation that is best for this position

				//Get local position in 2d plane
				Vector3 localPos = ToDoorSpace(position);

				//Work out an angel from it??
				float angle = Vector2.SignedAngle(new Vector2(localPos.x, localPos.z), new Vector2(0f, 1f));
				angle = Mathf.Clamp(angle, _minAngle, _maxAngle);
				
				UnityEngine.Debug.Log("Angle " + angle);
				UnityEngine.Debug.Log("localPos " + localPos);

				Quaternion localRotation = Quaternion.AngleAxis(angle, Vector3.up);

				rotation = ToWorldSpace(localRotation);

				position = this.transform.position;
			}

			public override void ConstrainPhysics(Rigidbody rigidbody)
			{
				//TO DO!
				//Allow rotation only!
				rigidbody.velocity = Vector3.zero;
				rigidbody.angularVelocity = Vector3.zero;
			}
			#endregion

			#region Protected Functions
			protected Vector3 ToDoorSpace(Vector3 worldPos)
			{
				if (this.transform.parent != null)
					return this.transform.parent.InverseTransformPoint(worldPos);

				return worldPos;
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