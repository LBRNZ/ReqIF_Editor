using ReqIF_Editor.Windows;
using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.CSharp;

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
        private Dictionary<AttributeDefinition, AttributeValue> _attributes;
        public SpecObjectViewerWindow(SpecobjectViewModel specObject, bool newSpecObject, string position = null)
        {
            InitializeComponent();
            _attributes = new Dictionary<AttributeDefinition, AttributeValue>();
            int i = 0;
            foreach(var value in specObject.Values)
            {
                var key = (Application.Current.MainWindow as MainWindow).content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes[i++];
                _attributes.Add(key, value);
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
                //    int currentIndex = 0;
                //    SpecObject currentObject = (Application.Current.MainWindow as MainWindow).MainDataGrid.SelectedItem as SpecObject;
                //    var specifications = (Application.Current.MainWindow as MainWindow).content.Specifications;

                //    //Add SpecObject to SpecHierarchy and to SpecObjects
                //    SpecHierarchy specHierarchy = specifications.First().Children.First().Descendants()
                //        .Where(node => node.Object == currentObject).First();
                //    if (_position == "after")
                //    {
                //        SpecHierarchy parentSpecHierarchy = specHierarchy.Container;
                //        int specHierarchyIndex = parentSpecHierarchy.Children.IndexOf(specHierarchy);
                //        parentSpecHierarchy.Children.Insert(specHierarchyIndex + 1, new SpecHierarchy()
                //        {
                //            Object = _specObject,
                //            Identifier = Guid.NewGuid().ToString(),
                //            LastChange = DateTime.Now
                //        });
                //        var previousObject = specHierarchy.Descendants().Last().Object;
                //        currentIndex = (Application.Current.MainWindow as MainWindow).content.SpecObjects.IndexOf(previousObject);
                //    }
                //    else if (_position == "under")
                //    {
                //        specHierarchy.Children.Insert(0, new SpecHierarchy()
                //        {
                //            Object = _specObject,
                //            Identifier = Guid.NewGuid().ToString(),
                //            LastChange = DateTime.Now
                //        });
                //        currentIndex = (Application.Current.MainWindow as MainWindow).MainDataGrid.SelectedIndex;

                //    }
                //    (Application.Current.MainWindow as MainWindow).content.SpecObjects.Insert(currentIndex + 1, _specObject);
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
            AttributeDefinition selectedAttribute = ((sender as Button).DataContext as dynamic).Key as AttributeDefinition;
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
            _attributes[selectedAttribute] = attributeValue;
            DataTable.Items.Refresh();
        }

    }
}
