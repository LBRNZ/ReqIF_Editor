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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
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
            SetLanguageDictionary();
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
            var test = new SpecObjectsViewModel(content);
            initializeColumns();
            MainDataGrid.ItemsSource = test.SpecObjects;

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
                    subfactory.SetBinding(TextBlock.TextProperty, new Binding("LongName"));
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
                factory.SetBinding(dp, new Binding("Values[" + i++ + "].ObjectValue")
                {
                    Mode = BindingMode.OneWay,
                    NotifyOnSourceUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                DataGridTemplateColumn dataGridTemplateColumn = new DataGridTemplateColumn()
                {
                    Header = dataType.LongName,
                    CellTemplate = new DataTemplate() { VisualTree = factory }
                };
                MainDataGrid.Columns.Add(dataGridTemplateColumn);
            }
        }

        public void Add_SpecObject(string position)
        {
            //SpecobjectViewModel specObject = new SpecobjectViewModel()
            //{
            //    Identifier = Guid.NewGuid().ToString(),
            //    LastChange = DateTime.Now,
            //    ReqIfContent = content,
            //    SpecType = content.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault()
            //};
            //Edit_SpecObject(specObject, true, position);
        }

        public void Edit_SpecObject(SpecobjectViewModel specObject, bool newSpecObject, string position = null)
        {
            SpecObjectViewerWindow SpecObjectViewer = new SpecObjectViewerWindow(specObject, newSpecObject, position);
            SpecObjectViewer.Owner = Window.GetWindow(this);
            SpecObjectViewer.Show();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Edit_SpecObject((sender as DataGridRow).DataContext as SpecobjectViewModel, false, "");

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
                    AnimationHelper.AnimateGridColumnExpandCollapse(SidePanelColumn, true, 300, 0, 200, new TimeSpan(0, 0, 0, 0, 200));
                    SidePanelSeperatorColumn.Width = new GridLength(5);
                } else
                {
                    AnimationHelper.AnimateGridColumnExpandCollapse(SidePanelColumn, false, 300, 0, 0, new TimeSpan(0, 0, 0, 0, 200));
                    SidePanelSeperatorColumn.Width = new GridLength(0);
                }
            }

        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
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
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ReqIF_Editor.Resources.text.css"))
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

    class HtmlHeading : TinyHtml.Wpf.WpfHtmlControl
    {
        public HtmlHeading()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ReqIF_Editor.Resources.heading.css"))
            {
                using (StreamReader reader = new StreamReader(s))
                {
                    SetMasterStylesheet(reader.ReadToEnd());
                }
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

            if ((cell.DataContext as SpecobjectViewModel).Values.SingleOrDefault(x => x?.AttributeDefinition.LongName == "ReqIF.ChapterName") != null)
            {
                Binding binding = new Binding("Values[" + _chapterIndex + "].TheValue")
                {
                    //Converter = new SpecObjectValueConverter((cell.DataContext as SpecObject).SpecType.SpecAttributes.Where(x => x.LongName == "ReqIF.ChapterName").FirstOrDefault().Identifier),
                    Mode = BindingMode.OneWay,
                    NotifyOnSourceUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                var factory = new FrameworkElementFactory(typeof(HtmlHeading));
                factory.SetBinding(HtmlHeading.HtmlProperty, binding);

                DataTemplate cellTemplate = new DataTemplate()
                {
                    VisualTree = factory
                };
                return cellTemplate;
            } else {
                Binding binding = new Binding("Values[" + _textIndex + "].TheValue")
                {
                    //Converter = new SpecObjectValueConverter((cell.DataContext as SpecObject).SpecType.SpecAttributes.Where(x => x.LongName == "ReqIF.Text").FirstOrDefault().Identifier),
                    Mode = BindingMode.OneWay,
                    NotifyOnSourceUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                var factory = new FrameworkElementFactory(typeof(Html));
                factory.SetBinding(Html.HtmlProperty, binding);

                DataTemplate cellTemplate = new DataTemplate()
                {
                    VisualTree = factory
                };
                return cellTemplate;
            }
            
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
