using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		/// <summary>
		/// <see cref="UnityEvent"/> that is invoked when an Interactor throws this interactible on detach.
		/// </summary>
		[Serializable]
		public sealed class GrabEvent : UnityEvent<GrabEventArgs>
		{
		}

		/// <summary>
		/// Event data associated with the event when an Interactor throws this interactible on detach.
		/// </summary>
		public class GrabEventArgs : BaseInteractionEventArgs
		{
			
		}

		/// <summary>
		/// <see cref="UnityEvent"/> that is invoked when an Interactor drops this interactible.
		/// </summary>
		[Serializable]
		public sealed class DropEvent : UnityEvent<DropEventArgs>
		{
		}

		/// <summary>
		/// Event data associated with the event when an Interactor drops this interactible.
		/// </summary>
		public class DropEventArgs : BaseInteractionEventArgs
		{
			public Vector3 velocity;
			public Vector3 angularVelocity;
		}
	}
}