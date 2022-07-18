using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// Component can be added to a XRBaseInteractable that will trigger UI Pointer events from on XRInteractors
		/// This means your other components can implement OnPointerDown or OnPointerClick etc
		/// </summary>
		[RequireComponent(typeof(XRBaseInteractable))]
		public class XRInteractableUIEvents : MonoBehaviour
		{
			#region Private Data
			private XRUIInputModule _inputModule;
			private List<XRBaseControllerInteractor> _hoveredInteractors = new List<XRBaseControllerInteractor>();
			private List<XRBaseControllerInteractor> _presssControllers = new List<XRBaseControllerInteractor>();
			#endregion

			#region Unity Messages
			private void Awake()
			{
				XRBaseInteractable interactable = GetComponent<XRBaseInteractable>();

				if (interactable != null)
				{
					interactable.hoverEntered.AddListener(OnHoverEnter);
					interactable.hoverExited.AddListener(OnHoverExit);
				}
			}

			private void OnDestroy()
			{
				XRBaseInteractable interactable = GetComponent<XRBaseInteractable>();

				if (interactable != null)
				{
					interactable.hoverEntered.RemoveListener(OnHoverEnter);
					interactable.hoverExited.RemoveListener(OnHoverExit);
				}
			}

			private void OnEnable()
			{
				FindInputModule();

				_hoveredInteractors.Clear();
				_presssControllers.Clear();
			}

			private void OnDisable()
			{
				if (_hoveredInteractors.Count > 0)
				{
					TrigggerHoverExit(null);
					_hoveredInteractors.Clear();
				}

				if (_presssControllers.Count > 0)
				{
					TriggerPointerUp(null);
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
						_presssControllers.Add(interactor);
						TriggerPointerDown(interactor);
					}
				}

				//Check for UI press up on any pressed controller
				for (int i = 0; i < _presssControllers.Count;)
				{
					if (!_presssControllers[i].xrController.uiPressInteractionState.active)
					{
						XRBaseControllerInteractor interactor = _presssControllers[i];
						_presssControllers.RemoveAt(i);
						TriggerPointerUp(interactor);
					}
					else
					{
						i++;
					}
				}
			}
			#endregion

			#region Private Functions
			public void OnHoverEnter(HoverEnterEventArgs args)
			{
				if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
				{
					_hoveredInteractors.Add(controllerInteractor);
					TriggerHoverEnter(controllerInteractor);
				}
			}

			public void OnHoverExit(HoverExitEventArgs args)
			{
				if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
				{
					_hoveredInteractors.Remove(controllerInteractor);
					TrigggerHoverExit(controllerInteractor);
				}
			}

			private void TriggerHoverEnter(XRBaseControllerInteractor controllerInteractor)
			{
				PointerEventData eventData = CreateEvent(controllerInteractor);
				eventData.pointerEnter = this.gameObject;
				
				ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerEnterHandler);
			}

			private void TrigggerHoverExit(XRBaseControllerInteractor controllerInteractor)
			{
				PointerEventData eventData = CreateEvent(controllerInteractor);
				ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerExitHandler);
			}

			private void TriggerPointerDown(XRBaseControllerInteractor controllerInteractor)
			{
				PointerEventData eventData = CreateEvent(controllerInteractor);
				eventData.button = PointerEventData.InputButton.Left;
				eventData.pointerPress = this.gameObject;

				ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerDownHandler);
			}

			private void TriggerPointerUp(XRBaseControllerInteractor controllerInteractor)
			{
				PointerEventData eventData = CreateEvent(controllerInteractor);
				eventData.button = PointerEventData.InputButton.Left;

				ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerUpHandler);

				if (_hoveredInteractors.Count > 0)
				{
					eventData = CreateEvent(controllerInteractor);
					eventData.button = PointerEventData.InputButton.Left;

					ExecuteEvents.Execute(this.gameObject, eventData, ExecuteEvents.pointerClickHandler);
				}
			}

			private PointerEventData CreateEvent(XRBaseControllerInteractor controllerInteractor)
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current);

				if (controllerInteractor is IUIInteractor uiInteractor)
				{
					if (_inputModule != null)
					{
						if (_inputModule.GetTrackedDeviceModel(uiInteractor, out TrackedDeviceModel model))
						{
							eventData.pointerId = model.pointerId;
							eventData.position = model.position;
							eventData.scrollDelta = model.scrollDelta;
							eventData.pointerCurrentRaycast = model.currentRaycast;
						}
					}
				}

				return eventData;
			}

			private void FindInputModule()
			{	
				if (_inputModule == null)
				{
					_inputModule = EventSystem.current.GetComponent<XRUIInputModule>();
				}
			}
			#endregion
		}

	}
}