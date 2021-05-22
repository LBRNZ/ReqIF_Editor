using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace ReqIF_Editor
{
    public class XHTMLConverter : IValueConverter
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(string))
            {
                string html = (string)value;
                string removeNamespaces = html.xslTransform(XmlReader.Create(assembly.GetManifestResourceStream("ReqIF_Editor.XSLT.NamespaceTrimmer.xslt")));
                string ObjectToImg = removeNamespaces.xslTransform(XmlReader.Create(assembly.GetManifestResourceStream("ReqIF_Editor.XSLT.ObjectToImg.xslt")));
                if((string)parameter == "heading")
                {
                    ObjectToImg = ObjectToImg.xslTransform(XmlReader.Create(assembly.GetManifestResourceStream("ReqIF_Editor.XSLT.AddClassToDiv.xslt")));
                }
                return ObjectToImg;
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(string))
            {
                string html = (string)value;
                //Change Html Tags to lower case
                html = Regex.Replace(html, "<([^>]*)>", m => "<" + m.Groups[1].Value.ToLower() + ">");
                //Replace <br> with <br />
                html = Regex.Replace(html, "<(br[^>]*)>", "<br />");
                
                string addNamespace = html.xslTransform(XmlReader.Create(assembly.GetManifestResourceStream("ReqIF_Editor.XSLT.AddNamespace.xslt")));
                string ImgToObject = html.xslTransform(XmlReader.Create(assembly.GetManifestResourceStream("ReqIF_Editor.XSLT.ImgToObject.xslt")));
                return ImgToObject;
            }
            else return null;
        }

    }


    public class TextAndChapterNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] != null)
            {
                return values[0];
            }
            else
            {
                return values[1];
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class HierarchyObjectValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(SpecObject))
            {
                var headerObject = ((SpecObject)value).Values.Where(x => x.AttributeDefinition.LongName == "ReqIF.ChapterName").FirstOrDefault();
                if(headerObject != null)
                {
                    return XDocument.Parse(headerObject.ObjectValue.ToString()).Root.Value;
                }
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class SpecificationChildrenValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(List<SpecHierarchy>))
            {
                //Only return SpecObjects that contain a heading
                return (value as List<SpecHierarchy>).Where(x => x.Object.Values.Where(a => a.AttributeDefinition.LongName == "ReqIF.ChapterName").Any()).ToList();
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class SearchResultObjectValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType().BaseType == typeof(AttributeValue))
            {
                var searchResultObject = ((AttributeValue)value).ObjectValue;
                if (searchResultObject != null)
                {
                    return XDocument.Parse(searchResultObject.ToString()).Root.Value;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class AttributeEditableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(Boolean))
            {
                if (Properties.Settings.Default.overrideReqifRights == true)
                {
                    return true;
                } else
                {
                    return value;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
