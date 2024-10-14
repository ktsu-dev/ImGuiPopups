namespace ktsu.ImGuiPopups;

using System.Numerics;
using ImGuiNET;
using ktsu.CaseConverter;

public partial class ImGuiPopups
{
	/// <summary>
	/// Base class for a modal window.
	/// </summary>
	public class Modal
	{
		internal string Title { get; set; } = string.Empty;
		private bool ShouldOpen { get; set; }
		private Action OnShowContent { get; set; } = () => { };
		private Vector2 CustomSize { get; set; } = Vector2.Zero;
		public bool WasOpen { get; private set; }

		/// <summary>
		/// Gets the id of the modal window.
		/// </summary>
		/// <returns>The id of the modal window.</returns>
		private string Name => $"{Title}###Modal_{Title.ToSnakeCase()}";

		/// <summary>
		/// Open the modal and set the title.
		/// </summary>
		/// <param name="title">The title of the modal window.</param>
		/// <param name="onShowContent">The delegate to invoke to show the content of the modal.</param>
		public void Open(string title, Action onShowContent) => Open(title, onShowContent, customSize: Vector2.Zero);

		/// <summary>
		/// Open the modal and set the title.
		/// </summary>
		/// <param name="title">The title of the modal window.</param>
		/// <param name="onShowContent">The delegate to invoke to show the content of the modal.</param>
		/// <param name="customSize">Custom size of the popup.</param>
		public void Open(string title, Action onShowContent, Vector2 customSize)
		{
			Title = title;
			OnShowContent = onShowContent;
			ShouldOpen = true;
			CustomSize = customSize;
		}

		/// <summary>
		/// Show the modal if it is open.
		/// </summary>
		/// <returns>True if the modal is open.</returns>
		public bool ShowIfOpen()
		{
			if (ShouldOpen)
			{
				ShouldOpen = false;
				ImGui.OpenPopup(Name);
			}

			bool result = ImGui.IsPopupOpen(Name);
			if (CustomSize != Vector2.Zero)
			{
				ImGui.SetNextWindowSize(CustomSize);
			}
			if (ImGui.BeginPopupModal(Name, ref result, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
			{
				OnShowContent();

				if (ImGui.IsKeyPressed(ImGuiKey.Escape))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}
			WasOpen = result;
			return result;
		}
	}
}
