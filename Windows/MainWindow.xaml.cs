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
using ReqIF_Editor.TreeDataGrid;

namespace ReqIF_Editor
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow, INotifyPropertyChanged
    {
        private IEnumerable<ReqIF> _reqif;
        public IEnumerable<ReqIF> reqif {
            get => this._reqif;
            set
            {
                _reqif = value;
                NotifyPropertyChanged();
            }
        }

        private GridDef _Source;
        public GridDef Source
        {
            get => this._Source;
            set
            {
                _Source = value;
                NotifyPropertyChanged();
            }
        }

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
            header = reqif.First().TheHeader;
            content = reqif.First().CoreContent;
            embeddedObjects = reqif.First().EmbeddedObjects;

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
            //initializeColumns();
            //MainDataGrid.ItemsSource = specObjectsViewModel.SpecObjects;
        }



        public void ClearDataGrid()
        {
            content = null;
            header = null;
            embeddedObjects = null;
            //MainDataGrid.ItemsSource = null;
            MainDataGrid.Columns.Clear();
            MainDataGrid.Items.Refresh();
        }

        #region Actions
        public void Add_SpecObject(string position)
        {
            SpecobjectViewModel specObject = new SpecobjectViewModel()
            {
                Identifier = Guid.NewGuid().ToString(),
                LastChange = DateTime.Now
            };
            foreach (AttributeDefinition attributeDefinition in content.SpecTypes.First().SpecAttributes)
            {
                specObject.Values.Add(new AttributeValueViewModel()
                {
                    AttributeValue = null,
                    AttributeDefinition = attributeDefinition
                });
            }
            Edit_SpecObject(specObject, true, position);
        }

        public void Edit_SpecObject(SpecobjectViewModel specObject, bool createSpecObject, string position = null)
        {
            SpecObjectViewerWindow SpecObjectViewer = new SpecObjectViewerWindow(specObject);
            SpecObjectViewer.Owner = Window.GetWindow(this);
            if (SpecObjectViewer.ShowDialog() == true)
            {
                if (createSpecObject)
                {
                    int currentIndex = 0;
                    SpecobjectViewModel currentModelObject = (Application.Current.MainWindow as MainWindow).MainDataGrid.SelectedItem as SpecobjectViewModel;
                    SpecObject currentObject = (Application.Current.MainWindow as MainWindow).content.SpecObjects.Single(x => x.Identifier == currentModelObject.Identifier);
                    var specifications = (Application.Current.MainWindow as MainWindow).content.Specifications;

                    //Create new SpecObject and add Attributes
                    SpecObject newSpecObject = new SpecObject()
                    {
                        Description = specObject.Description,
                        Identifier = specObject.Identifier,
                        LastChange = specObject.LastChange,
                        Type = currentObject.Type

                    };
                    foreach (var attribute in specObject.Values)
                    {
                        if (attribute.AttributeValue != null)
                            newSpecObject.Values.Add(attribute.AttributeValue);
                    }

                    //Add SpecObject to SpecHierarchy and to SpecObjects
                    SpecHierarchy specHierarchy = specifications.First().Children.First().Descendants()
                    .Where(node => node.Object == currentObject).First();
                    if (position == "after")
                    {
                        SpecHierarchy parentSpecHierarchy = specHierarchy.Container;
                        int specHierarchyIndex = parentSpecHierarchy.Children.IndexOf(specHierarchy);
                        parentSpecHierarchy.Children.Insert(specHierarchyIndex + 1, new SpecHierarchy()
                        {
                            Object = newSpecObject,
                            Identifier = Guid.NewGuid().ToString(),
                            LastChange = DateTime.Now
                        });
                        var previousObject = specHierarchy.Descendants().Last().Object;
                        currentIndex = (Application.Current.MainWindow as MainWindow).content.SpecObjects.IndexOf(previousObject);
                    }
                    else if (position == "under")
                    {
                        specHierarchy.Children.Insert(0, new SpecHierarchy()
                        {
                            Object = newSpecObject,
                            Identifier = Guid.NewGuid().ToString(),
                            LastChange = DateTime.Now
                        });
                        currentIndex = (Application.Current.MainWindow as MainWindow).MainDataGrid.SelectedIndex;

                    }
                    this.specObjectsViewModel.SpecObjects.Insert(currentIndex + 1, specObject);
                    this.content.SpecObjects.Insert(currentIndex + 1, newSpecObject);
                } else
                {
                    var originalSpecObject = content.SpecObjects.Single(x => x.Identifier == specObject.Identifier);
                    //Update changed AttributeValues
                    foreach (var definition in specObject.Values.Where(x => x.changed == true))
                    {
                        originalSpecObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition).ObjectValue
                            = specObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition).AttributeValue.ObjectValue;
                        definition.changed = false;
                    }
                    // Remove AttributeValues from original SpecObject
                    foreach (var definition in specObject.Values.Where(x => x.removed == true))
                    {
                        originalSpecObject.Values.Remove(originalSpecObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition));
                        definition.removed = false;
                    }
                    //Add new AttributeValues to original SpecObject
                    foreach (var definition in specObject.Values.Where(x => x.added == true))
                    {
                        originalSpecObject.Values.Add(specObject.Values.Single(x => x.AttributeDefinition == definition.AttributeDefinition).AttributeValue);
                        definition.added = false;
                    }
                }
                isContenChanged = true;
            }
        }

        private void ScrollToRow(SpecObject specObject)
        {
            specObject = specObjectsViewModel.SpecObjects.Single(x => x.Identifier == specObject.Identifier);
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
            var test = specObjectsViewModel.SpecObjects;
            var value = (sender as AttributeValueViewModel);
            //if (e.PropertyName == "AttributeValue")
            //{
            //    if (value == null)
            //    {
            //        //value.PropertyChanged -= AttributeValue_PropertyChanged;
            //        (sender as AttributeValueViewModel).AttributeValue.SpecElAt.Values.Remove((sender as AttributeValueViewModel).AttributeValue);
            //    } else
            //    {
            //        value.PropertyChanged += AttributeValue_PropertyChanged;
            //        (sender as AttributeValueViewModel).AttributeValue.SpecElAt.Values.Add((sender as AttributeValueViewModel).AttributeValue);
            //    }
            //}

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

        public void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Edit_SpecObject(((sender as DataGridRow).DataContext as RowDef).Cells, false, "");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SpecificationsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                MainDataGrid.Columns.Clear();
                Source = new TreeDataGrid.GridDef(e.AddedItems[0] as Specification);

                initializeColumns();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Display"));
            }

        }

        private void FilesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpecificationsCombo.SelectedIndex = 0;
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
                try
                {
                    Deserialize(openFileDialog.FileName);
                } catch(Exception exc)
                {
                    MessageBox.Show(exc.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if(content != null)
                {
                    NavigationTreeView.ItemsSource = content.Specifications.First().Children;

                    //Style rowStyle = new Style(typeof(DataGridRow));
                    //rowStyle.Setters.Add(new EventSetter(DataGridRow.MouseDoubleClickEvent,
                    //                         new MouseButtonEventHandler(Row_DoubleClick)));
                    //MainDataGrid.RowStyle = rowStyle;
                    filePath = openFileDialog.FileName;
                    }

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
            var specObjectType = (FilesCombo.SelectedItem as ReqIF).CoreContent.SpecTypes.FirstOrDefault(x => x.GetType() == typeof(SpecObjectType));
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
            foreach (var dataType in (FilesCombo.SelectedItem as ReqIF).CoreContent.SpecTypes.Where(x => x.GetType() == typeof(SpecObjectType)).FirstOrDefault().SpecAttributes) {
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
                var binding = new Binding("Cells.Values[" + i++ + "].AttributeValue.ObjectValue");
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
                if(typeOfDataType == typeof(DatatypeDefinitionXHTML))
                {
                    dataGridTemplateColumn.Width = 300;
                }
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
            var embeddedObject = ((MainWindow)System.Windows.Application.Current.MainWindow).embeddedObjects.Find(x => x.Name == url);
            if(embeddedObject != null)
            {
                embeddedObject.ObjectValue.Position = 0;
                return embeddedObject.ObjectValue;
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

            if ((cell.DataContext as RowDef).Cells.Values.SingleOrDefault(x => x?.AttributeDefinition.LongName == "ReqIF.ChapterName").AttributeValue != null)
            {
                binding = new Binding("Cells.Values[" + _chapterIndex + "].AttributeValue.ObjectValue")
                {
                    Mode = BindingMode.OneWay,
                    NotifyOnSourceUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Converter = new XHTMLConverter(),
                    ConverterParameter = "heading"
                };
            } else {
                binding = new Binding("Cells.Values[" + _textIndex + "].AttributeValue.ObjectValue")
                {
                    Mode = BindingMode.OneWay,
                    NotifyOnSourceUpdated = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Converter = new XHTMLConverter()
                };

            }
            var factory = new FrameworkElementFactory(typeof(Html));
            factory.SetBinding(Html.HtmlProperty, binding);
            
            //Navigation
            FrameworkElementFactory checkFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.CheckBox));
            checkFactory.SetBinding(System.Windows.Controls.CheckBox.MarginProperty, new Binding("Level") { Converter = new LevelToMarginConverter() });
            checkFactory.SetBinding(System.Windows.Controls.CheckBox.IsCheckedProperty, new Binding("IsExpanded") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            checkFactory.SetValue(System.Windows.Controls.CheckBox.StyleProperty, Application.Current.MainWindow.Resources["TreeExpanderStyle"] as Style);
            DataGridTemplateColumn navTemplateColumn = new DataGridTemplateColumn()
            {
                Header = "",
                CellTemplate = new DataTemplate() { VisualTree = checkFactory }
            };
            //Stackpanel
            FrameworkElementFactory stackFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            stackFactory.AppendChild(checkFactory);
            stackFactory.AppendChild(factory);

            DataTemplate cellTemplate = new DataTemplate()
            {
                VisualTree = stackFactory
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
