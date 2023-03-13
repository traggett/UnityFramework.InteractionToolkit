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
				Vector3 localPos = WorldToConstraintSpacePos(position);

				//Work out an angel from it??
				float angle = Vector2.SignedAngle(new Vector2(localPos.x, localPos.z), new Vector2(0f, 1f));
				angle = Mathf.Clamp(angle, _minAngle, _maxAngle);
				
				Quaternion localRotation = Quaternion.AngleAxis(angle, Vector3.up);

				rotation = ConstraintToWorldSpaceRotation(localRotation);

				position = this.transform.position;
			}

			public override void Constrain()
			{
				Rigidbody rigidbody = Interactable.Rigidbody;

				if (!rigidbody.IsSleeping())
				{
					//Don't allow any positional velocity
					rigidbody.velocity = Vector3.zero;

					//Only allow angular velocity around pivot
					Vector3 localAngularVel = WorldToConstraintSpaceVector(rigidbody.angularVelocity);
					localAngularVel.x = 0f;
					localAngularVel.z = 0f;

					//Find door rotation 
					float localAngle = MathUtils.DegreesTo180Range(this.transform.localRotation.eulerAngles.y);

					//If rotated beyond min/max limits then reverse angular velocity and clamp the rotation
					if (localAngle < _minAngle)
					{
						localAngle = _minAngle;

						//Reverse angular velocity?
						//TO DO - dampen or allow the door locking shut??
						localAngularVel.y = Mathf.Abs(localAngularVel.y);
					}

					if (localAngle > _maxAngle)
					{
						localAngle = _maxAngle;

						//Reverse angular velocity?
						//TO DO - dampen or allow the door locking shut??
						localAngularVel.y = -Mathf.Abs(localAngularVel.y);
					}

					this.transform.localRotation = Quaternion.Euler(0f, localAngle, 0f);

					if (!rigidbody.isKinematic)
					{
						rigidbody.angularVelocity = ConstraintToWorldSpaceVector(localAngularVel);
					}
				}
			}
			#endregion

			#region Protected Functions
			public override void DebugDraw(bool selected)
			{
				Debug.DebugDrawing.DrawWireArc(Vector3.zero, _minAngle, _maxAngle, 1f);
			}
			#endregion
		}
	}
}