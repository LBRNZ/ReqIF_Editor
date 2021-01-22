﻿using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace ReqIF_Editor
{
    public class SpecObjectValueConverter : IValueConverter
    {
        private string _Identifier;
        public SpecObjectValueConverter(string Identifier)
        {
            _Identifier = Identifier;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(List<ReqIFSharp.AttributeValue>))
            {
                List<ReqIFSharp.AttributeValue> values = (List<ReqIFSharp.AttributeValue>)value;
                if (values.Where(x => x.AttributeDefinition.Identifier == _Identifier).FirstOrDefault() != null)
                    if (values.Where(x => x.AttributeDefinition.Identifier == _Identifier).FirstOrDefault().ObjectValue.GetType() == typeof(List<EnumValue>))
                    {
                        return ((List<EnumValue>)values.Where(x => x.AttributeDefinition.Identifier == _Identifier).FirstOrDefault().ObjectValue).Select(x => x.LongName);
                    }
                    else if (values.Where(x => x.AttributeDefinition.Identifier == _Identifier).FirstOrDefault().AttributeDefinition.GetType() == typeof(AttributeDefinitionXHTML))
                    {
                        string removeNamespaces = transformXHTML((string)values.Where(x => x.AttributeDefinition.Identifier == _Identifier).FirstOrDefault().ObjectValue, "ReqIF_Editor.XSLT.NamespaceTrimmer.xslt");
                        string ObjectToImg = transformXHTML(removeNamespaces, "ReqIF_Editor.XSLT.ObjectToImg.xslt");
                        return ObjectToImg;
                    }
                    else return values.Where(x => x.AttributeDefinition.Identifier == _Identifier).FirstOrDefault().ObjectValue;
                else return null;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

        private string transformXHTML(string input, string xsltSource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            XslTransform xslt = new XslTransform();
            xslt.Load(XmlReader.Create(assembly.GetManifestResourceStream(xsltSource)));
            using (MemoryStream msInput = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input)))
            {
                XPathDocument xpathdocument = new XPathDocument(msInput);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (XmlTextWriter writer = new XmlTextWriter(ms, new UTF8Encoding(false)))
                    {
                        writer.Formatting = Formatting.Indented;

                        xslt.Transform(xpathdocument, null, writer, null);
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }

    public class XHTMLConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(string))
            {
                string html = (string)value;
                string removeNamespaces = transformXHTML(html, "ReqIF_Editor.XSLT.NamespaceTrimmer.xslt");
                string ObjectToImg = transformXHTML(removeNamespaces, "ReqIF_Editor.XSLT.ObjectToImg.xslt");
                return ObjectToImg;
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == typeof(string))
            {
                string html = (string)value;
                string addNamespace = transformXHTML(html, "ReqIF_Editor.XSLT.AddNamespace.xslt");
                string ImgToObject = transformXHTML(addNamespace, "ReqIF_Editor.XSLT.ImgToObject.xslt");
                return ImgToObject;
            }
            else return null;
        }

        private string transformXHTML(string input, string xsltSource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            XslTransform xslt = new XslTransform();
            xslt.Load(XmlReader.Create(assembly.GetManifestResourceStream(xsltSource)));
            using (MemoryStream msInput = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input)))
            {
                XPathDocument xpathdocument = new XPathDocument(msInput);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (XmlTextWriter writer = new XmlTextWriter(ms, new UTF8Encoding(false)))
                    {
                        writer.Formatting = Formatting.Indented;

                        xslt.Transform(xpathdocument, null, writer, null);
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
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
}
