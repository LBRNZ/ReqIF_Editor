using Fluent;
using Microsoft.Win32;
using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Media;

namespace ReqIF_Editor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public ReqIF reqif;
        public ReqIFHeader header;
        public ReqIFContent content;
        public SpecObjectsViewModel specObjectsViewModel;
        public List<EmbeddedObject> embeddedObjects;
        public SidePanel Sidepanel = new SidePanel();
        public bool isContenChanged = false;
        public string filePath;

        public MainWindow()
        {
            InitializeComponent();
            SetLanguageDictionary();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Sidepanel.PropertyChanged += SidePanel_PropertyChanged;

            Sidepanel.Expanded = false;
            NavigationButton.DataContext = Sidepanel;
        }


        public void Deserialize(string filepath)
        {
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            reqif = deserializer.Deserialize(filepath);
            header = reqif.TheHeader.FirstOrDefault(); ;
            content = reqif.CoreContent.FirstOrDefault();
            embeddedObjects = reqif.EmbeddedObjects;

            PropertyGrid.DataContext = header;
            specObjectsViewModel = new SpecObjectsViewModel(content);
            specObjectsViewModel.SpecObjects.CollectionChanged += SpecObjects_CollectionChanged;
            foreach (var specObject in specObjectsViewModel.SpecObjects)
            {
                foreach (var attribute in specObject.Values)
                {
                    attribute.PropertyChanged += Attribute_PropertyChanged;
                }
                foreach (var attribute in specObject.Values)
                {
                    if(attribute.AttributeValue != null)
                        attribute.AttributeValue.PropertyChanged += AttributeValue_PropertyChanged;
                }
            }
            initializeColumns();
            MainDataGrid.ItemsSource = specObjectsViewModel.SpecObjects;

        }



        public void ClearDataGrid()
        {
            content = null;
            header = null;
            embeddedObjects = null;
            MainDataGrid.ItemsSource = null;
            MainDataGrid.Columns.Clear();
            MainDataGrid.Items.Refresh();
        }

        #region Actions
        public void Add_SpecObject(string position)
        {
            SpecobjectViewModel specObject = new SpecobjectViewModel()
            {
                Identifier = Guid.NewGuid().ToString(),
                LastChange = DateTime.Now,
            };
            foreach (AttributeDefinition attributeDefinition in content.SpecTypes.First().SpecAttributes)
            {
                specObject.Values.Add(null);
            }
            Edit_SpecObject(specObject, true, position);
        }

        public void Edit_SpecObject(SpecobjectViewModel specObject, bool newSpecObject, string position = null)
        {
            SpecObjectViewerWindow SpecObjectViewer = new SpecObjectViewerWindow(specObject, newSpecObject, position);
            SpecObjectViewer.Owner = Window.GetWindow(this);
            SpecObjectViewer.Show();
        }

        private void ScrollToRow(SpecObject specObject)
        {
            MainDataGrid.SelectedItem = specObject;
            MainDataGrid.ScrollIntoView(specObject);
        }

        private void SearchDocument()
        {
            string searchPhrase = SearchInputBox.Text;
            if (searchPhrase != "")
            {
                List<List<AttributeValue>> listOfValues = content.SpecObjects.Select(x => x.Values).ToList();
                var searchResults = listOfValues.SelectMany(x => x).ToList().Where(s => s.ObjectValue.ToString().Contains(searchPhrase));
                SearchResultsLV.ItemsSource = searchResults;
                NavigationTabControl.SelectedIndex = 1;
            }
        }
        #endregion

        #region Events
        private void Attribute_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            (sender as AttributeValueViewModel).AttributeValue.PropertyChanged += AttributeValue_PropertyChanged;
            (sender as AttributeValueViewModel).AttributeValue.SpecElAt.Values.Add((sender as AttributeValueViewModel).AttributeValue);
        }

        private void AttributeValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            isContenChanged = true;
        }

        private void SpecObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            isContenChanged = true;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Commands.MainWindowCommands.BindCommandsToWindow(this);
        }

        public void SidePanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Expanded")
            {
                if ((sender as SidePanel).Expanded)
                {
                    AnimationHelper.AnimateGridColumnExpandCollapse(SidePanelColumn, true, 300, 0, 200, new TimeSpan(0, 0, 0, 0, 200));
                    SidePanelSeperatorColumn.Width = new GridLength(5);
                }
                else
                {
                    AnimationHelper.AnimateGridColumnExpandCollapse(SidePanelColumn, false, 300, 0, 0, new TimeSpan(0, 0, 0, 0, 200));
                    SidePanelSeperatorColumn.Width = new GridLength(0);
                }
            }

        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Edit_SpecObject((sender as DataGridRow).DataContext as SpecobjectViewModel, false, "");

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
        #endregion

        #region Callbacks
        private void Button_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ReqIF Datei (*.reqif, *.reqifz)|*.reqif; *.reqifz|Alle Dateien (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                ClearDataGrid();
                Deserialize(openFileDialog.FileName);
                //MainDataGrid.ItemsSource = content.SpecObjects;
                NavigationTreeView.ItemsSource = content.Specifications.First().Children;

                Style rowStyle = new Style(typeof(DataGridRow));
                rowStyle.Setters.Add(new EventSetter(DataGridRow.MouseDoubleClickEvent,
                                         new MouseButtonEventHandler(Row_DoubleClick)));
                MainDataGrid.RowStyle = rowStyle;
                filePath = openFileDialog.FileName;
            }
                
        }
        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void Button_NavigationClose_Click(object sender, RoutedEventArgs e)
        {
            Sidepanel.Expanded = false;
        }

        private void Button_SearchDocument_Click(object sender, RoutedEventArgs e)
        {
            SearchDocument();
        }

        private void SearchInputBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchDocument();
            }
        }

        private void SearchResult_Click(object sender, MouseButtonEventArgs e)
        {
            ScrollToRow(((sender as ListViewItem).Content as AttributeValue).SpecElAt as SpecObject);
        }

        private void NavigationTreeItem_Click(object sender, MouseButtonEventArgs e)
        {
            ScrollToRow(((sender as TreeViewItem).DataContext as SpecHierarchy).Object);

        }
        #endregion

        private void initializeColumns()
        {
            var specObjectType = content.SpecTypes.FirstOrDefault(x => x.GetType() == typeof(SpecObjectType));
            int chapterIndex = specObjectType?.SpecAttributes.FindIndex(x => x.LongName == "ReqIF.ChapterName") ?? -1;
            int textIndex = specObjectType?.SpecAttributes.FindIndex(x => x.LongName == "ReqIF.Text") ?? -1;

            int i = 0;
            if (chapterIndex >= 0 && textIndex >= 0)
            {
                DataGridTemplateColumn col = new DataGridTemplateColumn();
                col.Header = FindResource("requirement");

                col.CellTemplateSelector = new HtmlCellTemplateSelector(chapterIndex, textIndex);
                col.Width = 500;
                MainDataGrid.Columns.Add(col);
            }
            foreach (var dataType in content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes) {
                FrameworkElementFactory factory = null;
                DependencyProperty dp = null;
                if (chapterIndex == i || textIndex == i)
                {
                    i++;
                    continue;
                }

                Type typeOfDataType = dataType.DatatypeDefinition.GetType();
                if (typeOfDataType == typeof(DatatypeDefinitionEnumeration))
                {
                    factory = new FrameworkElementFactory(typeof(ListView));
                    dp = ListView.ItemsSourceProperty;
                    //Itemtemplate for EnumValue
                    FrameworkElementFactory subfactory = new FrameworkElementFactory(typeof(TextBlock));
                    subfactory.SetBinding(TextBlock.TextProperty, new Binding("LongName")
                    {
                        Mode = BindingMode.OneWay,
                        NotifyOnSourceUpdated = true,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });
                    DataTemplate dt = new DataTemplate()
                    {
                        VisualTree = subfactory
                    };
                    factory.SetValue(ListView.ItemTemplateProperty, dt);
                }
                else if(typeOfDataType == typeof(DatatypeDefinitionBoolean))
                {
                    factory = new FrameworkElementFactory(typeof(System.Windows.Controls.CheckBox));
                    dp = System.Windows.Controls.CheckBox.IsCheckedProperty;
                    factory.SetValue(System.Windows.Controls.CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                }
                else if (typeOfDataType == typeof(DatatypeDefinitionString))
                {
                    factory = new FrameworkElementFactory(typeof(TextBlock));
                    dp = TextBlock.TextProperty;
                }
                else if (typeOfDataType == typeof(DatatypeDefinitionInteger))
                {
                    factory = new FrameworkElementFactory(typeof(TextBlock));
                    dp = TextBlock.TextProperty;
                }
                else if (typeOfDataType == typeof(DatatypeDefinitionReal))
                {
                    factory = new FrameworkElementFactory(typeof(TextBlock));
                    dp = TextBlock.TextProperty;
                }
                else if (typeOfDataType == typeof(DatatypeDefinitionXHTML))
                {
                    factory = new FrameworkElementFactory(typeof(Html));
                    dp = Html.HtmlProperty;
                }
                else if (typeOfDataType == typeof(DatatypeDefinitionDate))
                {
                    factory = new FrameworkElementFactory(typeof(TextBlock));
                    dp = TextBlock.TextProperty;
                }
                factory.SetValue(IsEnabledProperty, false);
                var binding = new Binding("Values[" + i++ + "].AttributeValue.ObjectValue");
                binding.Mode = BindingMode.OneWay;
                if (typeOfDataType == typeof(DatatypeDefinitionXHTML))
                {
                    binding.Converter = new XHTMLConverter();
                }
                factory.SetBinding(dp, binding);

                DataGridTemplateColumn dataGridTemplateColumn = new DataGridTemplateColumn()
                {
                    Header = dataType.LongName,
                    CellTemplate = new DataTemplate() { VisualTree = factory }
                };
                MainDataGrid.Columns.Add(dataGridTemplateColumn);
            }
        }

        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToString())
            {
                case "en":
                    dict.Source = new Uri("..\\Resources\\en.xaml", UriKind.Relative);
                    RibbonLocalization.Current.Culture = new System.Globalization.CultureInfo("en");
                    break;
                case "de":
                    dict.Source = new Uri("..\\Resources\\de.xaml", UriKind.Relative);
                    RibbonLocalization.Current.Culture = new System.Globalization.CultureInfo("de");
                    break;
                case "es":
                    dict.Source = new Uri("..\\Resources\\es.xaml", UriKind.Relative);
                    RibbonLocalization.Current.Culture = new System.Globalization.CultureInfo("es");
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\en.xaml", UriKind.Relative);
                    RibbonLocalization.Current.Culture = new System.Globalization.CultureInfo("en");
                    break;
            }
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }

    class Html : TinyHtml.Wpf.WpfHtmlControl
    {
        public Html()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ReqIF_Editor.Resources.htmlViewer.css"))
            {
                using (StreamReader reader = new StreamReader(s))
                {
                    SetMasterStylesheet(reader.ReadToEnd());
                }
            }
        }
        protected override Stream OnLoadResource(string url)
        {
            MemoryStream memoryStream = new MemoryStream();
            var embeddedObject = ((MainWindow)System.Windows.Application.Current.MainWindow).embeddedObjects.Find(x => x.ImageName == url);
            if(embeddedObject != null)
            {
                embeddedObject.PreviewImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                return memoryStream;
            } else
            {
                Properties.Resources.Document_notFound.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                return memoryStream;
            }
        }
    }

    public class HtmlCellTemplateSelector : DataTemplateSelector
    {
        private int _chapterIndex;
        private int _textIndex;
        public HtmlCellTemplateSelector(int chapterIndex, int textIndex)
        {
            _chapterIndex = chapterIndex;
            _textIndex = textIndex;
        }

        public override DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            ContentPresenter presenter = container as ContentPresenter;
            DataGridCell cell = presenter.Parent as DataGridCell;
            Binding binding;

            if ((cell.DataContext as SpecobjectViewModel).Values.SingleOrDefault(x => x?.AttributeDefinition.LongName == "ReqIF.ChapterName").AttributeValue != null)
            {
                binding = new Binding("Values[" + _chapterIndex + "].AttributeValue.ObjectValue")
                {
                    Mode = BindingMode.OneWay,
                    NotifyOnSourceUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Converter = new XHTMLConverter(),
                    ConverterParameter = "heading"
                };
            } else {
                binding = new Binding("Values[" + _textIndex + "].AttributeValue.ObjectValue")
                {
                    Mode = BindingMode.OneWay,
                    NotifyOnSourceUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Converter = new XHTMLConverter()
                };

            }
            var factory = new FrameworkElementFactory(typeof(Html));
            factory.SetBinding(Html.HtmlProperty, binding);

            DataTemplate cellTemplate = new DataTemplate()
            {
                VisualTree = factory
            };
            return cellTemplate;

        }

    }

    public class SidePanel : INotifyPropertyChanged
    {
        private bool expanded;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Expanded
        {
            get { return expanded; }
            set
            {
                if (value != expanded)
                {
                    expanded = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
