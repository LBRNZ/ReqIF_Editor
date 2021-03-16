using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ReqIFSharp;

namespace ReqIF_Editor
{
    class SpecObjectsViewModel
    {
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
