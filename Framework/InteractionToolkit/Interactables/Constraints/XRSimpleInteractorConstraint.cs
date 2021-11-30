using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// Component that constrains an interactor to a transform whilst interacting with it
		/// </summary>
		public class XRSimpleInteractorConstraint : XRInteractorConstraint
		{
			public Transform _attachTransform;
			public bool _constrainPosition;
			public bool _constrainRotation;

			#region XRInteractorConstraint
			public override void ConstrainInteractor(XRBaseInteractor interactor, out bool constrainPosition, ref Vector3 position, out bool constrainRotation, ref Quaternion rotation)
			{
				Transform attachTransform = _attachTransform != null ? _attachTransform : transform;

				constrainRotation = _constrainRotation;

				if (_constrainRotation)
				{
					rotation = attachTransform.rotation;
				}

				constrainPosition = _constrainPosition;

				if (_constrainPosition)
				{
					position = attachTransform.position;
				}
			}
			#endregion
		}
	}
}