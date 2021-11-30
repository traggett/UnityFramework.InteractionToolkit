using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		public class XRSphericalConstraint : XRInteractorConstraint
		{
			#region Public Data
			public Transform _attachTransform;
			public bool _constrainPosition;
			public bool _constrainRotation;
			public bool _constrainToSurface;
			public float _radius;
			#endregion

			#region XRInteractorConstraint
			public override void ConstrainInteractor(XRBaseInteractor interactor, out bool constrainPosition, ref Vector3 position, out bool constrainRotation, ref Quaternion rotation)
			{
				Transform attachTransform = _attachTransform != null ? _attachTransform : transform;

				constrainPosition = _constrainPosition;

				if (_constrainPosition)
				{
					Vector3 toCentre = position - attachTransform.position;

					if (_constrainToSurface)
					{
						//Find closed point of surface of sphere
						position = attachTransform.position + (toCentre.normalized * _radius);
					}
					else
					{
						if (toCentre.sqrMagnitude > _radius * _radius)
						{
							position = attachTransform.position + (toCentre.normalized * _radius);
						}
					}
				}

				constrainRotation = _constrainRotation;

				if (_constrainRotation)
				{
					//Rotation faces towards centre?? Plus default rotation??

					rotation = attachTransform.rotation;
				}
			}
			#endregion
		}
	}
}