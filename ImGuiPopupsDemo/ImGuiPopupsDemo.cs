namespace ktsu.ImGuiPopupsDemo;

using System.Numerics;
using ImGuiNET;
using ktsu.ImGuiApp;
using ktsu.ImGuiPopups;

internal class ImGuiPopupsDemo
{
	private static void Main()
	{
		ImGuiPopupsDemo imGuiPopupsDemo = new();
		ImGuiApp.Start(nameof(ImGuiPopupsDemo), new ImGuiAppWindowState(), imGuiPopupsDemo.OnStart, imGuiPopupsDemo.OnTick, imGuiPopupsDemo.OnMenu, imGuiPopupsDemo.OnWindowResized);
	}

	private static string inputString = "String Input Popup";
	internal static readonly string[] Friends = ["James", "Cameron", "Matt", "Troy", "Hali"];

	private readonly ImGuiPopups.InputString popupInputString = new();
	private readonly ImGuiPopups.FilesystemBrowser popupFilesystemBrowser = new();
	private readonly ImGuiPopups.MessageOK popupMessageOK = new();
	private readonly ImGuiPopups.SearchableList<string> popupSearchableList = new();

	private void OnStart()
	{
	}

	private void OnTick(float dt)
	{
		if (ImGui.Button(inputString))
		{
			popupInputString.Open("Enter a string", "Enter", "Yeet", (string result) =>
			{
				inputString = result;
			});
		}

		if (ImGui.Button("Open File"))
		{
			popupFilesystemBrowser.FileOpen("Open File", (f) =>
			{
				popupMessageOK.Open("File Chosen", $"You chose: {f}");
			}, "*.cs");
		}

		if (ImGui.Button("Save File"))
		{
			popupFilesystemBrowser.FileSave("Save File", (f) =>
			{
				popupMessageOK.Open("File Chosen", $"You chose: {f}");
			}, "*.cs");
		}

		if (ImGui.Button("Choose Best Friend"))
		{
			popupSearchableList.Open("Best Friend", "Who is your best friend?", Friends, (string result) =>
			{
				popupMessageOK.Open("Best Friend Chosen", $"You chose: {result}");
			});
		}

		if (ImGui.Button("View Wall Of Text"))
		{
			#region WallOfTextVariable
			string message = @"What Is Lorem Ipsum?

Lorem ipsum is a placeholder text used in the graphic, print, and publishing industries to preview layouts and visual mockups. It is a dummy text that is not intended to be read, but rather to fill space on a page and give an idea of how the final design will look.

The passage usually begins with the phrase, “Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.”

The purpose of Lorem Ipsum is to create a natural-looking block of text (sentence, paragraph, page, etc.) that does not distract from the layout. It is a practice that is not without controversy, as laying out pages with meaningless filler text can be useful when the focus is meant to be on design, not content.

Lorem ipsum experienced a surge in popularity during the 1960s when Letraset used it on their dry-transfer sheets, and again during the 90s as desktop publishers bundled the text with their software. Today, it is seen all around the web on templates, websites, and stock designs.

Where Does Lorem Ipsum Come From?

Contrary to popular belief, Lorem Ipsum is not simply random text. It has a long history dating back to the 15th century.

Lorem ipsum is attributed to an unknown typesetter who is thought to have scrambled parts of Cicero’s “De Finibus Bonorum et Malorum” for use in a type specimen book.

Richard McClintock, a Latin scholar, is credited with discovering the source behind Lorem Ipsum, which is very similar to sections 1.10.32–33 of Cicero’s work:

""Sed ut perspiciatis, unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt, explicabo. Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est, qui dolorem ipsum, quia dolor sit amet consectetur adipisci[ng] velit, sed quia non numquam [do] eius modi tempora inci[di]dunt, ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit, qui in ea voluptate velit esse, quam nihil molestiae consequatur, vel illum, qui dolorem eum fugiat, quo voluptas nulla pariatur?

Cicero’s “De Finibus Bonorum et Malorum” – 45 BC""";
			#endregion

			popupMessageOK.Open("Wall Of Text", message, ImGuiPopups.PromptTextLayoutType.Wrapped, new Vector2(ImGui.GetContentRegionAvail().X * 0.75f, -1.0f));
		}

		popupSearchableList.ShowIfOpen();
		popupMessageOK.ShowIfOpen();
		popupInputString.ShowIfOpen();
		popupFilesystemBrowser.ShowIfOpen();
	}

	private void OnMenu()
	{
		// Method intentionally left empty.
	}

	private void OnWindowResized()
	{
		// Method intentionally left empty.
	}
}
