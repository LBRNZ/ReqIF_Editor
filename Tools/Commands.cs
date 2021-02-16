using ReqIFSharp;
using System;
using System.Collections.Generic;
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
			(sender as MainWindow).Edit_SpecObject((sender as MainWindow).MainDataGrid.SelectedItem as SpecObject, false);

		}

		public static void AddSpecObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
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

		public static void AddSpecObject_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SpecObject specObject = new SpecObject()
			{
				Identifier = Guid.NewGuid().ToString(),
				LastChange = DateTime.Now,
				ReqIfContent = (sender as MainWindow).content,
				SpecType = (sender as MainWindow).content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault()
			};
			(sender as MainWindow).Edit_SpecObject(specObject, true);
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
			serializer.Serialize((sender as MainWindow).reqif, (sender as MainWindow).filePath, null);
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
				new CommandBinding(MainWindowCommand.AddSpecObject, AddSpecObject_Executed, AddSpecObject_CanExecute));
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
		public static readonly RoutedUICommand AddSpecObject = new RoutedUICommand
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
