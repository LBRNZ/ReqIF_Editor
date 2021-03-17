using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReqIF_Editor.Commands
{
	public partial class MainWindowCommands : Window
	{
		public static void ShowSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if((sender as MainWindow).content != null)
			{
				e.CanExecute = true;
			} else
			{
				e.CanExecute = false;
			}
		}

		public static void ShowSearch_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as MainWindow).NavigationTabControl.SelectedIndex = 1;
			(sender as MainWindow).SearchInputBox.Focus();
			(sender as MainWindow).Sidepanel.Expanded = true;
		}
		public static void ShowNavigation_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if ((sender as MainWindow).content != null)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		public static void ShowNavigation_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as MainWindow).NavigationTabControl.SelectedIndex = 0;
			(sender as MainWindow).Sidepanel.Expanded = (sender as MainWindow).Sidepanel.Expanded;
		}

		public static void EditSpecObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if ((sender as MainWindow).MainDataGrid.SelectedItem != null)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		public static void EditSpecObject_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as MainWindow).Edit_SpecObject((sender as MainWindow).MainDataGrid.SelectedItem as SpecobjectViewModel, false);

		}

		public static void AddSpecObjectAfter_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if ((sender as MainWindow).MainDataGrid.SelectedItem != null)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		public static void AddSpecObjectUnder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if ((sender as MainWindow).MainDataGrid.SelectedItem != null)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		public static void AddSpecObjectAfter_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as MainWindow).Add_SpecObject("after");
		}

		public static void AddSpecObjectUnder_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as MainWindow).Add_SpecObject("under");
		}

		public static void CloseFile_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as MainWindow).ClearDataGrid();
			(sender as MainWindow).Sidepanel.Expanded = false;
		}
		public static void CloseFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if ((sender as MainWindow).content != null)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		public static void SaveFile_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var serializer = new ReqIFSerializer(false);
			string extension = Path.GetExtension((sender as MainWindow).filePath);
			if(extension == ".reqif")
            {
				serializer.Serialize((sender as MainWindow).reqif, (sender as MainWindow).filePath, null);
			} else if(extension == ".reqifz")
            {
				string reqif = serializer.Serialize((sender as MainWindow).reqif);
				using (var archive = ZipFile.Open((sender as MainWindow).filePath, ZipArchiveMode.Update))
				{
					var entry = archive.Entries.Where(x => x.Name.EndsWith(".reqif", StringComparison.CurrentCultureIgnoreCase)).First();
					var fullName = entry.FullName;
					entry.Delete();
					entry = archive.CreateEntry(fullName);

					using (StreamWriter writer = new StreamWriter(entry.Open()))
					{
						writer.Write(reqif);
					}
				}
			}
			(sender as MainWindow).isContenChanged = false;
		}
		public static void SaveFile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if ((sender as MainWindow).isContenChanged != false)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		public static void BindCommandsToWindow(Window window)
		{
			window.CommandBindings.Add(
				new CommandBinding(MainWindowCommand.ShowSearch, ShowSearch_Executed, ShowSearch_CanExecute));
			window.CommandBindings.Add(
				new CommandBinding(MainWindowCommand.ShowNavigation, ShowNavigation_Executed, ShowNavigation_CanExecute));
			window.CommandBindings.Add(
				new CommandBinding(MainWindowCommand.EditSpecObject, EditSpecObject_Executed, EditSpecObject_CanExecute));
			window.CommandBindings.Add(
				new CommandBinding(MainWindowCommand.AddSpecObjectAfter, AddSpecObjectAfter_Executed, AddSpecObjectAfter_CanExecute));
			window.CommandBindings.Add(
				new CommandBinding(MainWindowCommand.AddSpecObjectUnder, AddSpecObjectUnder_Executed, AddSpecObjectUnder_CanExecute));
			window.CommandBindings.Add(
				new CommandBinding(MainWindowCommand.CloseFile, CloseFile_Executed, CloseFile_CanExecute));
			window.CommandBindings.Add(
				new CommandBinding(MainWindowCommand.SaveFile, SaveFile_Executed, SaveFile_CanExecute));
		}
	}
	public static class MainWindowCommand
	{
		public static readonly RoutedUICommand ShowSearch = new RoutedUICommand
			(
				"ShowSearch",
				"ShowSearch",
				typeof(MainWindowCommand),
				new InputGestureCollection()
				{
					new KeyGesture(Key.F, ModifierKeys.Control)
				}
			);
		public static readonly RoutedUICommand ShowNavigation = new RoutedUICommand
			(
				"ShowNavigation",
				"ShowNavigation",
				typeof(MainWindowCommand)
			);
		public static readonly RoutedUICommand EditSpecObject = new RoutedUICommand
			(
				"EditSpecObject",
				"EditSpecObject",
				typeof(MainWindowCommand)
			);
		public static readonly RoutedUICommand AddSpecObjectAfter = new RoutedUICommand
			(
				"AddSpecObject",
				"AddSpecObject",
				typeof(MainWindowCommand)
			);
		public static readonly RoutedUICommand AddSpecObjectUnder = new RoutedUICommand
			(
				"AddSpecObject",
				"AddSpecObject",
				typeof(MainWindowCommand)
			);
		public static readonly RoutedUICommand CloseFile = new RoutedUICommand
			(
				"CloseFile",
				"CloseFile",
				typeof(MainWindowCommand)
			);
		public static readonly RoutedUICommand SaveFile = new RoutedUICommand
			(
				"SaveFile",
				"SaveFile",
				typeof(MainWindowCommand)
			);
	}
}
