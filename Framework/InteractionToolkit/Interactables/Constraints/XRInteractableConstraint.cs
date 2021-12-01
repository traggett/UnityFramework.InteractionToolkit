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
			#endregion
		}
	}
}