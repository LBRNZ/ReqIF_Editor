using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace ReqIF_Editor
{
    static class Extensions
    {
        public static IEnumerable<SpecHierarchy> Descendants(this SpecHierarchy root)
        {
            var nodes = new Stack<SpecHierarchy>(new[] { root });
            while (nodes.Any())
            {
                SpecHierarchy node = nodes.Pop();
                yield return node;
                for (int i = node.Children.Count - 1; i >= 0; i--)
                {
                    nodes.Push(node.Children[i]);
                }

            }
        }

        public static string xslTransform(this string str, XmlReader xslt)
        {
            XslCompiledTransform xslct = new XslCompiledTransform();
            xslct.Load(xslt);
            using (MemoryStream msInput = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(str)))
            {
                XPathDocument xpathdocument = new XPathDocument(msInput);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (XmlTextWriter writer = new XmlTextWriter(ms, new UTF8Encoding(false)))
                    {
                        writer.Formatting = Formatting.Indented;

                        xslct.Transform(xpathdocument, null, writer, null);
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
    public static class VisualTreeHelperExtension
    {
        struct StackElement
        {
            public FrameworkElement Element { get; set; }
            public int Position { get; set; }
        }
        public static IEnumerable<FrameworkElement> FindAllVisualDescendants(this FrameworkElement parent)
        {
            if (parent == null)
                yield break;
            Stack<StackElement> stack = new Stack<StackElement>();
            int i = 0;
            while (true)
            {
                if (i < VisualTreeHelper.GetChildrenCount(parent))
                {
                    FrameworkElement child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                    if (child != null)
                    {
                        if (child != null)
                            yield return child;
                        stack.Push(new StackElement { Element = parent, Position = i });
                        parent = child;
                        i = 0;
                        continue;
                    }
                    ++i;
                }
                else
                {
                    // back at the root of the search
                    if (stack.Count == 0)
                        yield break;
                    StackElement element = stack.Pop();
                    parent = element.Element;
                    i = element.Position;
                    ++i;
                }
            }
        }
    }
}
