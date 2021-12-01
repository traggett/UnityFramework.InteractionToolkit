using Framework.Maths;
using UnityEngine;

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
			public override void ConstrainTargetTransform(ref Vector3 position, ref Quaternion rotation)
			{
				//Ignore inputted rotation,

				//Work out a rotation that is best for this position

				//Get local position in 2d plane
				Vector3 localPos = ToDoorSpacePosition(position);

				//Work out an angel from it??
				float angle = Vector2.SignedAngle(new Vector2(localPos.x, localPos.z), new Vector2(0f, 1f));
				angle = Mathf.Clamp(angle, _minAngle, _maxAngle);
				
				Quaternion localRotation = Quaternion.AngleAxis(angle, Vector3.up);

				rotation = ToWorldSpace(localRotation);

				position = this.transform.position;
			}

			public override void Constrain()
			{
				Rigidbody rigidbody = Interactable.Rigidbody;

				//Don't allow any positional velocity
				rigidbody.velocity = Vector3.zero;

				//Only allow angular velocity around pivot
				Vector3 localAngularVel = ToDoorSpaceVector(rigidbody.angularVelocity);
				localAngularVel.x = 0f;
				localAngularVel.z = 0f;

				//Find door rotation 
				float localAngle = MathUtils.DegreesTo180Range(this.transform.localRotation.eulerAngles.y);

				//If rotated beyond min/max limits then reverse angular velocity and clamp the rotation
				if (localAngle < _minAngle)
				{
					localAngle = _minAngle;

					//TO DO! reverse angular velocity? Damen it?
					//Or trigger event so door can stay shut / trigger sound effects??
				}

				if (localAngle > _maxAngle)
				{
					localAngle = _maxAngle;

					//TO DO! reverse angular velocity? Damen it?
					//Or trigger event so door can stay shut / trigger sound effects??
				}

				this.transform.localRotation = Quaternion.Euler(0f, localAngle, 0f);

				rigidbody.angularVelocity = FromDoorSpaceVector(localAngularVel);
			}
			#endregion

			#region Protected Functions
			protected Vector3 ToDoorSpacePosition(Vector3 worldPos)
			{
				if (this.transform.parent != null)
					return this.transform.parent.InverseTransformPoint(worldPos);

				return worldPos;
			}

			protected Vector3 ToDoorSpaceVector(Vector3 worldVec)
			{
				if (this.transform.parent != null)
					return this.transform.parent.InverseTransformVector(worldVec);

				return worldVec;
			}

			protected Vector3 FromDoorSpaceVector(Vector3 doorSpaceVec)
			{
				if (this.transform.parent != null)
					return this.transform.parent.TransformVector(doorSpaceVec);

				return doorSpaceVec;
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