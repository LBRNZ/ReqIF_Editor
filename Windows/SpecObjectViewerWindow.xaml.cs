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
        private ObservableCollection<SpecObjectValueModel> _attributes;
        public SpecObjectViewerWindow(SpecobjectViewModel specObject, bool newSpecObject, string position = null)
        {
            InitializeComponent();
            _attributes = new ObservableCollection<SpecObjectValueModel>();
            int i = 0;
            foreach(var value in specObject.Values)
            {
                var definition = (Application.Current.MainWindow as MainWindow).content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes[i++];
                _attributes.Add(new SpecObjectValueModel()
                {
                    AttributeDefinition = definition,
                    AttributeValue = value,
                    hasChanged = false
                });
            }
            //Register property changed event for every value
            foreach (var attribute in _attributes)
            {
                attribute.PropertyChanged += AttributeValue_PropertyChanged;
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
                    (Application.Current.MainWindow as MainWindow).contenModel.SpecObjects.Insert(currentIndex + 1, _specObject);
            }
            int i = 0;
            foreach(var attribute in _attributes)
            {
                if (attribute.AttributeValue != null)
                {
                    _specObject.Values[i] = attribute.AttributeValue;
                    _specObject.LastChange = DateTime.Now;
                }
                i++;
            }

            Close();
        }

        private void AttributeValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(sender.GetType() == typeof(SpecObjectValueModel))
            {
                (sender as SpecObjectValueModel).hasChanged = true;
            }
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
            AttributeDefinition selectedAttribute = ((sender as Button).DataContext as SpecObjectValueModel).AttributeDefinition;
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
            _attributes.Single(x => x.AttributeDefinition == selectedAttribute).AttributeValue = attributeValue;
            DataTable.Items.Refresh();
        }

    }

    class SpecObjectValueModel : INotifyPropertyChanged
    {
        private AttributeDefinition _attributeDefinition;
        private AttributeValue _attributeValue;
        private bool _hasChanged;

        public AttributeDefinition AttributeDefinition
        {
            get { return _attributeDefinition; }
            set { _attributeDefinition = value;}
        }
        public AttributeValue AttributeValue
        {
            get { return _attributeValue; }
            set { _attributeValue = value; NotifyPropertyChanged(); }
        }
        public bool hasChanged
        {
            get { return _hasChanged; }
            set { _hasChanged = value;}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
