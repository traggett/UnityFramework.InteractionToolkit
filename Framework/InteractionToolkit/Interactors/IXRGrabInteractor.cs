namespace Framework
{
	namespace Interaction.Toolkit
	{
		public interface IXRGrabInteractor
		{
			#region Public Interface
			/// <summary>
			/// Returns if the grab interactor can actually grab an avialable interactible.
			/// </summary>
			bool IsGrabbedObjectAttached();
			#endregion
		}
	}
}