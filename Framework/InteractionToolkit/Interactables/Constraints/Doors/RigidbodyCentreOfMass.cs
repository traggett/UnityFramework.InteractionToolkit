using UnityEngine;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		public class RigidbodyCentreOfMass : MonoBehaviour
		{
			#region Public Data
			public Rigidbody _rigidbody;
			public Transform _centreOfMass;
			#endregion

			#region Unity Messages
			private void OnEnable()
			{
				if (_rigidbody != null)
				{
					Vector3 centerOfMass = _centreOfMass != null ? _centreOfMass.position : this.transform.position;
					_rigidbody.centerOfMass = _rigidbody.transform.InverseTransformPoint(centerOfMass);
				}
			}
			#endregion
		}
	}
}