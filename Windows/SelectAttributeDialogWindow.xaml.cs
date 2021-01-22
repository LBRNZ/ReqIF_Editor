using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReqIFSharp;

namespace ReqIF_Editor.Windows
{
    /// <summary>
    /// Interaktionslogik für ComboboxDialogWindow.xaml
    /// </summary>
    public partial class SelectAttributeDialogWindow : Window
    {
        public AttributeDefinition selectedAttribute;
        public SelectAttributeDialogWindow(List<AttributeDefinition> selection, string title)
        {
            InitializeComponent();
            Combobox.ItemsSource = selection;
            Combobox.SelectedIndex = 0;
            this.Title = title;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            selectedAttribute = Combobox.SelectedItem as AttributeDefinition;
            Close();
        }
    }
}
