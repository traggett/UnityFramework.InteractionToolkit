using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Framework
{
	namespace Interaction.Toolkit
	{
		public interface IXRGrabInteractor
		{
			#region Public Interface
			bool IsGrabbing();
			#endregion
		}
	}
}