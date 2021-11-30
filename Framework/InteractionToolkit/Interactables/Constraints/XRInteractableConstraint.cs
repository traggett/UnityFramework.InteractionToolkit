using System;
using UnityEngine;

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
			#endregion

			#region Virtual Interface
			public abstract void ConstrainTransform(ref Vector3 position, ref Quaternion rotation);

            public abstract void ConstrainPhysics(Rigidbody rigidbody);
            #endregion
        }
	}
}