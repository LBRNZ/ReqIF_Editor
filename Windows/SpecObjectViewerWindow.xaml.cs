using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.CSharp;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Smith.WPF.HtmlEditor;
using Xceed.Wpf.Toolkit;

namespace ReqIF_Editor
{
    /// <summary>
    /// Interaktionslogik für SpecObjectViewerWindow.xaml
    /// </summary>
    public partial class SpecObjectViewerWindow : Window
    {
        private SpecobjectViewModel _specObject;
        private ObservableCollection<AttributeValueViewModel> _attributes;
        public SpecObjectViewerWindow(SpecobjectViewModel specObject)
        {
            InitializeComponent();

            _specObject = specObject;

            //Create temporary collection of attributes
            _attributes = new ObservableCollection<AttributeValueViewModel>();
            foreach (var value in specObject.Values)
            {
                AttributeValue AttributeValue;
                if (value.AttributeValue != null)
                {

                    if(value.AttributeValue.GetType() == typeof(AttributeValueEnumeration))
                    {
                        AttributeValue = new AttributeValueEnumeration();
                        AttributeValue.AttributeDefinition = value.AttributeValue.AttributeDefinition;
                        AttributeValue.ObjectValue = (value.AttributeValue.ObjectValue as List<EnumValue>).Clone();
                    } else
                    {
                        AttributeValue = value.AttributeValue.Clone();
                    }
                    AttributeValue.PropertyChanged += AttributeValue_PropertyChanged;

                } else
                {
                    AttributeValue = null;
                }
                _attributes.Add(new AttributeValueViewModel()
                {
                    AttributeValue = AttributeValue,
                    AttributeDefinition = value.AttributeDefinition
                });
            }
            DataTable.ItemsSource = _attributes;
            InfoExpander.DataContext = specObject;

        }

        private void AttributeValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var AttrDef = (sender as AttributeValue).AttributeDefinition;
            _attributes.Single(x => x.AttributeDefinition == AttrDef).changed = true;
        }

        private void SaveSpecObject_Button_Click(object sender, RoutedEventArgs e)
        {
            // Add new AttributeValues to SpecObject and update changed AttributeValues
            foreach (var definition in _attributes.Where(x => x.added == true || x.changed == true))
            {
                _specObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition).AttributeValue
                    = _attributes.Single(x => x.AttributeDefinition == definition.AttributeDefinition).AttributeValue;
                _specObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition).changed = definition.changed;
                _specObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition).added = definition.added;
            }

            // Remove AttributeValues from SpecObject
            foreach (var definition in _attributes.Where(x => x.removed == true))
            {
                _specObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition).AttributeValue = null;
                _specObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition).removed = true;
            }

            //Update LastChange of SpecObject
            _specObject.LastChange = DateTime.Now;

            DialogResult = true;
            Close();
        }

        private void RemoveAttributeButton_Click(object sender, RoutedEventArgs e)
        {
            var dataGridRowIndex = ((sender as Button).BindingGroup.Owner as DataGridRow).GetIndex();
            _attributes[dataGridRowIndex].AttributeValue = null;
            if (_attributes[dataGridRowIndex].added != true)
            {
                _attributes[dataGridRowIndex].removed = true;
                _attributes[dataGridRowIndex].added = false;
            }
            _attributes[dataGridRowIndex].changed = false;

        }

        private void CancelSpecObjectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddAttributeButton_Click(object sender, RoutedEventArgs e)
        {
            var dataGridRowIndex = ((sender as Button).BindingGroup.Owner as DataGridRow).GetIndex();
            AttributeDefinition selectedAttribute = _attributes[dataGridRowIndex].AttributeDefinition;
            AttributeValue attributeValue = null;
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
            attributeValue.SpecElAt = ((Application.Current.MainWindow as MainWindow).FilesCombo.SelectedItem as ReqIF).CoreContent.SpecObjects.SingleOrDefault(x => x.Identifier == _specObject.Identifier);
            _attributes[dataGridRowIndex].AttributeValue = attributeValue;
            _attributes[dataGridRowIndex].added = true;
            _attributes[dataGridRowIndex].removed = false;
        }

    }

}
