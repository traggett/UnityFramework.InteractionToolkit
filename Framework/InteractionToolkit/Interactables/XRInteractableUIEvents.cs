using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// Component that will trigger UI Pointer events from on XRInteractors
		/// This means your other components can implement OnPointerDown or OnPointerClick etc
		/// </summary>
		public class XRInteractableUIEvents : XRBaseInteractable
		{
			#region Private Data
			private List<XRBaseControllerInteractor> _hoveredInteractors = new List<XRBaseControllerInteractor>();
			private List<XRBaseController> _presssControllers = new List<XRBaseController>();
			#endregion

			#region Unity Messages
			protected override void Awake()
			{
				base.Awake();

				this.hoverEntered.AddListener(OnHoverEnter);
				this.hoverExited.AddListener(OnHoverExit);
			}

			protected override void OnEnable()
			{
				base.OnEnable();

				_hoveredInteractors.Clear();
				_presssControllers.Clear();
			}

			protected override void OnDisable()
			{
				base.OnDisable();

				if (_hoveredInteractors.Count > 0)
				{
					OnUIHoverExit();
					_hoveredInteractors.Clear();
				}
					
				if (_presssControllers.Count > 0)
				{
					OnUIPointerUp();
					_presssControllers.Clear();
				}			
			}

			private void Update()
			{
				//Check for UI press down on any hovered controller
				foreach (XRBaseControllerInteractor interactor in _hoveredInteractors)
				{
					if (interactor.xrController.uiPressInteractionState.activatedThisFrame)
					{
						_presssControllers.Add(interactor.xrController);
						OnUIPointerDown();
					}
				}

				//Check for UI press up on any pressed controller
				for (int i = 0; i < _presssControllers.Count;)
				{
					if (!_presssControllers[i].uiPressInteractionState.active)
					{
						_presssControllers.RemoveAt(i);
						OnUIPointerUp();
					}
					else
					{
						i++;
					}
				}
			}
			#endregion

			#region Private Functions
			private void OnHoverEnter(HoverEnterEventArgs args)
			{
				if (args.interactor is XRBaseControllerInteractor controllerInteractor)
				{
					_hoveredInteractors.Add(controllerInteractor);
					OnUIHoverEnter();
				}				
			}

			private void OnHoverExit(HoverExitEventArgs args)
			{
				if (args.interactor is XRBaseControllerInteractor controllerInteractor)
				{
					_hoveredInteractors.Remove(controllerInteractor);
					OnUIHoverExit();
				}
			}

			private void OnUIHoverEnter()
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current)
				{

				};

				ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerEnterHandler);
			}

			private void OnUIHoverExit()
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current)
				{

				};

				ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerExitHandler);
			}

			private void OnUIPointerDown()
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current)
				{
					button = PointerEventData.InputButton.Left
				};

				ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerDownHandler);
			}

			private void OnUIPointerUp()
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current)
				{
					button = PointerEventData.InputButton.Left
				};

				ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerUpHandler);

				if (_hoveredInteractors.Count > 0)
				{
					eventData = new PointerEventData(EventSystem.current)
					{
						button = PointerEventData.InputButton.Left
					};

					ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerClickHandler);
				}
			}
			#endregion
		}
	}
}