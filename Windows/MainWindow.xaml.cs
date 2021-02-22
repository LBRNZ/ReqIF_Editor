using Fluent;
using Microsoft.Win32;
using ReqIFSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;

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
        public List<EmbeddedObject> embeddedObjects;
        public SidePanel Sidepanel = new SidePanel();
        public bool isContenChanged = false;
        public string filePath;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Sidepanel.PropertyChanged += SidePanel_PropertyChanged;

            Sidepanel.Expanded = false;
            NavigationButton.DataContext = Sidepanel;
        }

        private void SpecObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            isContenChanged = true;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Commands.MainWindowCommands.BindCommandsToWindow(this);
        }
        public void Deserialize(string filepath)
        {
            ReqIFDeserializer deserializer = new ReqIFDeserializer();
            reqif = deserializer.Deserialize(filepath);
            header = reqif.TheHeader.FirstOrDefault(); ;
            content = reqif.CoreContent.FirstOrDefault();
            embeddedObjects = reqif.EmbeddedObjects;
            content.SpecObjects.CollectionChanged += SpecObjects_CollectionChanged;

            PropertyGrid.DataContext = header;
            initializeColumns();
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
        #region Callbacks
        private void Button_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ReqIF Datei (*.reqif, *.reqifz)|*.reqif; *.reqifz|Alle Dateien (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                ClearDataGrid();
                Deserialize(openFileDialog.FileName);
                MainDataGrid.ItemsSource = content.SpecObjects;
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
            bool chapterNameExists = content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes.Where(x => x.LongName == "ReqIF.ChapterName").Any();
            bool textExists = content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes.Where(x => x.LongName == "ReqIF.Text").Any();


            if (chapterNameExists && textExists)
            {
                DataGridTemplateColumn col = new DataGridTemplateColumn();
                FrameworkElementFactory factory = null;
                col.Header = "Anforderung";
                MultiBinding mb = new MultiBinding()
                {
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                };
                mb.Converter = new TextAndChapterNameConverter();

                Binding chapterNameBinding = new Binding("Values")
                {
                    Converter = new SpecObjectValueConverter(content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes.Where(x => x.LongName == "ReqIF.ChapterName").FirstOrDefault().Identifier)
                };

                Binding textBinding = new Binding("Values")
                {
                    Converter = new SpecObjectValueConverter(content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes.Where(x => x.LongName == "ReqIF.Text").FirstOrDefault().Identifier)
                };

                mb.Bindings.Add(chapterNameBinding);
                mb.Bindings.Add(textBinding);
                factory = new FrameworkElementFactory(typeof(Html));
                factory.SetBinding(Html.HtmlProperty, mb);
                //factory.SetValue(Html.FontSizePropertyProperty, 20D);
                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = factory;
                col.CellTemplate = cellTemplate;
                col.Width = 500;
                MainDataGrid.Columns.Add(col);
            }
            foreach (var dataType in content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes) {
                DataGridTemplateColumn col = new DataGridTemplateColumn();
                FrameworkElementFactory factory = null;
                DependencyProperty dp = null;
                col.Header = dataType.LongName;
                if (chapterNameExists && textExists &&(dataType.LongName == "ReqIF.ChapterName" || dataType.LongName == "ReqIF.Text"))
                {
                    continue;
                }

                Type typeOfDataType = dataType.DatatypeDefinition.GetType();
                if (typeOfDataType == typeof(DatatypeDefinitionEnumeration))
                {
                    factory = new FrameworkElementFactory(typeof(ListView));
                    dp = ListView.ItemsSourceProperty;
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
                factory.SetBinding(dp, new Binding("Values")
                {
                    Mode = BindingMode.OneWay,
                    Converter = new SpecObjectValueConverter(dataType.Identifier),
                    NotifyOnSourceUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = factory;
                col.CellTemplate = cellTemplate;
                MainDataGrid.Columns.Add(col);
            }
        }

        public void Edit_SpecObject(SpecObject specObject, bool newSpecObject)
        {
            SpecObjectViewerWindow SpecObjectViewer = new SpecObjectViewerWindow(specObject, newSpecObject);
            SpecObjectViewer.Owner = Window.GetWindow(this);
            SpecObjectViewer.Show();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Edit_SpecObject((sender as DataGridRow).DataContext as SpecObject, false);

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

        public void SidePanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Expanded")
            {
                if((sender as SidePanel).Expanded)
                {
                    AnimationHelper.AnimateGridColumnExpandCollapse(SidePanelColumn, true, 300, 0, 0, new TimeSpan(0, 0, 0, 0, 300));
                    SidePanelSeperatorColumn.Width = new GridLength(5);
                } else
                {
                    AnimationHelper.AnimateGridColumnExpandCollapse(SidePanelColumn, false, 300, 0, 0, new TimeSpan(0, 0, 0, 0, 300));
                    SidePanelSeperatorColumn.Width = new GridLength(0);
                }
            }

        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }

    class Html : TinyHtml.Wpf.WpfHtmlControl
    {
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
                return null;
            }
        }
    }
    public class SidePanel : INotifyPropertyChanged
    {
        private bool expanded;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public bool Expanded
        {
            get { return expanded; }
            set
            {
                if (value != expanded)
                {
                    expanded = value;
                    OnPropertyChanged("Expanded");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
