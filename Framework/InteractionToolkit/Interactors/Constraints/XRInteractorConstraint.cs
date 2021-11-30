using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
    namespace Interaction.Toolkit
    {
        /// <summary>
        /// Component that allows constraining interactor movement whilst interacting with an interactable (eg make a hand stick to a lever)
        /// </summary>
        public abstract class XRInteractorConstraint : MonoBehaviour
        {
			#region Virtual Interface
			public abstract void ConstrainInteractor(XRBaseInteractor interactor, out bool constrainPosition, ref Vector3 position, out bool constrainRotation, ref Quaternion rotation);
            #endregion
        }
	}
}