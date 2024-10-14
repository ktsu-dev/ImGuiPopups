namespace ktsu.ImGuiPopups;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using ImGuiNET;
using ktsu.Extensions;
using ktsu.StrongPaths;
using Microsoft.Extensions.FileSystemGlobbing;

public partial class ImGuiPopups
{
	public enum FilesystemBrowserMode
	{
		Open,
		Save
	}

	public enum FilesystemBrowserTarget
	{
		File,
		Directory
	}

	/// <summary>
	/// A class for displaying a prompt popup window.
	/// </summary>
	public class FilesystemBrowser
	{
		private FilesystemBrowserMode BrowserMode { get; set; }
		private FilesystemBrowserTarget BrowserTarget { get; set; }
		private Action<AbsoluteFilePath> OnChooseFile { get; set; } = (f) => { };
		private Action<AbsoluteDirectoryPath> OnChooseDirectory { get; set; } = (d) => { };
		[JsonInclude]
		private AbsoluteDirectoryPath CurrentDirectory { get; set; } = (AbsoluteDirectoryPath)Environment.CurrentDirectory;
		private Collection<AnyAbsolutePath> CurrentContents { get; set; } = [];
		private AnyAbsolutePath ChosenItem { get; set; } = new();
		private Collection<string> Drives { get; set; } = [];
		private string Glob { get; set; } = "*";
		private Matcher Matcher { get; set; } = new();
		private FileName FileName { get; set; } = new();
		private Modal Modal { get; } = new();
		private MessageOK PopupMessageOK { get; } = new();

		public void FileOpen(string title, Action<AbsoluteFilePath> onChooseFile, string glob = "*") => FileOpen(title, onChooseFile, customSize: Vector2.Zero, glob);
		public void FileOpen(string title, Action<AbsoluteFilePath> onChooseFile, Vector2 customSize, string glob = "*") => File(title, FilesystemBrowserMode.Open, onChooseFile, customSize, glob);

		public void FileSave(string title, Action<AbsoluteFilePath> onChooseFile, string glob = "*") => FileSave(title, onChooseFile, customSize: Vector2.Zero, glob);
		public void FileSave(string title, Action<AbsoluteFilePath> onChooseFile, Vector2 customSize, string glob = "*") => File(title, FilesystemBrowserMode.Save, onChooseFile, customSize, glob);

		private void File(string title, FilesystemBrowserMode mode, Action<AbsoluteFilePath> onChooseFile, Vector2 customSize, string glob) => OpenPopup(title, mode, FilesystemBrowserTarget.File, onChooseFile, (d) => { }, customSize, glob);

		public void ChooseDirectory(string title, Action<AbsoluteDirectoryPath> onChooseDirectory) => ChooseDirectory(title, onChooseDirectory, customSize: Vector2.Zero);
		public void ChooseDirectory(string title, Action<AbsoluteDirectoryPath> onChooseDirectory, Vector2 customSize) => OpenPopup(title, FilesystemBrowserMode.Open, FilesystemBrowserTarget.Directory, (d) => { }, onChooseDirectory, customSize, "*");

		private void OpenPopup(string title, FilesystemBrowserMode mode, FilesystemBrowserTarget target, Action<AbsoluteFilePath> onChooseFile, Action<AbsoluteDirectoryPath> onChooseDirectory, Vector2 customSize, string glob)
		{
			FileName = new();
			BrowserMode = mode;
			BrowserTarget = target;
			OnChooseFile = onChooseFile;
			OnChooseDirectory = onChooseDirectory;
			Glob = glob;
			Matcher = new();
			Matcher.AddInclude(Glob);
			Drives.Clear();
			Environment.GetLogicalDrives().ForEach(Drives.Add);
			RefreshContents();
			Modal.Open(title, ShowContent, customSize);
		}

