using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
    namespace Interaction.Toolkit
    {
        namespace VR
        {
            public class VRHandPoser : MonoBehaviour
            {
                public AnimationClip _hoverPose; 
                public AnimationClip _interactPose;

                public void OnHoverEnter(HoverEnterEventArgs eventArgs)
				{
                    if (_hoverPose != null)
					{
                        VRHandController hand = eventArgs.interactor.GetComponent<VRHandController>();

                        if (hand != null)
                        {
                            hand.SetOverridePose(this, _hoverPose);
                        }
                    }
                }

                public void OnHoverExit(HoverExitEventArgs eventArgs)
                {
                    VRHandController hand = eventArgs.interactor.GetComponent<VRHandController>();

                    if (hand != null)
                    {
                        hand.ClearOverridePose(this);
                    }
                }

                public void OnSelectEnter(SelectEnterEventArgs eventArgs)
                {
                    if (_interactPose != null)
                    {
                        VRHandController hand = eventArgs.interactor.GetComponent<VRHandController>();

                        if (hand != null)
                        {
                            hand.SetOverridePose(this, _interactPose);
                        }
                    }
                }

                public void OnSelectExit(SelectExitEventArgs eventArgs)
                {
                    VRHandController hand = eventArgs.interactor.GetComponent<VRHandController>();

                    if (hand != null)
                    {
                        hand.ClearOverridePose(this);
                    }
                }
            }
        }
    }
}