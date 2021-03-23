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
        private bool _newSpecObject;
        private SpecobjectViewModel _specObject;
        private string _position;
        private ObservableCollection<AttributeValueViewModel> _attributes;
        private List<AttributeDefinition> _newAttributeValues;
        public SpecObjectViewerWindow(SpecobjectViewModel specObject, bool newSpecObject, string position = null)
        {
            InitializeComponent();

            _newAttributeValues = new List<AttributeDefinition>();

            //Create temporary collection of attributes
            _attributes = new ObservableCollection<AttributeValueViewModel>();
            foreach (var value in specObject.Values)
            {
                if(value.AttributeValue != null)
                {
                    _attributes.Add(value);
                } else
                {
                    _attributes.Add(new AttributeValueViewModel()
                    {
                        AttributeValue = null,
                        AttributeDefinition = value.AttributeDefinition
                    });
                }
            }
            DataTable.ItemsSource = _attributes;
            InfoExpander.DataContext = specObject;
            _newSpecObject = newSpecObject;
            _specObject = specObject;
            _position = position;
        }


        private void SaveSpecObject_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_newSpecObject)
            {
                int currentIndex = 0;
                SpecObject currentObject = (Application.Current.MainWindow as MainWindow).MainDataGrid.SelectedItem as SpecObject;
                var specifications = (Application.Current.MainWindow as MainWindow).content.Specifications;

                //Create new SpecObject and add Attributes
                SpecObject newSpecObject = new SpecObject()
                {
                    Description = _specObject.Description,
                    Identifier = _specObject.Identifier,
                    LastChange = _specObject.LastChange

                };
                foreach (var attribute in _attributes)
                {
                    newSpecObject.Values.Add(attribute.AttributeValue);
                }

                    //Add SpecObject to SpecHierarchy and to SpecObjects
                //SpecHierarchy specHierarchy = specifications.First().Children.First().Descendants()
                //.Where(node => node.Object == currentObject).First();
                //if (_position == "after")
                //{
                //    SpecHierarchy parentSpecHierarchy = specHierarchy.Container;
                //    int specHierarchyIndex = parentSpecHierarchy.Children.IndexOf(specHierarchy);
                //    parentSpecHierarchy.Children.Insert(specHierarchyIndex + 1, new SpecHierarchy()
                //    {
                //        Object = _specObject,
                //        Identifier = Guid.NewGuid().ToString(),
                //        LastChange = DateTime.Now
                //    });
                //    var previousObject = specHierarchy.Descendants().Last().Object;
                //    currentIndex = (Application.Current.MainWindow as MainWindow).content.SpecObjects.IndexOf(previousObject);
                //}
                //else if (_position == "under")
                //{
                //    specHierarchy.Children.Insert(0, new SpecHierarchy()
                //    {
                //        Object = _specObject,
                //        Identifier = Guid.NewGuid().ToString(),
                //        LastChange = DateTime.Now
                //    });
                //    currentIndex = (Application.Current.MainWindow as MainWindow).MainDataGrid.SelectedIndex;

                //}
                    (Application.Current.MainWindow as MainWindow).specObjectsViewModel.SpecObjects.Insert(currentIndex + 1, _specObject);
            }

            // Add new AttributeValues to SpecObject
            foreach(var definition in _newAttributeValues)
            {
                _specObject.Values.Single(x => x.AttributeDefinition == definition).AttributeValue = _attributes.Single(x => x.AttributeDefinition == definition).AttributeValue;
            }

            // Update binding sources
            var dataFields = DataTable.FindAllVisualDescendants()
                .Where(elt => elt.Name == "dataField");
            foreach (var dataField in dataFields)
            {
                BindingExpression binding = null;
                if ((dataField.DataContext as AttributeValue).GetType() == typeof(AttributeValueBoolean)){
                    binding = dataField.GetBindingExpression(CheckBox.IsCheckedProperty);
                }
                else if((dataField.DataContext as AttributeValue).GetType() == typeof(AttributeValueXHTML))
                {
                    binding = dataField.GetBindingExpression(HtmlEditor.BindingContentProperty);
                }
                else if ((dataField.DataContext as AttributeValue).GetType() == typeof(AttributeValueEnumeration))
                {
                    binding = dataField.GetBindingExpression(ListBox.SelectedItemProperty);
                }
                else if ((dataField.DataContext as AttributeValue).GetType() == typeof(AttributeValueString))
                {
                    binding = dataField.GetBindingExpression(TextBox.TextProperty);
                }
                else if ((dataField.DataContext as AttributeValue).GetType() == typeof(AttributeValueDate))
                {
                    binding = dataField.GetBindingExpression(DateTimePicker.ValueProperty);
                }
                else if ((dataField.DataContext as AttributeValue).GetType() == typeof(AttributeValueInteger))
                {
                    binding = dataField.GetBindingExpression(LongUpDown.ValueProperty);
                }
                else if ((dataField.DataContext as AttributeValue).GetType() == typeof(AttributeValueReal))
                {
                    binding = dataField.GetBindingExpression(DoubleUpDown.ValueProperty);
                }
                binding.UpdateSource();
            }

            //Update LastChange of SpecObject
            _specObject.LastChange = DateTime.Now;
            Close();
        }
        

        private void CancelSpecObjectButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddSpecObjectButton_Click(object sender, RoutedEventArgs e)
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
            attributeValue.SpecElAt = (Application.Current.MainWindow as MainWindow).content.SpecObjects.Single(x => x.Identifier == _specObject.Identifier);
            _attributes[dataGridRowIndex].AttributeValue = attributeValue;
            _newAttributeValues.Add(selectedAttribute);
        }

    }

}