		/// <summary>
		/// Show the content of the popup.
		/// </summary>
		private void ShowContent()
		{
			if (Drives.Count != 0)
			{
				if (ImGui.BeginCombo("##Drives", Drives[0]))
				{
					string currentDrive = CurrentDirectory.Split(Path.VolumeSeparatorChar).First() + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar;
					foreach (string drive in Drives)
					{
						if (ImGui.Selectable(drive, drive == currentDrive))
						{
							CurrentDirectory = (AbsoluteDirectoryPath)drive;
							RefreshContents();
						}
					}
					ImGui.EndCombo();
				}
			}
			ImGui.TextUnformatted($"{CurrentDirectory}{Path.DirectorySeparatorChar}{Glob}");
			if (ImGui.BeginChild("FilesystemBrowser", new(500, 400), ImGuiChildFlags.None))
			{
				if (ImGui.BeginTable(nameof(FilesystemBrowser), 1, ImGuiTableFlags.Borders))
				{
					ImGui.TableSetupColumn("Path", ImGuiTableColumnFlags.WidthStretch, 40);
					//ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.None, 3);
					//ImGui.TableSetupColumn("Modified", ImGuiTableColumnFlags.None, 3);
					ImGui.TableHeadersRow();

					var flags = ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.NoAutoClosePopups;
					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					if (ImGui.Selectable("..", false, flags))
					{
						if (ImGui.IsMouseDoubleClicked(0))
						{
							string? newPath = Path.GetDirectoryName(CurrentDirectory.WeakString.Trim(Path.DirectorySeparatorChar));
							if (newPath is not null)
							{
								CurrentDirectory = (AbsoluteDirectoryPath)newPath;
								RefreshContents();
							}
						}
					}

					foreach (var path in CurrentContents.OrderBy(p => p is not AbsoluteDirectoryPath).ThenBy(p => p).ToCollection())
					{
						ImGui.TableNextRow();
						ImGui.TableNextColumn();
						var directory = path as AbsoluteDirectoryPath;
						var file = path as AbsoluteFilePath;
						string displayPath = path.WeakString;
						displayPath = displayPath.RemovePrefix(CurrentDirectory).Trim(Path.DirectorySeparatorChar);

						if (directory is not null)
						{
							displayPath += Path.DirectorySeparatorChar;
						}

						if (ImGui.Selectable(displayPath, ChosenItem == path, flags))
						{
							if (directory is not null)
							{
								ChosenItem = directory;
								if (ImGui.IsMouseDoubleClicked(0))
								{
									CurrentDirectory = directory;
									RefreshContents();
								}
							}
							else if (file is not null)
							{
								ChosenItem = file;
								FileName = file.FileName;
								if (ImGui.IsMouseDoubleClicked(0))
								{
									ChooseItem();
								}
							}
						}
					}
					ImGui.EndTable();
				}
			}
			ImGui.EndChild();

			if (BrowserMode == FilesystemBrowserMode.Save)
			{
				string fileName = FileName;
				ImGui.InputText("##SaveAs", ref fileName, 256);
				FileName = (FileName)fileName;
			}

			string confirmText = BrowserMode switch
			{
				FilesystemBrowserMode.Open => "Open",
				FilesystemBrowserMode.Save => "Save",
				_ => "Choose"
			};
			if (ImGui.Button(confirmText))
			{
				ChooseItem();
			}
			ImGui.SameLine();
			if (ImGui.Button("Cancel"))
			{
				ImGui.CloseCurrentPopup();
			}
			PopupMessageOK.ShowIfOpen();
		}

		private void ChooseItem()
		{
			if (BrowserTarget == FilesystemBrowserTarget.File)
			{
				var chosenFile = CurrentDirectory / FileName;
				if (!Matcher.Match(FileName).HasMatches)
				{
					PopupMessageOK.Open("Invalid File Name", "The file name does not match the glob pattern.");
					return;
				}

				OnChooseFile((AbsoluteFilePath)Path.GetFullPath(chosenFile));
			}
			else if (BrowserTarget == FilesystemBrowserTarget.Directory && ChosenItem is AbsoluteDirectoryPath directory)
			{
				OnChooseDirectory((AbsoluteDirectoryPath)Path.GetFullPath(directory));
			}
			ImGui.CloseCurrentPopup();
		}

		private void RefreshContents()
		{
			ChosenItem = new();
			CurrentContents.Clear();
			CurrentDirectory.Contents.ForEach(p =>
			{
				if (BrowserTarget == FilesystemBrowserTarget.File || (BrowserTarget == FilesystemBrowserTarget.Directory && p is AbsoluteDirectoryPath))
				{
					if (p is AbsoluteDirectoryPath || Matcher.Match(Path.GetFileName(p)).HasMatches)
					{
						CurrentContents.Add(p);
					}
				}
			});
		}

		/// <summary>
		/// Show the modal if it is open.
		/// </summary>
		/// <returns>True if the modal is open.</returns>
		public bool ShowIfOpen() => Modal.ShowIfOpen();
	}
}
