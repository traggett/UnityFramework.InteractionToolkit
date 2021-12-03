using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
    namespace Interaction.Toolkit
    {
		namespace XR
		{
			/// <summary>
			/// Component that forces a hand into a pose or position/rotation whilst
			/// </summary>
			public abstract class XRHandPoser : MonoBehaviour
			{
				#region Public Interface
				/// <summary>
				/// Hook this function up to the OnSelectedEnter or OnHoverEnter or Activate events on an XRBaseInteractable
				/// </summary>
				public void SetHandPose(BaseInteractionEventArgs args)
				{
					XRHandInteractor handInteractor = args.interactor as XRHandInteractor;

					if (handInteractor != null)
					{
						if (args is SelectEnterEventArgs)
							handInteractor.ApplyHandPoserOnSelected(this);
						else if (args is HoverEnterEventArgs)
							handInteractor.ApplyHandPoserOnHovered(this);
					}
				}

				/// <summary>
				/// Hook this function up to the OnSelectedExit or OnHoverExit or Deactivate event on an XRBaseInteractable
				/// </summary>
				public void ClearHandPose(BaseInteractionEventArgs args)
				{
					XRHandInteractor handInteractor = args.interactor as XRHandInteractor;

					if (handInteractor != null)
					{
						if (args is SelectExitEventArgs)
							handInteractor.ClearHandPoserOnSelected(this);
						else if (args is HoverExitEventArgs)
							handInteractor.ClearHandPoserOnHovered(this);
					}
				}
				#endregion

				#region Virtual Interface
				public abstract XRHandPose GetPose(XRHandInteractor interactor);
				#endregion
			}
		}
	}
}