using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using ReqIFSharp;

namespace ReqIF_Editor
{
    public class SpecObjectsViewModel
    {

        public SpecObjectsViewModel(ReqIFContent content)
        {
            foreach (SpecObject specObject in content.SpecObjects)
            {
                SpecobjectViewModel specobjectViewModel = new SpecobjectViewModel()
                {
                    Identifier = specObject.Identifier,
                    AlternativeId = specObject.AlternativeId,
                    Description = specObject.Description,
                    LastChange = specObject.LastChange,
                    LongName = specObject.LongName,
                    Type = specObject.Type,
                    SpecType = specObject.SpecType
                };
                foreach (AttributeDefinition attributeDefinition in content.SpecTypes.First().SpecAttributes)
                {
                    AttributeValue attributeValue = specObject.Values.Where(x => x.AttributeDefinition == attributeDefinition).FirstOrDefault();

                    specobjectViewModel.Values.Add(new AttributeValueViewModel() { AttributeValue = attributeValue, AttributeDefinition = attributeDefinition });
                }
                this.SpecObjects.Add(specobjectViewModel);
            }
        }

        private readonly ObservableCollection<SpecobjectViewModel> specObjects = new ObservableCollection<SpecobjectViewModel>();

        /// <summary>
        /// Gets the <see cref="SpecobjectViewModel"/>
        /// </summary>
        public ObservableCollection<SpecobjectViewModel> SpecObjects
        {
            get
            {
                return this.specObjects;
            }
        }
    }
    public class SpecobjectViewModel : SpecObject
    {
        private ObservableCollection<AttributeValueViewModel> values = new ObservableCollection<AttributeValueViewModel>();

        public new ObservableCollection<AttributeValueViewModel> Values
        {
            get
            {
                return this.values;
            }
        }
    }
    public class AttributeValueViewModel : INotifyPropertyChanged
    {
        private AttributeDefinition _attributeDefinition;
        private AttributeValue _attributeValue;

        public AttributeDefinition AttributeDefinition
        {
            get { return _attributeDefinition; }
            set { _attributeDefinition = value; }
        }
        public AttributeValue AttributeValue
        {
            get { return _attributeValue; }
            set { _attributeValue = value; NotifyPropertyChanged(); }
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
