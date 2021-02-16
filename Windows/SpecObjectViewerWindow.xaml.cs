using ReqIF_Editor.Windows;
using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ReqIF_Editor
{
    /// <summary>
    /// Interaktionslogik für SpecObjectViewerWindow.xaml
    /// </summary>
    public partial class SpecObjectViewerWindow : Window
    {
        private bool _newSpecObject;
        private SpecObject _specObject;
        public SpecObjectViewerWindow(SpecObject specObject, bool newSpecObject)
        {
            InitializeComponent();
            DataTable.ItemsSource = specObject.Values;
            _newSpecObject = newSpecObject;
            _specObject = specObject;

        }

        private void SaveSpecObject_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_newSpecObject)
            {
                int currentIndex = (Application.Current.MainWindow as MainWindow).MainDataGrid.SelectedIndex;
                (Application.Current.MainWindow as MainWindow).content.SpecObjects.Insert(currentIndex + 1, _specObject);
            }
            for (int i = 0; i < DataTable.Items.Count; i++)
            {
                DataGridRow row = DataTable.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                BindingExpression binding = row.BindingGroup.BindingExpressions.First() as BindingExpression;
                (binding.DataItem as AttributeValue).PropertyChanged += SpecObjectViewerWindow_PropertyChanged;
                binding.UpdateSource();
            }
            
            Close();
        }

        private void SpecObjectViewerWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow).isContenChanged = true;
        }

        private void CancelSpecObjectButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddSpecObjectButton_Click(object sender, RoutedEventArgs e)
        {
            string title = "Attribut wählen";
            List<AttributeDefinition> atrDef = (Application.Current.MainWindow as MainWindow).content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes;
            SelectAttributeDialogWindow cdw = new SelectAttributeDialogWindow(atrDef, title);
            cdw.Owner = this;
            if ((bool)cdw.ShowDialog())
            {
                AttributeValue attributeValue = null;
                AttributeDefinition selectedAttribute = cdw.selectedAttribute;
                if (selectedAttribute.GetType() == typeof(AttributeDefinitionBoolean))
                {
                    attributeValue = new AttributeValueBoolean();
                    attributeValue.ObjectValue = false;
                } else if (selectedAttribute.GetType() == typeof(AttributeDefinitionDate))
                {
                    attributeValue = new AttributeValueDate();
                    attributeValue.ObjectValue = DateTime.Now;
                }
                else if (selectedAttribute.GetType() == typeof(AttributeDefinitionEnumeration))
                {
                    attributeValue = new AttributeValueEnumeration();
                    List<EnumValue> enumValues = new List<EnumValue>();
                    enumValues.Add(((DatatypeDefinitionEnumeration)selectedAttribute.DatatypeDefinition).SpecifiedValues.First());
                    attributeValue.ObjectValue = enumValues;
                }
                else if (selectedAttribute.GetType() == typeof(AttributeDefinitionInteger))
                {
                    attributeValue = new AttributeValueInteger();
                    attributeValue.ObjectValue = "";
                }
                else if (selectedAttribute.GetType() == typeof(AttributeDefinitionReal))
                {
                    attributeValue = new AttributeValueReal();
                    attributeValue.ObjectValue = "";
                }
                else if (selectedAttribute.GetType() == typeof(AttributeDefinitionString))
                {
                    attributeValue = new AttributeValueString();
                    attributeValue.ObjectValue = "";
                }
                else if (selectedAttribute.GetType() == typeof(AttributeDefinitionXHTML))
                {
                    attributeValue = new AttributeValueXHTML();
                    attributeValue.ObjectValue = "<div></div>";
                }
                attributeValue.AttributeDefinition = selectedAttribute;
                _specObject.Values.Add(attributeValue);
                DataTable.Items.Refresh();
            }
        }
    }
}
