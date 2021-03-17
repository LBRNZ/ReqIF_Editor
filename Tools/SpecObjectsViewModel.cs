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
    class SpecObjectsViewModel
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
                    LongName = specObject.LongName
                };
                foreach (AttributeDefinition attributeDefinition in content.SpecTypes.First().SpecAttributes)
                {
                    AttributeValue attributeValue = specObject.Values.Where(x => x.AttributeDefinition == attributeDefinition).FirstOrDefault();
                    if(attributeValue?.GetType() == typeof(AttributeValueXHTML))
                    {
                        var assembly = Assembly.GetExecutingAssembly();
                        string removeNamespaces =((string)attributeValue.ObjectValue).xslTransform(XmlReader.Create(assembly.GetManifestResourceStream("ReqIF_Editor.XSLT.NamespaceTrimmer.xslt")));
                        string ObjectToImg = removeNamespaces.xslTransform(XmlReader.Create(assembly.GetManifestResourceStream("ReqIF_Editor.XSLT.ObjectToImg.xslt")));
                        attributeValue.ObjectValue = ObjectToImg;
                    }
                    specobjectViewModel.Values.Add(attributeValue);
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
    public class SpecobjectViewModel : Identifiable, INotifyPropertyChanged
    {
        private ObservableCollection<AttributeValue> values = new ObservableCollection<AttributeValue>();

        public ObservableCollection<AttributeValue> Values
        {
            get
            {
                return this.values;
            }
            set
            {
                if (values == value)
                    return;
                values = value;
                NotifyPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
