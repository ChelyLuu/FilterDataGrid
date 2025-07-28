using FilterDataGrid.Themes;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;

namespace FilterDataGrid
{
    public class FilterDataGrid : DataGrid, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged实现接口
        /// <summary>
        /// OnPropertyChange
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public FilterDataGrid()
        {
            #region 抑制System.Windows.Data Error: 4错误，这些绑定错误实际上是无害的
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            #endregion

            CurrentCustomizeFilters = new ObservableCollection<CustomizeFilter>();
            this.KeyDown += FilterDataGrid_KeyDown_Search;//定义打开全局搜索面板事件
            this.Loaded += FilterDataGrid_Loaded;
            #region 主题设置
            Application.Current.Resources["FPrimaryColor"] = Application.Current.Resources["PrimaryHueMidBrush"];
            Application.Current.Resources["FPrimaryForeColor"] = Application.Current.Resources["PrimaryHueMidForegroundBrush"];
            Application.Current.Resources["FPrimaryLightColor"] = Application.Current.Resources["PrimaryHueLightBrush"];
            Application.Current.Resources["FPrimaryLightForeColor"] = Application.Current.Resources["PrimaryHueLightForegroundBrush"];
            Application.Current.Resources["FPrimaryDarkColor"] = Application.Current.Resources["PrimaryHueDarkBrush"];
            Application.Current.Resources["FPrimaryDarkForeColor"] = Application.Current.Resources["PrimaryHueDarkForegroundBrush"];

            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            if (theme.GetBaseTheme() == BaseTheme.Dark)
            {
                Application.Current.Resources.Remove("DataGridLight.xaml");
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"FilterDataGrid;component/Themes/DataGridDark.xaml", UriKind.RelativeOrAbsolute)
                });
            }
            #endregion

            CommandBindings.Add(new CommandBinding(ShowFilter, ShowFilterCommand, CanShowFilter));
            CommandBindings.Add(new CommandBinding(ApplyFilter, ApplyFilterCommand, CanApplyFilter));
            CommandBindings.Add(new CommandBinding(CancelFilter, CancelFilterCommand));
            CommandBindings.Add(new CommandBinding(CheckedAll, CheckedAllCommand));
            CommandBindings.Add(new CommandBinding(IsChecked, IsCheckedCommand));
            CommandBindings.Add(new CommandBinding(RemoveFilter, RemoveFilterCommand, CanRemoveFilter));
            CommandBindings.Add(new CommandBinding(RemoveAllFilter, RemoveAllFilterCommand, CanRemoveAllFilter));
            CommandBindings.Add(new CommandBinding(RemoveThisFilter, RemoveThisFilterCommand, CanRemoveThisFilter));
            CommandBindings.Add(new CommandBinding(CustomizeFilter, CustomizeFilterCommand, CanCustomizeFilter));

            CommandBindings.Add(new CommandBinding(AscendingSort, AscendingSortCommand, CanAscendingSort));
            CommandBindings.Add(new CommandBinding(DescendingSort, DescendingSortCommand, CanDescendingSort));
            CommandBindings.Add(new CommandBinding(RemoveSort, RemoveSortCommand, CanRemoveSort));
            CommandBindings.Add(new CommandBinding(RemoveAllSort, RemoveAllSortCommand, CanRemoveAllSort));
            CommandBindings.Add(new CommandBinding(ClickSort, ClickSortCommand));

            CommandBindings.Add(new CommandBinding(SearchPanel, SearchPanelCommand, CanSearchPanel));
            CommandBindings.Add(new CommandBinding(CloseSearchPanel, CloseSearchPanelCommand));
            CommandBindings.Add(new CommandBinding(ClearSearchWord, ClearSearchWordCommand));
            CommandBindings.Add(new CommandBinding(AppleSearchWord, AppleSearchWordCommand));


            CommandBindings.Add(new CommandBinding(BackPage, NextPageCommand, CanNextPage));
            CommandBindings.Add(new CommandBinding(PrevPage, PrevPageCommand, CanPrevPage));
            CommandBindings.Add(new CommandBinding(HomePage, HomePageCommand, CanHomePage));
            CommandBindings.Add(new CommandBinding(EndPage, EndPageCommand, CanEndPage));
            CommandBindings.Add(new CommandBinding(GoToPage, GoToPageCommand));
            CommandBindings.Add(new CommandBinding(ChangePageSize, ChangePageSizeCommand));

            CommandBindings.Add(new CommandBinding(GroupByColumn, GroupByColumnCommand, CanGroupByColumn));
            CommandBindings.Add(new CommandBinding(ClearGroup, ClearGroupCommand, CanClearGroup));
            CommandBindings.Add(new CommandBinding(ExpandedAllGroup, ExpandedAllGroupCommand, CanExpandedAllGroup));
            CommandBindings.Add(new CommandBinding(CollapsedAllGroup, CollapsedAllGroupCommand, CanCollapsedAllGroup));

            CommandBindings.Add(new CommandBinding(InterfaceSettings, InterfaceSettingsCommand, CanInterfaceSettings));
            CommandBindings.Add(new CommandBinding(ShowDragSumPanel, ShowDragSumPanelCommand, CanShowDragSumPanel));
        }
        private void FilterDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //是否显示过滤Button

            foreach (var col in this.Columns)
            {
                Button btn = VisualTreeHelpers.GetHeader(col, this)
                ?.FindVisualChild<Button>("FilterButton");
                if (btn == null || col == null) continue;
                if (col.GetType() == typeof(DataGridTemplateColumn))
                {
                    btn.Visibility = Visibility.Collapsed;
                }
                else if (col.GetType() == typeof(DataGridTextColumn))
                {
                    DataGridTextColumn dataGridTextColumn = col as DataGridTextColumn;
                    if (!dataGridTextColumn.IsColumnFiltered) { btn.Visibility = Visibility.Collapsed; }
                }
                else if (col.GetType() == typeof(DataGridCheckBoxColumn))
                {
                    DataGridCheckBoxColumn dataGridTextColumn = col as DataGridCheckBoxColumn;
                    if (!dataGridTextColumn.IsColumnFiltered) { btn.Visibility = Visibility.Collapsed; }
                }
            }

            dataGridSelectionUnit = this.SelectionUnit;
            dataGridSelectionMode = this.SelectionMode;
        }

        private void FilterDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SumAndCount();
        }

        #region 命令绑定
        public static readonly ICommand ShowFilter = new RoutedCommand();
        public static readonly ICommand ApplyFilter = new RoutedCommand();
        public static readonly ICommand CancelFilter = new RoutedCommand();
        public static readonly ICommand CheckedAll = new RoutedCommand();
        public static readonly ICommand IsChecked = new RoutedCommand();
        public static readonly ICommand RemoveFilter = new RoutedCommand();
        public static readonly ICommand RemoveAllFilter = new RoutedCommand();
        public static readonly ICommand RemoveThisFilter = new RoutedCommand();
        public static readonly ICommand CustomizeFilter = new RoutedCommand();

        public static readonly ICommand AscendingSort = new RoutedCommand();
        public static readonly ICommand DescendingSort = new RoutedCommand();
        public static readonly ICommand RemoveSort = new RoutedCommand();
        public static readonly ICommand RemoveAllSort = new RoutedCommand();
        public static readonly ICommand ClickSort = new RoutedCommand();

        public static readonly ICommand SearchPanel = new RoutedCommand();
        public static readonly ICommand CloseSearchPanel = new RoutedCommand();
        public static readonly ICommand ClearSearchWord = new RoutedCommand();
        public static readonly ICommand AppleSearchWord = new RoutedCommand();

        public static readonly ICommand ShowColumnsWindow = new RoutedCommand();
        public static readonly ICommand ShowHideColumn = new RoutedCommand();
        public static readonly ICommand ColumnFormatString = new RoutedCommand();

        public static readonly ICommand BackPage = new RoutedCommand();
        public static readonly ICommand PrevPage = new RoutedCommand();
        public static readonly ICommand HomePage = new RoutedCommand();
        public static readonly ICommand EndPage = new RoutedCommand();
        public static readonly ICommand GoToPage = new RoutedCommand();
        public static readonly ICommand ChangePageSize = new RoutedCommand();

        public static readonly ICommand GroupByColumn = new RoutedCommand();
        public static readonly ICommand ClearGroup = new RoutedCommand();
        public static readonly ICommand ExpandedAllGroup = new RoutedCommand();
        public static readonly ICommand CollapsedAllGroup = new RoutedCommand();

        public static readonly ICommand InterfaceSettings = new RoutedCommand();
        public static readonly ICommand ShowDragSumPanel = new RoutedCommand();
        #endregion

        #region Private Fields

        private DataGrid TotalRow;//统计行
        private ScrollViewer TotalScroll;//统计行滚动条
        private DataGridSelectionUnit dataGridSelectionUnit;
        private DataGridSelectionMode dataGridSelectionMode;

        private string searchText;
        private bool pending;
        private bool search;
        private Button button;
        private Popup popup;
        private TextBox searchTextBox;
        private Thumb thumb;
        private Grid sizableContentGrid;
        private DataGridColumnHeadersPresenter columnHeadersPresenter;
        private Cursor cursor;

        private string fieldName;
        private string lastFilter;
        private double minHeight;
        private double minWidth;
        private double sizableContentHeight;
        private double sizableContentWidth;

        private FilterCommon CurrentFilter { get; set; }//当前过滤
        private Type collectionType;//集合类型
        private string globalSearchWord;//全局搜索关键词
        private bool isNumber;//判断是否为数字
        private List<FilterCommon> GlobalFilterList { get; } = new List<FilterCommon>();
        private ICollectionView CollectionViewSource { get; set; }

        private ICollectionView ItemCollectionView { get; set; }

        private readonly Dictionary<string, Predicate<object>> criteria = new Dictionary<string, Predicate<object>>();

        //Popup filtered items (ListBox/TreeView)
        private IEnumerable<FilterItem> PopupViewItems =>
          ItemCollectionView?.OfType<FilterItem>().Where(c => c.Level != 0) ?? new List<FilterItem>();
        //Popup source collection (ListBox/TreeView)
        private IEnumerable<FilterItem> SourcePopupViewItems =>
          ItemCollectionView?.SourceCollection.OfType<FilterItem>().Where(c => c.Level != 0) ?? new List<FilterItem>();


        private List<GroupSortItem> groupSortItemList = new List<GroupSortItem>();
        #endregion

        #region 绑定到界面属性


        private bool _isDragSum = false;
        public bool IsDragSum
        {
            get { return _isDragSum; }
            set { _isDragSum = value; OnPropertyChanged(nameof(IsDragSum)); }
        }

        private Visibility _showDragSum = Visibility.Collapsed;
        public Visibility ShowDragSum
        {
            get { return _showDragSum; }
            set { _showDragSum = value; OnPropertyChanged(nameof(ShowDragSum)); }
        }

        //自定义调整标题栏高度
        private int _customizeHeaderHeight = 40;
        public int CustomizeHeaderHeight
        {
            get { return _customizeHeaderHeight; }
            set { _customizeHeaderHeight = value; OnPropertyChanged(nameof(CustomizeHeaderHeight)); }
        }

        //自定义调整行的高度
        private int _customizeRowHeight = 40;
        public int CustomizeRowHeight
        {
            get { return _customizeRowHeight; }
            set { _customizeRowHeight = value; OnPropertyChanged(nameof(CustomizeRowHeight)); }
        }

        //自定义调整分组行的高度
        private int _groupRowHeight = 30;
        public int GroupRowHeight
        {
            get { return _groupRowHeight; }
            set { _groupRowHeight = value; OnPropertyChanged(nameof(GroupRowHeight)); }
        }

        //自定义调整行头的宽度
        private int _customizeRowHeaderWidth = 50;
        public int CustomizeRowHeaderWidth
        {
            get { return _customizeRowHeaderWidth; }
            set { _customizeRowHeaderWidth = value; OnPropertyChanged(nameof(CustomizeRowHeaderWidth)); }
        }

        //统计行数据源
        private DataView totalRowItemSource;
        public DataView TotalRowItemSource
        {
            get { return totalRowItemSource; }
            set { totalRowItemSource = value; OnPropertyChanged(nameof(TotalRowItemSource)); }
        }

        //全局搜索面板打开、关闭
        private Visibility _searchPanelVisibility = Visibility.Collapsed;
        public Visibility SearchPanelVisibility
        {
            get { return _searchPanelVisibility; }
            set { _searchPanelVisibility = value; OnPropertyChanged(nameof(SearchPanelVisibility)); }
        }

        //ListBox可视
        private Visibility _itemControlVisibility = Visibility.Visible;
        public Visibility ItemControlVisibility
        {
            get { return _itemControlVisibility; }
            set { _itemControlVisibility = value; OnPropertyChanged(nameof(ItemControlVisibility)); }
        }

        //TreeView可视
        private Visibility _treeViewVisibility = Visibility.Collapsed;
        public Visibility TreeViewVisibility
        {
            get { return _treeViewVisibility; }
            set { _treeViewVisibility = value; OnPropertyChanged(nameof(TreeViewVisibility)); }
        }

        //TreeView数据源
        private List<FilterItemDate> treeview;
        public List<FilterItemDate> TreeviewItems
        {
            get => treeview ?? new List<FilterItemDate>();
            set
            {
                treeview = value;
                OnPropertyChanged(nameof(TreeviewItems));
            }
        }

        //ListBox数据源
        private List<FilterItem> listBoxItems;
        public List<FilterItem> ListBoxItems
        {
            get => listBoxItems ?? new List<FilterItem>();
            set
            {
                listBoxItems = value;
                OnPropertyChanged(nameof(ListBoxItems));
            }
        }

        private bool? _selectAllCheck = false;
        public bool? SelectAllCheck
        {
            get { return _selectAllCheck; }
            set { _selectAllCheck = value; OnPropertyChanged(nameof(SelectAllCheck)); }
        }

        private Type fieldType;
        public Type FieldType
        {
            get => fieldType;
            set
            {
                fieldType = value;
                OnPropertyChanged();
            }
        }

        private int _currentPageDisplay = 1;
        public int CurrentPageDisplay
        {
            get { return _currentPageDisplay; }
            set { _currentPageDisplay = value; OnPropertyChanged(nameof(CurrentPageDisplay)); }
        }

        private HorizontalAlignment _pageBarAlignment;
        public HorizontalAlignment PageBarAlignment
        {
            get { return _pageBarAlignment; }
            set { _pageBarAlignment = value; OnPropertyChanged(nameof(PageBarAlignment)); }
        }

        private bool _expandedAll;
        public bool ExpanderAll
        {
            get { return _expandedAll; }
            set { _expandedAll = value; }
        }

        private string _exportTipMessage;
        public string ExportTipMessage
        {
            get { return _exportTipMessage; }
            set { _exportTipMessage = value; OnPropertyChanged("ExportTipMessage"); }
        }

        public string GroupFormatString { get; set; }


        //允许用户进行分组
        public bool CanUserFilter { get; set; } = true;
        //Display row index
        public bool ShowRowIndex { get; set; } = false;

        // Display items count
        public int ItemsSourceCount { get; set; }

        //允许用户进行分组
        public bool CanUserGroup { get; set; } = false;
        //允许鼠标拖动统计数据
        public bool CanDragSum { get; set; } = false;
        //是否使用了隔行换色
        public bool DoubleLineColor { get; set; } = false;

        //统计行可视属性
        public Visibility TotalFooterVisibility { get; set; } = Visibility.Collapsed;
        public Visibility PageBarVisibility { get; set; } = Visibility.Collapsed;
        public List<ColumnProperty> ColumnsCollection { get; set; }
        public static ObservableCollection<CustomizeFilter> CurrentCustomizeFilters { get; set; }

        private string _mouseDragSum = Properties.Resources.Count + "\r\n" +
                    Properties.Resources.Avg + "\r\n" +
                    Properties.Resources.Sum;
        public string MouseDragSum
        {
            get { return _mouseDragSum; }
            set { _mouseDragSum = value; OnPropertyChanged(nameof(MouseDragSum)); }
        }

        #endregion

        #region Public DependencyProperty

        /// <summary>
        /// 统计行集合
        /// </summary>
        public List<FooterItem> FooterItems
        {
            get => (List<FooterItem>)GetValue(FooterItemsProperty);
            set => SetValue(FooterItemsProperty, value);
        }
        public static readonly DependencyProperty FooterItemsProperty =
            DependencyProperty.Register("FooterItems", typeof(List<FooterItem>), typeof(FilterDataGrid), new PropertyMetadata(new List<FooterItem>()));

        public List<string> GroupColumnsList
        {
            get => (List<string>)GetValue(GroupColumnsListProperty);
            set => SetValue(GroupColumnsListProperty, value);
        }
        public static readonly DependencyProperty GroupColumnsListProperty =
            DependencyProperty.Register("GroupColumnsList", typeof(List<string>), typeof(FilterDataGrid), new PropertyMetadata(new List<string>()));

        public int CurrentPage
        {
            get { return (int)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }
        public static readonly DependencyProperty CurrentPageProperty =
       DependencyProperty.Register("CurrentPage", typeof(int), typeof(FilterDataGrid), new PropertyMetadata(1));


        public int TotalPages
        {
            get { return (int)GetValue(TotalPagesProperty); }
            set { SetValue(TotalPagesProperty, value); }
        }
        public static readonly DependencyProperty TotalPagesProperty =
      DependencyProperty.Register("TotalPages", typeof(int), typeof(FilterDataGrid), new PropertyMetadata(1));

        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }
        public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register("PageSize", typeof(int), typeof(FilterDataGrid), new PropertyMetadata(500));

        public List<int> PageSizeList
        {
            get { return (List<int>)GetValue(PageSizeListProperty); }
            set { SetValue(PageSizeListProperty, value); }
        }
        public static readonly DependencyProperty PageSizeListProperty =
        DependencyProperty.Register("PageSizeList", typeof(List<int>), typeof(FilterDataGrid), new PropertyMetadata(new List<int>() { 10, 20, 30, 50, 80, 100, 150, 200, 300, 500, 800, 1000 }));


        public string HiText
        {
            get { return (string)GetValue(HiTextProperty); }
            set { SetValue(HiTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HiText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiTextProperty =
            DependencyProperty.Register("HiText", typeof(string), typeof(FilterDataGrid), new PropertyMetadata(""));



        #endregion

        #region 自定义事件
        public event RoutedEventHandler PageChanged
        {
            add { AddHandler(PageChangedEvent, value); }
            remove { RemoveHandler(PageChangedEvent, value); }
        }
        public static readonly RoutedEvent PageChangedEvent =
              EventManager.RegisterRoutedEvent("PageChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FilterDataGrid));


        public event RoutedEventHandler ApplyGlobalSearch
        {
            add { AddHandler(ApplyGlobalSearchEvent, value); }
            remove { RemoveHandler(ApplyGlobalSearchEvent, value); }
        }
        public static readonly RoutedEvent ApplyGlobalSearchEvent =
              EventManager.RegisterRoutedEvent("ApplyGlobalSearch", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FilterDataGrid));


        public event RoutedEventHandler CancelGlobalSearch
        {
            add { AddHandler(CancelGlobalSearchEvent, value); }
            remove { RemoveHandler(CancelGlobalSearchEvent, value); }
        }
        public static readonly RoutedEvent CancelGlobalSearchEvent =
              EventManager.RegisterRoutedEvent("CancelGlobalSearch", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FilterDataGrid));


        #endregion



        /// <summary>
        ///     Initialize datagrid
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            try
            {

                if (CanUserGroup)
                {
                    VirtualizingPanel.SetIsVirtualizingWhenGrouping(this, true);

                    GroupStyle gs = new GroupStyle();

                    gs.ContainerStyle = Application.Current.TryFindResource("DataGridGroupSyle") as Style;
                    this.GroupStyle.Add(gs);
                    gs = new GroupStyle();
                    gs.ContainerStyle = Application.Current.TryFindResource("DataGridGroupSyle2") as Style;
                    this.GroupStyle.Add(gs);
                }

                ColumnHeaderStyle = Application.Current.TryFindResource("FilterDataGridColumnHeader") as Style;
                RowHeaderStyle = Application.Current.TryFindResource("FilterDataGridRowHeader") as Style;

                string filePath = AppDomain.CurrentDomain.BaseDirectory + "Layout/" + this.Name + ".xml";
                if (File.Exists(filePath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);
                    XmlNode root = doc.SelectSingleNode("DataGrid");
                    XmlNode xmlNode = root.SelectSingleNode("ChangeColor");
                    if (xmlNode.InnerText == "True")
                    {
                        RowStyle = Application.Current.TryFindResource("DataGridRowStyle_ChangeColor") as Style;
                        DoubleLineColor = true;
                    }

                    xmlNode = root.SelectSingleNode("ShowGlobalSearchPanel");
                    if (xmlNode.InnerText == "True")
                    {
                        SearchPanelVisibility = Visibility.Visible;
                    }
                    else
                    {
                        SearchPanelVisibility = Visibility.Collapsed;
                    }

                    xmlNode = root.SelectSingleNode("ShowVerticalGridLine");
                    if (xmlNode.InnerText == "True")
                    {
                        this.GridLinesVisibility = DataGridGridLinesVisibility.All;
                    }
                    else
                    {
                        this.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
                    }

                    xmlNode = root.SelectSingleNode("ColumnHeaderHeight");
                    CustomizeHeaderHeight = Convert.ToInt32(xmlNode.InnerText);

                    xmlNode = root.SelectSingleNode("RowHeight");
                    CustomizeRowHeight = Convert.ToInt32(xmlNode.InnerText);

                    xmlNode = root.SelectSingleNode("RowHeaderWidth");
                    CustomizeRowHeaderWidth = Convert.ToInt32(xmlNode.InnerText);

                    xmlNode = root.SelectSingleNode("GroupRowHeight");
                    GroupRowHeight = Convert.ToInt32(xmlNode.InnerText);

                    xmlNode = root.SelectSingleNode("Group");
                    GroupColumnsList = new List<string>();
                    foreach (XmlNode node in xmlNode.ChildNodes)
                    {
                        GroupColumnsList.Add(node.InnerText);
                    }

                    xmlNode = root.SelectSingleNode("Sort");
                    foreach (XmlNode node in xmlNode.ChildNodes)
                    {
                        groupSortItemList.Add(
                            new GroupSortItem
                            {
                                FieldName = node.Name,
                                SortDirection = GroupSortItem.StringToSortDirdection(node.InnerText)
                            });
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnInitialized : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     The source of the Datagrid items has been changed (refresh or on loading)
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {

            if (TotalFooterVisibility == Visibility.Visible)
            {
                ScrollViewer sv = VisualTreeHelpers.FindChild<ScrollViewer>(this);
                if (sv != null)
                {
                    sv.ScrollChanged -= new ScrollChangedEventHandler(Sv_ScrollChanged);
                }
                sv.ScrollChanged += new ScrollChangedEventHandler(Sv_ScrollChanged);
                TotalRow = (DataGrid)sv.Template.FindName("TotalRow", sv);
                TotalRow.Columns?.Clear();
            }
            if (CanUserGroup) ExpanderAll = false;
            base.OnItemsSourceChanged(oldValue, newValue);

            try
            {

                if (newValue == null) return;
                if (oldValue != null)
                {
                    // reset current filter, !important
                    CurrentFilter = null;

                    // reset GlobalFilterList list
                    GlobalFilterList.Clear();

                    // reset criteria List
                    criteria.Clear();

                    // free previous resource
                    CollectionViewSource = System.Windows.Data.CollectionViewSource.GetDefaultView(new object());

                    //foreach (var col in this.Columns)
                    //{
                    //    Console.WriteLine(col.GetType());
                    //    Button btn = VisualTreeHelpers.GetHeader(col, this)
                    //    ?.FindVisualChild<Button>("FilterButton");

                    //    if (button == null || string.IsNullOrEmpty(fieldName)) continue;

                    //    button.Tag = "false";


                    //}

                    // scroll to top on reload collection
                    //var scrollViewer = GetTemplateChild("DG_ScrollViewer") as ScrollViewer;
                    //scrollViewer?.ScrollToTop();
                }
                CollectionViewSource = System.Windows.Data.CollectionViewSource.GetDefaultView(ItemsSource);

                // set Filter, contribution : STEFAN HEIMEL
                if (CollectionViewSource.CanFilter) CollectionViewSource.Filter = AllFilter;

                CollectionViewSource.CollectionChanged -= CollectionViewSource_CollectionChanged;
                CollectionViewSource.CollectionChanged += CollectionViewSource_CollectionChanged;

                ItemsSourceCount = Items.Count;

                OnPropertyChanged(nameof(ItemsSourceCount));



                // get collection type
                if (ItemsSourceCount > 0)
                    // contribution : APFLKUACHA
                    collectionType = ItemsSource is ICollectionView collectionView
                        ? collectionView.SourceCollection?.GetType().GenericTypeArguments.FirstOrDefault()
                        : ItemsSource?.GetType().GenericTypeArguments.FirstOrDefault();
                this.Focus();

                // generating custom columns
                //if (!AutoGenerateColumns && collectionType != null) GeneratingCustomsColumn();

                #region 改变数据源时保持分组状态
                if (GroupColumnsList.Count > 0)
                {
                    foreach (var item in GroupColumnsList)
                    {
                        CollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(item));
                    }
                }
                #endregion

                #region 改变数据源时保持排序状态
                if (groupSortItemList.Count > 0)
                {
                    foreach (var item in groupSortItemList)
                    {
                        foreach (DataGridColumn columnHeader in this.Columns)
                        {
                            if (DataGridColumnHelper.DataGridColumnToString(columnHeader) == item.FieldName)
                            {
                                columnHeader.SortDirection = item.SortDirection;
                            }
                        }
                        CollectionViewSource.SortDescriptions.Add(new SortDescription(item.FieldName, item.SortDirection));
                    }
                }
                #endregion

                #region TotalRow 统计行
                if (TotalFooterVisibility == Visibility.Visible)
                {
                    foreach (var item in this.Columns)
                    {
                        DataGridTextColumn cl = new DataGridTextColumn
                        {
                            Header = item.Header,
                            Width = item.Width,
                            DisplayIndex = item.DisplayIndex
                        };

                        Binding widthBd = new Binding
                        {
                            Source = item,
                            Mode = BindingMode.TwoWay,
                            Path = new PropertyPath(DataGridColumn.WidthProperty)
                        };
                        BindingOperations.SetBinding(cl, DataGridTextColumn.WidthProperty, widthBd);

                        Binding visibleBd = new Binding
                        {
                            Source = item,
                            Mode = BindingMode.TwoWay,
                            Path = new PropertyPath(DataGridColumn.VisibilityProperty)
                        };
                        BindingOperations.SetBinding(cl, DataGridTextColumn.VisibilityProperty, visibleBd);

                        Binding indexBd = new Binding
                        {
                            Source = item,
                            Mode = BindingMode.OneWay,
                            Path = new PropertyPath(DataGridColumn.DisplayIndexProperty)
                        };
                        BindingOperations.SetBinding(cl, DataGridTextColumn.DisplayIndexProperty, indexBd);


                        if (item.GetType().Name == "DataGridTextColumn")
                            cl.Binding = (item as DataGridTextColumn).Binding;
                        TotalRow.Columns.Add(cl);
                    }

                    TotalScroll = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(TotalRow, 0), 0) as ScrollViewer;
                    CollectionViewSource.Refresh();
                }
                #endregion

                #region  加载界面设置
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "Layout/" + this.Name + ".xml";
                if (File.Exists(filePath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);
                    XmlNode root = doc.SelectSingleNode("DataGrid");
                    XmlNode xmlNode = root.SelectSingleNode("Columns");

                    foreach (var item in this.Columns)
                    {
                        if (item.GetType() == typeof(DataGridTextColumn))
                        {
                            DataGridTextColumn dtc = (DataGridTextColumn)item;
                            if (xmlNode.SelectSingleNode(dtc.FieldName) == null) continue;
                            XmlNode columnNode = xmlNode.SelectSingleNode(dtc.FieldName).SelectSingleNode("Width");
                            item.Width = Convert.ToDouble(columnNode.InnerText);

                            columnNode = xmlNode.SelectSingleNode(dtc.FieldName).SelectSingleNode("Visibility");
                            if (columnNode.InnerText == "Collapsed") item.Visibility = Visibility.Collapsed;

                            columnNode = xmlNode.SelectSingleNode(dtc.FieldName).SelectSingleNode("DisplayIndex");
                            item.DisplayIndex = Convert.ToInt32(columnNode.InnerText);

                            columnNode = xmlNode.SelectSingleNode(dtc.FieldName).SelectSingleNode("Alignment");
                            switch (columnNode.InnerText)
                            {
                                case "Left":
                                    dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Left") as Style;
                                    break;
                                case "Center":
                                    dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Center") as Style;
                                    break;
                                case "Right":
                                    dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Right") as Style;
                                    break;
                                default:
                                    dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Default") as Style;
                                    break;
                            }
                        }
                        else if (item.GetType() == typeof(DataGridCheckBoxColumn))
                        {
                            DataGridCheckBoxColumn dcc = item as DataGridCheckBoxColumn;
                            if (xmlNode.SelectSingleNode(dcc.FieldName) == null) continue;
                            XmlNode columnNode = xmlNode.SelectSingleNode(dcc.FieldName).SelectSingleNode("Width");
                            item.Width = Convert.ToDouble(columnNode.InnerText);

                            columnNode = xmlNode.SelectSingleNode(dcc.FieldName).SelectSingleNode("Visibility");
                            if (columnNode.InnerText == "Collapsed") item.Visibility = Visibility.Collapsed;

                            columnNode = xmlNode.SelectSingleNode(dcc.FieldName).SelectSingleNode("DisplayIndex");
                            item.DisplayIndex = Convert.ToInt32(columnNode.InnerText);
                            dcc.CellStyle = Application.Current.TryFindResource("CheckBoxCellStyle") as Style;
                        }
                        else
                        {
                            DataGridTemplateColumn dtc = item as DataGridTemplateColumn;
                            if (xmlNode.SelectSingleNode(dtc.FieldName) == null) continue;
                            XmlNode columnNode = xmlNode.SelectSingleNode(dtc.FieldName).SelectSingleNode("Width");
                            item.Width = Convert.ToDouble(columnNode.InnerText);

                            columnNode = xmlNode.SelectSingleNode(dtc.FieldName).SelectSingleNode("Visibility");
                            if (columnNode.InnerText == "Collapsed") item.Visibility = Visibility.Collapsed;

                            columnNode = xmlNode.SelectSingleNode(dtc.FieldName).SelectSingleNode("DisplayIndex");
                            item.DisplayIndex = Convert.ToInt32(columnNode.InnerText);
                            dtc.CellStyle = Application.Current.TryFindResource("TemplateColumnCellStyle") as Style;
                        }

                    }
                }
                else
                {
                    foreach (var item in Columns)
                    {
                        if (item.GetType() == typeof(DataGridTextColumn))
                        {
                            DataGridTextColumn dtc = item as DataGridTextColumn;
                            if(dtc.HorizontalAlignment==  HorizontalAlignment.Right)
                                dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Right") as Style;
                            else if (dtc.HorizontalAlignment == HorizontalAlignment.Center)
                                dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Center") as Style;
                            else if (dtc.HorizontalAlignment == HorizontalAlignment.Left)
                                dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Left") as Style;
                            else
                                dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Default") as Style;
                        }
                        else if (item.GetType() == typeof(DataGridCheckBoxColumn))
                        {
                            DataGridCheckBoxColumn dtc = item as DataGridCheckBoxColumn;
                            dtc.CellStyle = Application.Current.TryFindResource("CheckBoxCellStyle") as Style;
                        }
                        else
                        {
                            DataGridTemplateColumn dtc = item as DataGridTemplateColumn;
                            dtc.CellStyle = Application.Current.TryFindResource("TemplateColumnCellStyle") as Style;
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Auto generated column, set templateHeader
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
        {

            base.OnAutoGeneratingColumn(e);

            try
            {

                if (e.Column.GetType() == typeof(System.Windows.Controls.DataGridTextColumn))
                {
                    var column = new DataGridTextColumn
                    {
                        Binding = new Binding(e.PropertyName) { ConverterCulture = Properties.Resources.Culture /* StringFormat */ },
                        FieldName = e.PropertyName,
                        Header = e.Column.Header.ToString(),
                        IsColumnFiltered = true
                    };
                    e.Column = column;
                }
                if (e.Column.GetType() == typeof(System.Windows.Controls.DataGridCheckBoxColumn))
                {
                    var column = new DataGridCheckBoxColumn
                    {
                        Binding = new Binding(e.PropertyName) { ConverterCulture = Properties.Resources.Culture /* StringFormat */ },
                        FieldName = e.PropertyName,
                        Header = e.Column.Header.ToString(),
                        IsColumnFiltered = true
                    };
                    e.Column = column;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnAutoGeneratingColumn : {ex.Message}");
            }
        }

        /// <summary>
        ///     Adding Rows count
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoadingRow(DataGridRowEventArgs e)
        {
            if (ShowRowIndex)
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        ///     Generate custom columns that can be filtered
        /// </summary>
        //private void GeneratingCustomsColumn()
        //{

        //    try
        //    {
        //        // get the columns that can be filtered
        //        var columns = Columns
        //            .Where(c => (c is DataGridTextColumn dtx && dtx.IsColumnFiltered)
        //                        || (c is DataGridTemplateColumn dtp && dtp.IsColumnFiltered)
        //                        || (c is DataGridCheckBoxColumn dcb && dcb.IsColumnFiltered)
        //                        )
        //            .Select(c => c)
        //            .ToList();

        //        // set header template
        //        foreach (var col in columns)
        //        {
        //            var columnType = col.GetType();

        //            if (col.HeaderTemplate != null)
        //            {
        //                // reset filter Button
        //                var buttonFilter = VisualTreeHelpers.GetHeader(col, this)
        //                    ?.FindVisualChild<Button>("FilterButton");
        //                if (buttonFilter != null) FilterState.SetIsFiltered(buttonFilter, false);
        //            }
        //            else
        //            {
        //                if (columnType == typeof(DataGridTextColumn))
        //                {
        //                    var column = (DataGridTextColumn)col;

        //                    // template
        //                    //column.HeaderTemplate = (DataTemplate)TryFindResource("DataGridHeaderTemplate");

        //                    fieldType = null;
        //                    var fieldProperty = collectionType.GetProperty(((Binding)column.Binding).Path.Path);

        //                    // get type or underlying type if nullable
        //                    if (fieldProperty != null)
        //                        fieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ??
        //                                    fieldProperty.PropertyType;

        //                    // apply DateFormatString when StringFormat for column is not provided or empty
        //                    //if (fieldType == typeof(DateTime) && !string.IsNullOrEmpty(DateFormatString))
        //                    //    if (string.IsNullOrEmpty(column.Binding.StringFormat))
        //                    //        column.Binding.StringFormat = DateFormatString;

        //                    // culture
        //                    if (((Binding)column.Binding).ConverterCulture == null)
        //                        ((Binding)column.Binding).ConverterCulture = Properties.Resources.Culture;

        //                    column.FieldName = ((Binding)column.Binding).Path.Path;
        //                }

        //                if (columnType == typeof(DataGridTemplateColumn))
        //                {
        //                    // DataGridTemplateColumn has no culture property
        //                    var column = (DataGridTemplateColumn)col;

        //                    // template
        //                    //column.HeaderTemplate = (DataTemplate)TryFindResource("DataGridHeaderTemplate");
        //                }

        //                if (columnType == typeof(DataGridCheckBoxColumn))
        //                {
        //                    // DataGridCheckBoxColumn has no culture property
        //                    var column = (DataGridCheckBoxColumn)col;

        //                    column.FieldName = ((Binding)column.Binding).Path.Path;

        //                    if (((Binding)column.Binding).ConverterCulture == null)
        //                        ((Binding)column.Binding).ConverterCulture =  Properties.Resources.Culture;

        //                    // template
        //                    //column.HeaderTemplate = (DataTemplate)TryFindResource("DataGridHeaderTemplate");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"FilterDataGrid.GeneratingCustomColumn : {ex.Message}");
        //        throw;
        //    }
        //}


        /// <summary>
        /// 统计行滚动条跟随主滚动条一起滚动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = (ScrollViewer)sender;
            TotalScroll.ScrollToHorizontalOffset(sv.HorizontalOffset);
        }

        /// <summary>
        /// 数据集合改变时，统计行数据调整
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionViewSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (TotalFooterVisibility == Visibility.Visible)
            {
                if (Items == null || Items.Count == 0) { DataTable dataTable = new DataTable(); TotalRowItemSource = dataTable.DefaultView; return; }
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        DataTable dataTable = new DataTable();
                        Type itemType = null;
                        itemType = Items[0].GetType();
                        PropertyInfo[] ps = itemType.GetProperties();
                        foreach (PropertyInfo property in ps)
                        {
                            dataTable.Columns.Add(property.Name);
                        }

                        dataTable.Rows.Add();

                        foreach (FooterItem item in FooterItems)
                        {
                            if (item.TotalType == TotalType.Count)
                            {
                                dataTable.Rows[0][item.FieldName] = Items.Count.ToString(item.FormatString);
                            }
                            if (item.TotalType == TotalType.Sum)
                            {
                                var fieldProperty = collectionType.GetProperty(item.FieldName);

                                var sourceObjectList = new List<object>();

                                sourceObjectList = Items.Cast<object>()
                                                        .Select(x => fieldProperty?.GetValue(x, null))
                                                                             .ToList();

                                List<double> list_int = sourceObjectList.ConvertAll(c => { return Convert.ToDouble(c); }).ToList();

                                dataTable.Rows[0][item.FieldName] = list_int.Sum().ToString(item.FormatString);
                            }
                            if (item.TotalType == TotalType.Ave)
                            {
                                var fieldProperty = collectionType.GetProperty(item.FieldName);

                                var sourceObjectList = new List<object>();

                                sourceObjectList = Items.Cast<object>()
                                                        .Select(x => fieldProperty?.GetValue(x, null))
                                                                             .ToList();

                                List<double> list_int = sourceObjectList.ConvertAll(c => { return Convert.ToDouble(c); }).ToList();

                                dataTable.Rows[0][item.FieldName] = list_int.Average().ToString(item.FormatString);
                            }
                        }

                        TotalRowItemSource = dataTable.DefaultView;
                        dataTable.Dispose();
                    });
                });
            }
        }

        /// <summary>
        ///     Filter current list in popup
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool SearchFilter(object obj)
        {
            var item = (FilterItem)obj;
            if (string.IsNullOrEmpty(searchText) || item == null || item.Level == 0) return true;

            var content = Convert.ToString(item.Content, Properties.Resources.Culture);

            return content.ToLower().Contains(searchText.ToLower());//不区分大小写
        }

        /// <summary>
        ///  过滤条件
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public bool AllFilter(object sourceObject)
        {
            bool FilterResult = Filter(sourceObject);

            if (globalSearchWord != null)
            {
                FilterResult = FilterResult && Filter2(sourceObject);
            }

            if (CurrentCustomizeFilters.Count > 0)
                FilterResult = FilterResult && Filter3(sourceObject);

            return FilterResult;

        }

        /// <summary>
        /// 全局搜索过滤条件
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        private bool Filter2(object sourceObject)
        {
            Type typeName = sourceObject.GetType();
            PropertyInfo[] ps = typeName.GetProperties();
            bool filter = false;
            for (int i = 0; i < ps.Count(); i++)
            {
                if (ps[i].GetValue(sourceObject, null) == null)
                {
                    filter = filter || false;
                }
                else
                {
                    filter = filter || ps[i].GetValue(sourceObject, null).ToString().Contains(globalSearchWord);
                }
            }
            return filter;

        }

        private bool Filter3(object sourceObject)
        {
            bool result = false;
            //Type typeName = sourceObject.GetType();
            //PropertyInfo[] ps = typeName.GetProperties();

            foreach (var item in CurrentCustomizeFilters)
            {
                var fieldProperty = collectionType.GetProperty(item.ColumnsList.FieldName);
                //var fieldProperty = ps.FirstOrDefault(x => x.Name == item.ColumnsList.FieldName);
                //Console.WriteLine($"Filter3: {item.ColumnsList.FieldName} {fieldProperty?.Name}");
                if (item.AndOr == Properties.Resources.And)
                {
                    switch (item.OperatorID)
                    {
                        case "Equal":
                            result = result && fieldProperty.GetValue(sourceObject, null).ToString() == item.Condition;
                            break;
                        case "UnEqual":
                            result = result && fieldProperty.GetValue(sourceObject, null).ToString() != item.Condition;
                            break;
                        case "Contain":
                            result = result && fieldProperty.GetValue(sourceObject, null).ToString().Contains(item.Condition);
                            break;
                        case "UnContain":
                            result = result && !fieldProperty.GetValue(sourceObject, null).ToString().Contains(item.Condition);
                            break;
                        case "Greater":
                            result = result && (item.ColumnsList.ColumnType == typeof(DateTime)
                                ? (DateTime)fieldProperty.GetValue(sourceObject, null) > Convert.ToDateTime(item.Condition)
                                : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) > Convert.ToDouble(item.Condition));
                            break;
                        case "GreaterEqual":
                            result = result && (item.ColumnsList.ColumnType == typeof(DateTime)
                                ? (DateTime)fieldProperty.GetValue(sourceObject, null) >= Convert.ToDateTime(item.Condition)
                                : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) >= Convert.ToDouble(item.Condition));
                            break;
                        case "Less":
                            result = result && (item.ColumnsList.ColumnType == typeof(DateTime)
                                 ? (DateTime)fieldProperty.GetValue(sourceObject, null) < Convert.ToDateTime(item.Condition)
                                 : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) < Convert.ToDouble(item.Condition));
                            break;
                        case "LessEqual":
                            result = result && (item.ColumnsList.ColumnType == typeof(DateTime)
                                 ? (DateTime)fieldProperty.GetValue(sourceObject, null) <= Convert.ToDateTime(item.Condition)
                                 : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) <= Convert.ToDouble(item.Condition));
                            break;
                        case "StartWith":
                            result = result && fieldProperty.GetValue(sourceObject, null).ToString().StartsWith(item.Condition);
                            break;
                        case "EndWith":
                            result = result && fieldProperty.GetValue(sourceObject, null).ToString().EndsWith(item.Condition);
                            break;
                        case "Null":
                            result = result && string.IsNullOrEmpty(fieldProperty.GetValue(sourceObject, null).ToString());
                            break;
                        case "UnNull":
                            result = result && !string.IsNullOrEmpty(fieldProperty.GetValue(sourceObject, null).ToString());
                            break;
                    }
                }
                else if (item.AndOr == Properties.Resources.Or)
                {
                    switch (item.OperatorID)
                    {
                        case "Equal":
                            result = result || fieldProperty.GetValue(sourceObject, null).ToString() == item.Condition;
                            break;
                        case "UnEqual":
                            result = result || fieldProperty.GetValue(sourceObject, null).ToString() != item.Condition;
                            break;
                        case "Contain":
                            result = result || fieldProperty.GetValue(sourceObject, null).ToString().Contains(item.Condition);
                            break;
                        case "UnContain":
                            result = result || !fieldProperty.GetValue(sourceObject, null).ToString().Contains(item.Condition);
                            break;
                        case "Greater":
                            result = result || (item.ColumnsList.ColumnType == typeof(DateTime)
                                ? (DateTime)fieldProperty.GetValue(sourceObject, null) > Convert.ToDateTime(item.Condition)
                                : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) > Convert.ToDouble(item.Condition));
                            break;
                        case "GreaterEqual":
                            result = result || (item.ColumnsList.ColumnType == typeof(DateTime)
                                ? (DateTime)fieldProperty.GetValue(sourceObject, null) >= Convert.ToDateTime(item.Condition)
                                : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) >= Convert.ToDouble(item.Condition));
                            break;
                        case "Less":
                            result = result || (item.ColumnsList.ColumnType == typeof(DateTime)
                                 ? (DateTime)fieldProperty.GetValue(sourceObject, null) < Convert.ToDateTime(item.Condition)
                                 : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) < Convert.ToDouble(item.Condition));
                            break;
                        case "LessEqual":
                            result = result || (item.ColumnsList.ColumnType == typeof(DateTime)
                                 ? (DateTime)fieldProperty.GetValue(sourceObject, null) <= Convert.ToDateTime(item.Condition)
                                 : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) <= Convert.ToDouble(item.Condition));
                            break;
                        case "StartWith":
                            result = result || fieldProperty.GetValue(sourceObject, null).ToString().StartsWith(item.Condition);
                            break;
                        case "EndWith":
                            result = result || fieldProperty.GetValue(sourceObject, null).ToString().EndsWith(item.Condition);
                            break;
                        case "Null":
                            result = result || string.IsNullOrEmpty(fieldProperty.GetValue(sourceObject, null).ToString());
                            break;
                        case "UnNull":
                            result = result || !string.IsNullOrEmpty(fieldProperty.GetValue(sourceObject, null).ToString());
                            break;
                    }
                }
                else
                {
                    switch (item.OperatorID)
                    {
                        case "Equal":
                            result = fieldProperty.GetValue(sourceObject, null).ToString() == item.Condition;
                            break;
                        case "UnEqual":
                            result = fieldProperty.GetValue(sourceObject, null).ToString() != item.Condition;
                            break;
                        case "Contain":
                            result = fieldProperty.GetValue(sourceObject, null).ToString().Contains(item.Condition);
                            break;
                        case "UnContain":
                            result = !fieldProperty.GetValue(sourceObject, null).ToString().Contains(item.Condition);
                            break;
                        case "Greater":
                            result = item.ColumnsList.ColumnType == typeof(DateTime)
                                ? (DateTime)fieldProperty.GetValue(sourceObject, null) > Convert.ToDateTime(item.Condition)
                                : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) > Convert.ToDouble(item.Condition);
                            break;
                        case "GreaterEqual":
                            result = item.ColumnsList.ColumnType == typeof(DateTime)
                                ? (DateTime)fieldProperty.GetValue(sourceObject, null) >= Convert.ToDateTime(item.Condition)
                                : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) >= Convert.ToDouble(item.Condition);
                            break;
                        case "Less":
                            result = item.ColumnsList.ColumnType == typeof(DateTime)
                                 ? (DateTime)fieldProperty.GetValue(sourceObject, null) < Convert.ToDateTime(item.Condition)
                                 : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) < Convert.ToDouble(item.Condition);
                            break;
                        case "LessEqual":
                            result = item.ColumnsList.ColumnType == typeof(DateTime)
                                 ? (DateTime)fieldProperty.GetValue(sourceObject, null) <= Convert.ToDateTime(item.Condition)
                                 : Convert.ToDouble(fieldProperty.GetValue(sourceObject, null)) <= Convert.ToDouble(item.Condition);
                            break;
                        case "StartWith":
                            result = fieldProperty.GetValue(sourceObject, null).ToString().StartsWith(item.Condition);
                            break;
                        case "EndWith":
                            result = fieldProperty.GetValue(sourceObject, null).ToString().EndsWith(item.Condition);
                            break;
                        case "Null":
                            result = string.IsNullOrEmpty(fieldProperty.GetValue(sourceObject, null).ToString());
                            break;
                        case "UnNull":
                            result = !string.IsNullOrEmpty(fieldProperty.GetValue(sourceObject, null).ToString());
                            break;
                    }
                }
            }

            return result;
        }

        private bool Filter(object o)
        {
            return criteria.Values
                   .Aggregate(true, (prevValue, predicate) => prevValue && predicate(o));
        }

        /// <summary>
        /// 打开全局搜索面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterDataGrid_KeyDown_Search(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
                SearchPanelVisibility = Visibility.Visible;
        }

        /// <summary>
        ///     Reset the size of popup to original size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupClosed(object sender, EventArgs e)
        {

            var pop = (Popup)sender;

            if (!pending)
            {
                // clear resources
                ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(new object());
                CurrentFilter = null;
            }

            // unsubscribe from event and re-enable datagrid
            pop.Closed -= PopupClosed;
            pop.MouseDown -= onMousedown;
            searchTextBox.TextChanged -= SearchTextBoxOnTextChanged;
            thumb.DragCompleted -= OnResizeThumbDragCompleted;
            thumb.DragDelta -= OnResizeThumbDragDelta;
            thumb.DragStarted -= OnResizeThumbDragStarted;

            sizableContentGrid.Width = sizableContentWidth;
            sizableContentGrid.Height = sizableContentHeight;


            ListBoxItems = new List<FilterItem>();
            TreeviewItems = new List<FilterItemDate>();

            searchText = string.Empty;
            search = false;


            // re-enable columnHeadersPresenter
            if (columnHeadersPresenter != null)
                columnHeadersPresenter.IsEnabled = true;

        }

        /// <summary>
        ///     Handle Mousedown, contribution : WORDIBOI
        /// </summary>
        private readonly MouseButtonEventHandler onMousedown = (o, eArgs) => { eArgs.Handled = true; };

        /// <summary>
        ///     Search TextBox Text Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            // fix TextChanged event fires twice I did not find another solution
            if (textBox == null || textBox.Text == searchText || ItemCollectionView == null) return;

            searchText = textBox.Text;

            search = !string.IsNullOrEmpty(searchText);

            // apply filter
            ItemCollectionView.Refresh();

            if (CurrentFilter.FieldType != typeof(DateTime) || treeview == null) return;

            // rebuild treeview rebuild treeview
            if (string.IsNullOrEmpty(searchText))
            {
                // fill the tree with the elements of the list of the original items
                TreeviewItems = BuildTree(SourcePopupViewItems);
            }
            else
            {
                // fill the tree only with the items found by the search
                var items = PopupViewItems.Where(i => i.IsChecked).ToList();

                // if at least one item is not null, fill in the tree structure otherwise the tree structure contains only the item (select all).
                TreeviewItems = BuildTree(items.Any() ? items : null);
            }
        }

        /// <summary>
        ///     Build the item tree
        /// </summary>
        /// <param name="dates"></param>
        /// <returns></returns>
        private List<FilterItemDate> BuildTree(IEnumerable<FilterItem> dates)
        {
            try
            {
                var tree = new List<FilterItemDate>();


                if (dates == null) return tree;

                var dateTimes = dates.ToList();

                foreach (var y in dateTimes.Where(c => c.Level == 1)
                             .Select(filterItem => new
                             {
                                 ((DateTime)filterItem.Content).Date,
                                 Item = filterItem
                             })
                             .GroupBy(g => g.Date.Year)
                             .Select(year => new FilterItemDate
                             {
                                 Level = 1,
                                 Content = year.Key,
                                 Label = year.FirstOrDefault()?.Date.ToString("yyyy", Properties.Resources.Culture),
                                 Initialize = true, // default state
                                 FieldType = fieldType,

                                 Children = year.GroupBy(date => date.Date.Month)
                                     .Select(month => new FilterItemDate
                                     {
                                         Level = 2,
                                         Content = month.Key,
                                         Label = month.FirstOrDefault()?.Date.ToString("MMMM", Properties.Resources.Culture),
                                         Initialize = true, // default state
                                         FieldType = fieldType,

                                         Children = month.GroupBy(date => date.Date.Day)
                                             .Select(day => new FilterItemDate
                                             {
                                                 Level = 3,
                                                 Content = day.Key,
                                                 Label = day.FirstOrDefault()?.Date.ToString("dd", Properties.Resources.Culture),
                                                 Initialize = true, // default state
                                                 FieldType = fieldType,

                                                 // filter Item linked to the day, it propagates the states changes
                                                 Item = day.FirstOrDefault()?.Item,

                                                 Children = new List<FilterItemDate>()
                                             }).ToList()
                                     }).ToList()
                             }))
                {
                    // set parent and IsChecked property if uncheck Previous items
                    y.Children.ForEach(m =>
                    {
                        m.Parent = y;

                        m.Children.ForEach(d =>
                        {
                            d.Parent = m;

                            // set the state of the "IsChecked" property based on the items already filtered (unchecked)
                            if (d.Item.IsChecked) return;

                            // call the SetIsChecked method of the FilterItemDate class
                            d.IsChecked = false;

                            // reset with new state (isChanged == false)
                            d.Initialize = d.IsChecked;
                        });
                        // reset with new state
                        m.Initialize = m.IsChecked;
                    });
                    // reset with new state
                    y.Initialize = y.IsChecked;
                    tree.Add(y);
                }
                // last empty item if exist in collection
                if (dateTimes.Any(d => d.Level == -1))
                {
                    var empty = dateTimes.FirstOrDefault(x => x.Level == -1);
                    if (empty != null)
                        tree.Add(
                            new FilterItemDate
                            {
                                Label = Properties.Resources.Empty, // translation
                                Content = null,
                                Level = -1,
                                FieldType = fieldType,
                                Initialize = empty.IsChecked,
                                Item = empty,
                                Children = new List<FilterItemDate>()
                            }
                        );
                }
                tree.First().Tree = tree;
                return tree;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterCommon.BuildTree : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Remove current filter
        /// </summary>
        private void RemoveCurrentFilter()
        {
            if (CurrentFilter == null) return;

            popup.IsOpen = false; // raise PopupClosed event

            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);

            // button icon reset
            FilterState.SetIsFiltered(button, false);

            if (CurrentFilter.IsFiltered && criteria.Remove(CurrentFilter.FieldName))
                CollectionViewSource.Refresh();

            if (GlobalFilterList.Contains(CurrentFilter))
                _ = GlobalFilterList.Remove(CurrentFilter);

            // set the last filter applied
            lastFilter = GlobalFilterList.LastOrDefault()?.FieldName;

            CurrentFilter.IsFiltered = false;
            CurrentFilter = null;

            if (FilterState.GetIsFiltered(button) == true)
            {
                button.Tag = "True";
            }
            else
            {
                button.Tag = "false";
            }

        }

        #region 调整窗体大小代码
        /// <summary>
        ///     On Resize Thumb Drag Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Cursor = cursor;
        }

        /// <summary>
        ///     Get delta on drag thumb
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            // initialize the first Actual size Width/Height
            if (sizableContentHeight <= 0)
            {
                sizableContentHeight = sizableContentGrid.ActualHeight;
                sizableContentWidth = sizableContentGrid.ActualWidth;
            }

            var yAdjust = sizableContentGrid.Height + e.VerticalChange;
            var xAdjust = sizableContentGrid.Width + e.HorizontalChange;

            //make sure not to resize to negative width or heigth
            xAdjust = sizableContentGrid.ActualWidth + xAdjust > minWidth ? xAdjust : minWidth;
            yAdjust = sizableContentGrid.ActualHeight + yAdjust > minHeight ? yAdjust : minHeight;

            xAdjust = xAdjust < minWidth ? minWidth : xAdjust;
            yAdjust = yAdjust < minHeight ? minHeight : yAdjust;

            // set size of grid
            sizableContentGrid.Width = xAdjust;
            sizableContentGrid.Height = yAdjust;
        }

        /// <summary>
        ///     On Resize Thumb DragStarted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResizeThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            cursor = Cursor;
            Cursor = Cursors.SizeNWSE;
        }

        #endregion

        #region 命令事件

        /// <summary>
        /// 显示过滤窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            searchText = string.Empty;
            search = false;

            try
            {

                // filter button
                button = (Button)e.OriginalSource;

                if (Items.Count == 0 || button == null) return;


                // contribution : OTTOSSON
                // for the moment this functionality is not tested, I do not know if it can cause unexpected effects
                _ = CommitEdit(DataGridEditingUnit.Row, true);
                //CommitEdit();
                //CommitEdit(DataGridEditingUnit.Row, true);

                // navigate up to the current header and get column type
                var header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(button);
                var columnType = header.Column.GetType();

                #region 原代码
                // get field name from binding Path
                if (columnType == typeof(DataGridTextColumn))
                {
                    var column = (DataGridTextColumn)header.Column;
                    fieldName = column.FieldName;
                    //column.CanUserSort = false;
                    //currentColumn = column;
                    if (!column.IsColumnFiltered)
                    {
                        button.IsEnabled = false;
                        return;
                    }

                }

                if (columnType == typeof(DataGridTemplateColumn))
                {
                    var column = (DataGridTemplateColumn)header.Column;
                    fieldName = column.FieldName;
                    //column.CanUserSort = false;
                    //currentColumn = column;
                    if (!column.IsColumnFiltered)
                    {
                        button.IsEnabled = false;
                        return;
                    }
                }

                if (columnType == typeof(DataGridCheckBoxColumn))
                {
                    var column = (DataGridCheckBoxColumn)header.Column;
                    fieldName = column.FieldName;
                    //column.CanUserSort = false;
                    //currentColumn = column;
                    if (!column.IsColumnFiltered)
                    {
                        button.IsEnabled = false;
                        return;
                    }
                }

                #endregion



                // then down to the current popup
                popup = VisualTreeHelpers.FindChild<Popup>(header, "FilterPopup");
                columnHeadersPresenter = VisualTreeHelpers.FindAncestor<DataGridColumnHeadersPresenter>(header);

                if (popup == null || columnHeadersPresenter == null) return;


                // disable columnHeadersPresenter while popup is open
                if (columnHeadersPresenter != null)
                    columnHeadersPresenter.IsEnabled = false;

                // popup handle event
                popup.Closed += PopupClosed;

                //disable popup background clickthrough, contribution : WORDIBOI
                popup.MouseDown += onMousedown;

                // resizable grid
                sizableContentGrid = VisualTreeHelpers.FindChild<Grid>(popup.Child, "SizableContentGrid");

                // search textbox
                searchTextBox = VisualTreeHelpers.FindChild<TextBox>(popup.Child, "SearchBox");
                searchTextBox.Text = string.Empty;
                searchTextBox.TextChanged += SearchTextBoxOnTextChanged;
                searchTextBox.Focusable = true;

                // thumb resize grip
                thumb = VisualTreeHelpers.FindChild<Thumb>(sizableContentGrid, "PopupThumb");

                minHeight = sizableContentGrid.MinHeight;
                minWidth = sizableContentGrid.MinWidth;

                // thumb handle event
                thumb.DragCompleted += OnResizeThumbDragCompleted;
                thumb.DragDelta += OnResizeThumbDragDelta;
                thumb.DragStarted += OnResizeThumbDragStarted;

                // invalid fieldName
                if (string.IsNullOrEmpty(fieldName)) return;

                // get type of field
                fieldType = null;
                var fieldProperty = collectionType.GetProperty(fieldName);

                // get type or underlying type if nullable
                if (fieldProperty != null)
                    FieldType = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;

                // If no filter, add filter to GlobalFilterList list
                CurrentFilter = GlobalFilterList.FirstOrDefault(f => f.FieldName == fieldName) ??
                                new FilterCommon
                                {
                                    FieldName = fieldName,
                                    FieldType = fieldType,
                                    FieldProperty = fieldProperty
                                };

                // list of all item values, filtered and unfiltered (previous filtered items)
                var sourceObjectList = new List<object>();

                // set cursor
                Mouse.OverrideCursor = Cursors.Wait;

                List<FilterItem> filterItemList = null;//new List<FilterItem>();

                // get the list of values distinct from the list of raw values of the current column
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (fieldType == typeof(DateTime))
                        {
                            sourceObjectList = Items.Cast<object>()
                               .Select(x => (object)((DateTime?)fieldProperty?.GetValue(x, null))?.Date)
                               .Distinct()
                               .ToList();
                        }
                        else
                        {
                            sourceObjectList = Items.Cast<object>()
                              .Select(x => fieldProperty?.GetValue(x, null))
                              .Distinct()
                              .ToList();
                        }
                        //sourceObjectList = sourceObjectList.Distinct().ToList();
                    });

                    // adds the previous filtered items to the list of new items (CurrentFilter.PreviouslyFilteredItems)
                    if (lastFilter == CurrentFilter.FieldName)
                        sourceObjectList.AddRange(CurrentFilter?.PreviouslyFilteredItems ?? new HashSet<object>());

                    // empty item flag
                    // if they exist, remove all null or empty string values from the list.
                    // content == null and content == "" are two different things but both labeled as (blank)
                    var emptyItem = sourceObjectList.RemoveAll(v => v == null || v.Equals(string.Empty)) > 0;

                    // TODO : AggregateException when user can add row

                    // sorting is a very slow operation, using ParallelQuery
                    sourceObjectList = sourceObjectList.AsParallel().OrderBy(x => x).ToList();

                    filterItemList = new List<FilterItem>(sourceObjectList.Count);
                    if (fieldType == typeof(bool))
                    {
                        filterItemList = new List<FilterItem>(sourceObjectList.Count + 1);
                    }

                    // add all items (not null) to the filterItemList,
                    // the list of dates is calculated by BuildTree from this list
                    filterItemList.AddRange(sourceObjectList.Select(item => new FilterItem
                    {
                        Content = item,
                        ContentLength = item?.ToString()?.Length ?? 0,
                        FieldType = fieldType,
                        Label = item,
                        Level = 1,
                        Initialize = CurrentFilter.PreviouslyFilteredItems?.Contains(item) == false
                    }));


                    if (fieldType == typeof(bool))
                        filterItemList.ToList().ForEach(c =>
                        {
                            c.Label = (bool)c.Content ? Properties.Resources.IsTrue : Properties.Resources.IsFalse;
                        });

                    // add a empty item(if exist) at the bottom of the list
                    if (!emptyItem) return;

                    sourceObjectList.Insert(sourceObjectList.Count, null);

                    filterItemList.Add(new FilterItem
                    {
                        FieldType = fieldType,
                        Content = null,
                        Label = Properties.Resources.Empty,
                        Level = -1,
                        Initialize = CurrentFilter?.PreviouslyFilteredItems?.Contains(null) == false
                    });


                });

                List<bool> list_check = filterItemList.Select(t => t.IsChecked).Distinct().ToList();

                if (list_check.Count == 1)
                    SelectAllCheck = list_check[0];
                else
                    SelectAllCheck = null;
                list_check.Clear();

                // ItemsSource (ListBow/TreeView)
                if (fieldType == typeof(DateTime))
                {
                    ItemControlVisibility = Visibility.Collapsed;
                    TreeViewVisibility = Visibility.Visible;

                    TreeviewItems = BuildTree(filterItemList);
                }
                else
                {
                    ItemControlVisibility = Visibility.Visible;
                    TreeViewVisibility = Visibility.Collapsed;

                    ListBoxItems = filterItemList;
                }


                // Set ICollectionView for filtering in the pop-up window
                ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(filterItemList);

                // set filter in popup
                if (ItemCollectionView.CanFilter) ItemCollectionView.Filter = SearchFilter;

                // open popup
                popup.IsOpen = true;

                // set focus on searchTextBox
                searchTextBox.Focus();
                Keyboard.Focus(searchTextBox);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ShowFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            { Mouse.OverrideCursor = null; }

        }

        /// <summary>
        ///     Can show filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanShowFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CollectionViewSource?.CanFilter == true && (!popup?.IsOpen ?? true) && !pending;
        }

        /// <summary>
        ///     Click OK Button when Popup is Open, apply filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ApplyFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {

            pending = true;
            popup.IsOpen = false; // raise PopupClosed event

            // set cursor wait
            //Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                await Task.Run(() =>
                {
                    var previousFiltered = CurrentFilter.PreviouslyFilteredItems;
                    bool blankIsUnchecked;

                    if (search)
                    {
                        // 搜索结果
                        var searchResult = PopupViewItems.Where(c => c.IsChecked).ToList();

                        // 未选中 ：除 searchResult 之外的所有项目
                        var uncheckedItems = SourcePopupViewItems.Except(searchResult).ToList();
                        uncheckedItems.AddRange(searchResult.Where(c => c.IsChecked == false));

                        previousFiltered.ExceptWith(searchResult.Select(c => c.Content));

                        previousFiltered.UnionWith(uncheckedItems.Select(c => c.Content));

                        blankIsUnchecked = !uncheckedItems.Any(c => c.Level == -1);
                    }
                    else
                    {
                        // 更改的弹出项
                        var changedItems = PopupViewItems.Where(c => c.IsChanged).ToList();

                        var checkedItems = changedItems.Where(c => c.IsChecked);
                        var uncheckedItems = changedItems.Where(c => !c.IsChecked).ToList();

                        // 再次检查除未选中的项目外的上一个项目
                        previousFiltered.ExceptWith(checkedItems.Select(c => c.Content));
                        previousFiltered.UnionWith(uncheckedItems.Select(c => c.Content));

                        blankIsUnchecked = checkedItems.Any(c => c.Level == -1);
                    }


                    // 两个值 null 和 string.empty
                    if (CurrentFilter.FieldType != typeof(DateTime) &&
                        previousFiltered.Any(c => c == null || c.ToString() == string.Empty))
                    {
                        // 如果未选中 （blank） item，则添加string.Empty。
                        // 在此步骤中，之前已添加 Null 值
                        if (!blankIsUnchecked)
                        {
                            previousFiltered.Add(string.Empty);
                        }
                        // 如果重新选中 （blank） item，则删除string.Empty。
                        else
                        {
                            previousFiltered.RemoveWhere(item => item?.ToString() == string.Empty);
                        }

                    }

                    // 如果之前尚未添加筛选器，请添加筛选器
                    if (!CurrentFilter.IsFiltered) CurrentFilter.AddFilter(criteria);

                    // 将当前过滤器添加到 GlobalFilterList
                    if (GlobalFilterList.All(f => f.FieldName != CurrentFilter.FieldName))
                        GlobalFilterList.Add(CurrentFilter);

                    // 将 Current Field Name （当前字段名称） 设置为 Last Filter Name （最后一个筛选器名称）
                    lastFilter = CurrentFilter.FieldName;
                });

                // apply filter
                CollectionViewSource.Refresh();

                // 如果没有要筛选的项目，则删除当前筛选器
                if (!CurrentFilter.PreviouslyFilteredItems.Any())
                    RemoveCurrentFilter();

                // 设置按钮图标（筛选或未筛选）
                FilterState.SetIsFiltered(button, CurrentFilter?.IsFiltered ?? false);

                this.Focus();
                if (FilterState.GetIsFiltered(button) == true)
                {
                    button.Tag = "True";
                }
                else
                {
                    button.Tag = "False";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ApplyFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                ItemCollectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(new object());
                pending = false;
                CurrentFilter = null;
            }
        }

        /// <summary>
        ///     Can Apply filter (popup Ok button)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanApplyFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            // CanExecute only when the popup is open
            if ((popup?.IsOpen ?? false) == false)
            {
                e.CanExecute = false;
            }
            else
            {
                if (search)
                    e.CanExecute = PopupViewItems.Any(f => f?.IsChecked == true);
                else
                    e.CanExecute = PopupViewItems.Any(f => f.IsChanged) &&
                                   PopupViewItems.Any(f => f?.IsChecked == true);
            }
        }

        /// <summary>
        ///     Cancel button, close popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (popup == null) return;
            popup.IsOpen = false; // raise EventArgs PopupClosed
        }

        /// <summary>
        ///     Check/uncheck all item when the action is (select all)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckedAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // only when the item[0] (select all) is checked or unchecked
            if (ItemCollectionView == null) return;

            foreach (var obj in PopupViewItems.ToList()
                     .Where(f => f.IsChecked != SelectAllCheck))
                obj.IsChecked = (bool)SelectAllCheck;

            if (TreeViewVisibility == Visibility.Visible)
            {
                TreeviewItems = BuildTree(PopupViewItems);
            }
        }

        private void IsCheckedCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (ItemControlVisibility == Visibility.Visible)
            {
                var item = (FilterItem)e.Parameter;

                // only when the item[0] (select all) is checked or unchecked
                if (ItemCollectionView == null) return;

                int count = PopupViewItems.ToList().Where(f => f.IsChecked != item.IsChecked).Count();

                if (count > 0)
                {
                    SelectAllCheck = null;
                }
                else
                {
                    SelectAllCheck = item.IsChecked;
                }
            }
            else
            {
                var item = (FilterItemDate)e.Parameter;

                // only when the item[0] (select all) is checked or unchecked
                if (ItemCollectionView == null) return;

                int count = PopupViewItems.ToList().Where(f => f.IsChecked != item.IsChecked).Count();

                if (count > 0)
                {
                    SelectAllCheck = null;
                }
                else
                {
                    SelectAllCheck = item.IsChecked;
                }
            }
        }

        /// <summary>
        ///     remove current filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveCurrentFilter();
        }

        /// <summary>
        ///     Can remove filter when current column (CurrentFilter) filtered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanRemoveFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentFilter?.IsFiltered ?? false;
        }

        /// <summary>
        /// Remove All Filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveAllFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var mi = e.Parameter as MenuItem;
                var cm = mi.Parent as ContextMenu;
                var header = cm.PlacementTarget as DataGridColumnHeader;

                FilterDataGrid f = header.Tag as FilterDataGrid;

                f.CommitEdit();
                f.CommitEdit(DataGridEditingUnit.Row, true);


                foreach (var col in f.Columns)
                {
                    if (!AutoGenerateColumns)
                    {
                        switch (col)
                        {
                            case DataGridTextColumn ctxt when ctxt.IsColumnFiltered:
                                fieldName = ctxt.FieldName;
                                break;

                            case DataGridTemplateColumn ctpl when ctpl.IsColumnFiltered:
                                fieldName = ctpl.FieldName;
                                break;

                            case DataGridCheckBoxColumn chk when chk.IsColumnFiltered:
                                fieldName = chk.FieldName;
                                break;

                            case null:
                                continue;
                        }
                    }
                    else
                    {
                        switch (col)
                        {
                            case DataGridTextColumn ctxt when ctxt.IsColumnFiltered:
                                fieldName = ctxt.Header.ToString();
                                break;

                            case DataGridTemplateColumn ctpl when ctpl.IsColumnFiltered:
                                fieldName = ctpl.Header.ToString();
                                break;

                            case DataGridCheckBoxColumn chk when chk.IsColumnFiltered:
                                fieldName = chk.Header.ToString();
                                break;

                            case null:
                                continue;
                        }
                    }


                    button = VisualTreeHelpers.GetHeader(col, f)
                        ?.FindVisualChild<Button>("FilterButton");

                    if (button == null || string.IsNullOrEmpty(fieldName)) continue;

                    button.Tag = "false";

                    CurrentFilter = GlobalFilterList.FirstOrDefault(c => c.FieldName == fieldName);

                    if (CurrentFilter != null) RemoveCurrentFilter();
                    if (CurrentCustomizeFilters != null) CurrentCustomizeFilters.Clear();

                    f.CollectionViewSource.Refresh();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.RemoveAllFilterCommand error : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Can remove all filter when GlobalFilterList.Count > 0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanRemoveAllFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalFilterList.Count > 0 || CurrentCustomizeFilters.Count > 0;
        }

        private void RemoveThisFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CommitEdit(DataGridEditingUnit.Row, true);
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            string fieldName1 = DataGridColumnHelper.DataGridHeaderToString(header);
            if (criteria.Remove(fieldName1))
                CollectionViewSource.Refresh();

            foreach (var item in GlobalFilterList)
            {
                if (item.FieldName == fieldName1)
                {
                    GlobalFilterList.Remove(item);
                    break;
                }
            }

            // set the last filter applied
            lastFilter = GlobalFilterList.LastOrDefault()?.FieldName;

            Button button1 = VisualTreeHelpers.FindChild<Button>(header, "FilterButton");
            button1.Tag = "false";

        }

        private static List<CustomizeFilterColumn> lists = new List<CustomizeFilterColumn>();
        private void CustomizeFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {

            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;

            FilterDataGrid f = header.Tag as FilterDataGrid;

            f.CommitEdit();
            f.CommitEdit(DataGridEditingUnit.Row, true);

            if (lists.Count == 0)
            {
                foreach (var item in f.Columns)
                {
                    if (item.GetType() == typeof(DataGridTextColumn))
                    {
                        var column = (DataGridTextColumn)item;
                        if (column.IsColumnFiltered)
                        {
                            Type columntype = null;
                            var fieldProperty = collectionType.GetProperty(column.FieldName);
                            if (fieldProperty != null)
                                columntype = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;
                            lists.Add(new CustomizeFilterColumn()
                            {
                                Header = item.Header.ToString(),
                                FieldName = column.FieldName,
                                ColumnType = columntype,
                            });
                        }
                    }

                    if (item.GetType() == typeof(DataGridCheckBoxColumn))
                    {
                        var column = (DataGridCheckBoxColumn)item;
                        if (column.IsColumnFiltered)
                        {
                            Type columntype = null;
                            var fieldProperty = collectionType.GetProperty(column.FieldName);
                            if (fieldProperty != null)
                                columntype = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;
                            lists.Add(new CustomizeFilterColumn()
                            {
                                Header = item.Header.ToString(),
                                FieldName = column.FieldName,
                                ColumnType = columntype,
                            });
                        }
                    }

                    if (item.GetType() == typeof(DataGridTemplateColumn))
                    {
                        var column = (DataGridTemplateColumn)item;
                        if (column.IsColumnFiltered)
                        {
                            Type columntype = null;
                            var fieldProperty = collectionType.GetProperty(column.FieldName);
                            if (fieldProperty != null)
                                columntype = Nullable.GetUnderlyingType(fieldProperty.PropertyType) ?? fieldProperty.PropertyType;
                            lists.Add(new CustomizeFilterColumn()
                            {
                                Header = item.Header.ToString(),
                                FieldName = column.FieldName,
                                ColumnType = columntype,
                            });
                        }

                    }
                }
            }




            Point position = f.PointToScreen(new Point(0d, 0d));
            CustomizeSerach customizeSerach = new CustomizeSerach(lists)
            {
                ColumnsList = lists,
                Left = position.X + this.ActualWidth / 3,
                Top = position.Y,
            };
            if (customizeSerach.ShowDialog() == true)
            {
                CurrentCustomizeFilters = customizeSerach.CurrentCustomizeFilters;
                if (CurrentCustomizeFilters.Count == 0)
                {

                    foreach (var col in f.Columns)
                    {
                        if (col.GetType() == typeof(DataGridTextColumn))
                        {
                            DataGridTextColumn dataGridText = col as DataGridTextColumn;


                            if (!criteria.Keys.Contains(dataGridText.FieldName))
                            {
                                Button btn = VisualTreeHelpers.GetHeader(col, f)?.FindVisualChild<Button>("FilterButton");
                                if (btn == null || col == null) continue;
                                btn.Tag = "false";
                            }
                        }
                        else if (col.GetType() == typeof(DataGridCheckBoxColumn))
                        {
                            DataGridCheckBoxColumn dataGridText = col as DataGridCheckBoxColumn;
                            if (!criteria.Keys.Contains(dataGridText.FieldName))
                            {
                                Button btn = VisualTreeHelpers.GetHeader(col, f)?.FindVisualChild<Button>("FilterButton");
                                if (btn == null || col == null) continue;
                                btn.Tag = "false";
                            }
                        }

                    }
                }
                else
                {
                    foreach (var item in CurrentCustomizeFilters)
                    {
                        foreach (var col in f.Columns)
                        {
                            if (col.GetType() == typeof(DataGridTextColumn))
                            {
                                DataGridTextColumn dataGridText = col as DataGridTextColumn;
                                if (dataGridText.FieldName == item.ColumnsList.FieldName)
                                {
                                    Button btn = VisualTreeHelpers.GetHeader(col, f)?.FindVisualChild<Button>("FilterButton");
                                    if (btn == null || col == null) continue;
                                    btn.Tag = "True";
                                }
                            }
                            else if (col.GetType() == typeof(DataGridCheckBoxColumn))
                            {
                                DataGridCheckBoxColumn dataGridText = col as DataGridCheckBoxColumn;
                                if (dataGridText.FieldName == item.ColumnsList.FieldName)
                                {
                                    Button btn = VisualTreeHelpers.GetHeader(col, f)?.FindVisualChild<Button>("FilterButton");
                                    if (btn == null || col == null) continue;
                                    btn.Tag = "True";
                                }
                            }
                        }
                    }
                }



                f.CollectionViewSource.Refresh();
            }
        }

        private void CanCustomizeFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CanUserFilter) e.CanExecute = true; else e.CanExecute = false;
        }

        /// <summary>
        /// Can remove all filter when GlobalFilterList.Count > 0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanRemoveThisFilter(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            string fieldName1 = DataGridColumnHelper.DataGridHeaderToString(header);
            e.CanExecute = GlobalFilterList.Where(x => x.FieldName == fieldName1).Count() > 0;
        }

        #region 排序设置

        private void AscendingSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            //var mi = e.Parameter as MenuItem;
            //var cm = mi.Parent as ContextMenu;
            //var header = cm.PlacementTarget as DataGridColumnHeader;

            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;

            f.CommitEdit();
            f.CommitEdit(DataGridEditingUnit.Row, true);

            header.Column.SortDirection = ListSortDirection.Ascending;
            List<SortDescription> items = f.CollectionViewSource.SortDescriptions.Where(x => x.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header)).ToList();
            foreach (var item in items)
            {
                f.CollectionViewSource.SortDescriptions.Remove(item);
            }

            f.CollectionViewSource.SortDescriptions.Add(new SortDescription(DataGridColumnHelper.DataGridHeaderToString(header), ListSortDirection.Ascending));
            f.groupSortItemList.Add(new GroupSortItem() { FieldName = DataGridColumnHelper.DataGridHeaderToString(header), SortDirection = ListSortDirection.Ascending });
            f.Focus();
            Mouse.OverrideCursor = null;
        }

        private void CanAscendingSort(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;

            List<SortDescription> listSort = CollectionViewSource.SortDescriptions.Cast<SortDescription>().ToList();
            int count1 = listSort.Where(f => f.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header) && f.Direction == ListSortDirection.Ascending).Count();

            if (count1 == 0)
                e.CanExecute = true;

            if (header.Column.GetType() == typeof(DataGridTemplateColumn))
                e.CanExecute = false;
        }

        private void DescendingSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;

            f.CommitEdit();
            f.CommitEdit(DataGridEditingUnit.Row, true);

            header.Column.SortDirection = ListSortDirection.Descending;
            List<SortDescription> items = f.CollectionViewSource.SortDescriptions.Where(x => x.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header)).ToList();
            foreach (var item in items)
            {
                f.CollectionViewSource.SortDescriptions.Remove(item);
            }

            f.CollectionViewSource.SortDescriptions.Add(new SortDescription(DataGridColumnHelper.DataGridHeaderToString(header), ListSortDirection.Descending));

            f.groupSortItemList.Add(new GroupSortItem() { FieldName = DataGridColumnHelper.DataGridHeaderToString(header), SortDirection = ListSortDirection.Descending });
            f.Focus();
            Mouse.OverrideCursor = null;
        }

        private void CanDescendingSort(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;

            List<SortDescription> listSort = CollectionViewSource.SortDescriptions.Cast<SortDescription>().ToList();
            int count1 = listSort.Where(f => f.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header) && f.Direction == ListSortDirection.Descending).Count();

            if (count1 == 0)
                e.CanExecute = true;
            if (header.Column.GetType() == typeof(DataGridTemplateColumn))
                e.CanExecute = false;
        }
        private void RemoveSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;
            f.CommitEdit();
            f.CommitEdit(DataGridEditingUnit.Row, true);

            header.Column.SortDirection = null;

            List<SortDescription> items = f.CollectionViewSource.SortDescriptions.Where(x => x.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header)).ToList();
            List<GroupSortItem> sortItems = f.groupSortItemList.Where(x => x.FieldName == DataGridColumnHelper.DataGridHeaderToString(header)).ToList();

            foreach (var item in items)
            {
                f.CollectionViewSource.SortDescriptions.Remove(item);
            }

            foreach (var item in sortItems)
            {
                f.groupSortItemList.Remove(item);
            }
            f.Focus();
            Mouse.OverrideCursor = null;
        }

        private void CanRemoveSort(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;


            List<SortDescription> listSort = f.CollectionViewSource.SortDescriptions.Cast<SortDescription>().ToList();
            int count1 = listSort.Where(x => x.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header)).Count();
            if (count1 > 0)
                e.CanExecute = true;
        }

        private void RemoveAllSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;

            f.CommitEdit();
            f.CommitEdit(DataGridEditingUnit.Row, true);

            foreach (DataGridColumn columnHeader in f.Columns)
            {
                columnHeader.SortDirection = null;
            }
            f.CollectionViewSource.SortDescriptions.Clear();
            f.groupSortItemList.Clear();
            f.Focus();
        }

        private void CanRemoveAllSort(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;

            List<SortDescription> listSort = f.CollectionViewSource.SortDescriptions.Cast<SortDescription>().ToList();
            int count1 = listSort.Count();

            if (count1 > 0)
                e.CanExecute = true;
        }

        private void ClickSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);

            var mi = e.OriginalSource as Grid;
            var header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(mi);

            if (header.Column.GetType() == typeof(DataGridTemplateColumn))
            {
                Mouse.OverrideCursor = null;
                return;
            }


            foreach (DataGridColumn columnHeader in this.Columns)
            {
                if (columnHeader.DisplayIndex != header.Column.DisplayIndex)
                {
                    columnHeader.SortDirection = null;
                }
            }
            CollectionViewSource.SortDescriptions.Clear();
            groupSortItemList.Clear();

            if (header.Column.SortDirection == ListSortDirection.Ascending)
            {
                header.Column.SortDirection = ListSortDirection.Descending;
                CollectionViewSource.SortDescriptions.Add(new SortDescription(DataGridColumnHelper.DataGridHeaderToString(header), ListSortDirection.Descending));

                groupSortItemList.Add(new GroupSortItem() { FieldName = DataGridColumnHelper.DataGridHeaderToString(header), SortDirection = ListSortDirection.Descending });

            }
            else
            {
                header.Column.SortDirection = ListSortDirection.Ascending;
                CollectionViewSource.SortDescriptions.Add(new SortDescription(DataGridColumnHelper.DataGridHeaderToString(header), ListSortDirection.Ascending));

                groupSortItemList.Add(new GroupSortItem() { FieldName = DataGridColumnHelper.DataGridHeaderToString(header), SortDirection = ListSortDirection.Ascending });

            }

            this.Focus();

            Mouse.OverrideCursor = null;
        }

        #endregion

        /// <summary>
        /// 打开全局搜索面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchPanelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;
            f.SearchPanelVisibility = Visibility.Visible;
        }
        /// <summary>
        /// 能否打开全局搜索面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanSearchPanel(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;
            if (f.Items.Count > 0 && f.SearchPanelVisibility == Visibility.Collapsed)
                e.CanExecute = true;
        }
        /// <summary>
        /// 关闭全局搜索面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseSearchPanelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SearchPanelVisibility = Visibility.Collapsed;
        }

        private void ClearSearchWordCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);

            (e.Parameter as TextBox).Text = "";
            globalSearchWord = null;
            HiText = "";

            if (PageBarVisibility == Visibility.Visible)
            {
                RaiseEvent(new RoutedEventArgs(CancelGlobalSearchEvent, this));
            }

            CollectionViewSource.Refresh();

        }

        private void AppleSearchWordCommand(object sender, ExecutedRoutedEventArgs e)
        {

            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);
            globalSearchWord = ((TextBox)e.Parameter).Text;
            HiText = globalSearchWord;
            if (PageBarVisibility == Visibility.Visible && !string.IsNullOrEmpty(HiText))
            {
                RaiseEvent(new RoutedEventArgs(ApplyGlobalSearchEvent, this));
            }

            CollectionViewSource.Refresh();

        }

        #region 分页相关代码
        private void NextPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentPageDisplay = CurrentPage = CurrentPage + 1;
            SwitchPage();
        }

        private void CanNextPage(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CurrentPage >= TotalPages) e.CanExecute = false; else e.CanExecute = true;
        }

        private void PrevPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentPageDisplay = CurrentPage = CurrentPage - 1;
            SwitchPage();
        }

        private void CanPrevPage(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CurrentPage <= 1) e.CanExecute = false; else e.CanExecute = true;
        }

        private void HomePageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentPageDisplay = CurrentPage = 1;
            SwitchPage();
        }

        private void CanHomePage(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CurrentPage <= 1) e.CanExecute = false; else e.CanExecute = true;
        }

        private void EndPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentPageDisplay = CurrentPage = TotalPages;
            SwitchPage();
        }

        private void CanEndPage(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CurrentPage >= TotalPages) e.CanExecute = false; else e.CanExecute = true;
        }

        private void GoToPageCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentPage = CurrentPageDisplay;
            SwitchPage();
        }

        private void ChangePageSizeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentPage = CurrentPageDisplay = 1;
            PageSize = Convert.ToInt32(e.Parameter);
            SwitchPage();
        }

        private void SwitchPage()
        {
            RaiseEvent(new RoutedEventArgs(PageChangedEvent, this));
        }
        #endregion

        #region 分组相关代码

        private void GroupByColumnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;

            if (f.CollectionViewSource != null && f.CollectionViewSource.CanGroup == true)
            {
                // set cursor
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {

                    f.CommitEdit();
                    f.CommitEdit(DataGridEditingUnit.Row, true);


                    string groupName;
                    if (AutoGenerateColumns == false)
                    {

                        DataGridTextColumn dataGridTextColumn = header.Column as DataGridTextColumn;
                        groupName = dataGridTextColumn.FieldName.ToString();

                    }
                    else
                    {
                        groupName = header.Column.Header.ToString();
                    }
                    f.CollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(groupName));
                    f.CollectionViewSource.Refresh();

                    f.GroupColumnsList.Add(groupName);

                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

            }
        }

        private void CanGroupByColumn(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CanUserGroup)
            {
                var mi = e.Parameter as MenuItem;
                var cm = mi.Parent as ContextMenu;
                var header = cm.PlacementTarget as DataGridColumnHeader;
                if (header.Column.GetType() == typeof(DataGridTextColumn))
                    e.CanExecute = true;
                else { e.CanExecute = false; }
            }
            else e.CanExecute = false;
        }

        private void ClearGroupCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;

            if (f.CollectionViewSource != null && f.CollectionViewSource.CanGroup == true)
            {
                CommitEdit();
                CommitEdit(DataGridEditingUnit.Row, true);

                f.CollectionViewSource.GroupDescriptions.Clear();
                f.CollectionViewSource.Refresh();

                f.GroupColumnsList.Clear();
            }
        }

        private void CanClearGroup(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;

            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;
            if (f.CanUserGroup && f.GroupColumnsList.Count > 0) e.CanExecute = true; else e.CanExecute = false;
        }

        private async void ExpandedAllGroupCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;

            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;
            await Task.Run(() =>
              {


                  f.Dispatcher.BeginInvoke(new Action(() =>
                   { Mouse.OverrideCursor = Cursors.Wait; }));

                  f.ExpanderAll = true;
                  OnPropertyChanged(nameof(ExpanderAll));

                  f.Dispatcher.BeginInvoke(new Action(() =>
                { f.CollectionViewSource.Refresh(); Mouse.OverrideCursor = null; }));
              });
        }

        private void CanExpandedAllGroup(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;

            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;
            if (f.CanUserGroup && f.GroupColumnsList.Count > 0 && f.ExpanderAll == false) e.CanExecute = true; else e.CanExecute = false;
        }

        private void CollapsedAllGroupCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;

            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;
            Task.Run(() =>
             {
                 this.Dispatcher.BeginInvoke(new Action(() => { Mouse.OverrideCursor = Cursors.Wait; }));

                 f.ExpanderAll = false;
                 OnPropertyChanged(nameof(f.ExpanderAll));

                 this.Dispatcher.BeginInvoke(new Action(() => { f.CollectionViewSource.Refresh(); Mouse.OverrideCursor = null; }));
             });
        }

        private void CanCollapsedAllGroup(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;

            var header = cm.PlacementTarget as DataGridColumnHeader;
            FilterDataGrid f = header.Tag as FilterDataGrid;
            if (f.CanUserGroup && f.GroupColumnsList.Count > 0 && f.ExpanderAll == true) e.CanExecute = true; else e.CanExecute = false;
        }
        #endregion

        private void InterfaceSettingsCommand(object sender, ExecutedRoutedEventArgs e)
        {

            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;

            FilterDataGrid f = header.Tag as FilterDataGrid;

            ColumnsCollection = new List<ColumnProperty>();
            foreach (var item in this.Columns)
            {
                ColumnProperty columnProperty = new ColumnProperty()
                {
                    ColumnSort = item.SortDirection.ToString(),
                    Name = item.Header.ToString(),
                    Index = item.DisplayIndex,
                    Columnwidth = item.ActualWidth,
                    Visibility = item.Visibility,
                    ColumnType = item.GetType(),
                    Alignment = "Default",
                };

                if (item.GetType() == typeof(DataGridTextColumn))
                {
                    DataGridTextColumn column = (DataGridTextColumn)item;
                    columnProperty.FieldName = column.FieldName;
                }
                else if (item.GetType() == typeof(DataGridCheckBoxColumn))
                {
                    DataGridCheckBoxColumn column = (DataGridCheckBoxColumn)item;
                    columnProperty.FieldName = column.FieldName;
                }
                else if (item.GetType() == typeof(DataGridTemplateColumn))
                {
                    DataGridTemplateColumn column = (DataGridTemplateColumn)item;
                    columnProperty.FieldName = column.FieldName;
                }

                string filePath = AppDomain.CurrentDomain.BaseDirectory + "Layout/" + this.Name + ".xml";
                if (File.Exists(filePath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);
                    XmlNode root = doc.SelectSingleNode("DataGrid");
                    if (root.SelectSingleNode("Columns").SelectSingleNode(columnProperty.FieldName) == null) continue;
                    XmlNode xmlNode = root.SelectSingleNode("Columns").SelectSingleNode(columnProperty.FieldName).SelectSingleNode("Alignment");
                    columnProperty.Alignment = xmlNode.InnerText;
                }
                ColumnsCollection.Add(columnProperty);
            }

            Point position = f.PointToScreen(new Point(0d, 0d));

            LayoutPopup layoutPopup = new LayoutPopup(f)
            {
                DataContext = f,
                customGroups = GroupColumnsList,
                groupSortItems = groupSortItemList,
                Left = position.X + f.ActualWidth / 2,
                Top = position.Y,
            };
            layoutPopup.ShowDialog();

        }

        private void CanInterfaceSettings(object sender, CanExecuteRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Name)) e.CanExecute = false; else e.CanExecute = true;
        }


        private void ShowDragSumPanelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MenuItem menuItem = e.Parameter as MenuItem;
            var cm = menuItem.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;

            FilterDataGrid f = header.Tag as FilterDataGrid;
            if (menuItem.IsChecked)
            {
                f.SelectionUnit = DataGridSelectionUnit.Cell;
                f.SelectionMode = DataGridSelectionMode.Extended;
                f.ShowDragSum = Visibility.Visible;
                f.SelectedCellsChanged += FilterDataGrid_SelectedCellsChanged;
            }
            else
            {
                f.SelectionUnit = dataGridSelectionUnit;
                f.SelectionMode = dataGridSelectionMode;
                f.ShowDragSum = Visibility.Collapsed;
                f.SelectedCellsChanged -= FilterDataGrid_SelectedCellsChanged;

            }

        }

        private void CanShowDragSumPanel(object sender, CanExecuteRoutedEventArgs e)
        {
            MenuItem menuItem = e.Parameter as MenuItem;
            var cm = menuItem.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;

            FilterDataGrid f = header.Tag as FilterDataGrid;
            if (!f.CanDragSum) e.CanExecute = false; else e.CanExecute = true;
        }

        #endregion


        #region 方法（外部调用）

        private System.Threading.Timer timerClose;

        public IEnumerable<T> PageInitialized<T>(IEnumerable<T> initializedData)
        {
            CurrentPageDisplay = CurrentPage = 1;
            TotalPages = (int)Math.Ceiling((double)initializedData.Count() / PageSize);
            return initializedData.Take(PageSize);
        }

        public IEnumerable<T> PageSwitched<T>(IEnumerable<T> changedData)
        {
            TotalPages = (int)Math.Ceiling((double)changedData.Count() / PageSize);
            return changedData.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        }

        public void SumAndCount()
        {
            isNumber = false;
            double total = 0;

            var cells = (List<DataGridCellInfo>)(this.SelectedCells.ToList());
            var columns = cells.GroupBy(x => x.Column).Select(y => y.First()).ToList();
            var selectCells = cells.Cast<DataGridCellInfo>().Select(x => x.Item).ToList();

            foreach (var item in columns)
            {
                if (item.Column.GetType() == typeof(DataGridTextColumn))
                {
                    var textColumn = (DataGridTextColumn)item.Column;
                    var fieldProperty = collectionType.GetProperty(textColumn.FieldName);
                    if (fieldProperty.PropertyType == typeof(Int32?) || fieldProperty.PropertyType == typeof(double?) ||
                        fieldProperty.PropertyType == typeof(Int32) || fieldProperty.PropertyType == typeof(double))
                    {
                        var sumList = selectCells.Cast<object>().Select(x => fieldProperty?.GetValue(x, null)).ToList();
                        List<double> list_int = sumList.ConvertAll(c => { return Convert.ToDouble(c); }).ToList();
                        total += list_int.Sum();
                        isNumber = true;
                    }
                }
            }

            if (isNumber)
                MouseDragSum = Properties.Resources.Count + (cells.Count).ToString("n0") + "\r\n" +
                    Properties.Resources.Avg + ((total / columns.Count) / cells.Count).ToString("n2") + "\r\n" +
                    Properties.Resources.Sum + (total / columns.Count).ToString("n2");
            else
                MouseDragSum = Properties.Resources.Count + (cells.Count).ToString("n0") + "\r\n" +
                    Properties.Resources.Avg + "\r\n" +
                    Properties.Resources.Sum;

        }

        public void ExportToExcel(string filename)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = ".xlsx|*.xlsx";
            saveFileDialog.FileName = filename;
            if (saveFileDialog.ShowDialog() == true)
            {
                var config = new OpenXmlConfiguration { };
                List<DynamicExcelColumn> objs = new List<DynamicExcelColumn>();
                foreach (var column in this.Columns)
                {
                    if (column.GetType() == typeof(DataGridTextColumn))
                    {
                        var dataGridTextColumn = (DataGridTextColumn)column;
                        objs.Add(new DynamicExcelColumn(dataGridTextColumn.FieldName) { Width = column.ActualWidth / 8, Name = dataGridTextColumn.Header.ToString() });
                    }
                    else if (column.GetType() == typeof(DataGridCheckBoxColumn))
                    {
                        var dataGridCheckBoxColumn = (DataGridCheckBoxColumn)column;
                        objs.Add(new DynamicExcelColumn(dataGridCheckBoxColumn.FieldName) { Width = column.ActualWidth / 8, Name = dataGridCheckBoxColumn.Header.ToString() });
                    }

                }
                config.DynamicColumns = objs.ToArray();
                config.AutoFilter = false;

                MiniExcel.SaveAs(saveFileDialog.FileName, CollectionViewSource, overwriteFile: true, configuration: config);

                ExportTipMessage = string.Format(Properties.Resources.ExportTip, this.Items.Count);
                //Border border = VisualTreeHelpers.FindChild<Border>(this, "Bor_ExportTipBorder");
                Border border = this.Template.FindName("Bor_ExportTipBorder", this) as Border;
                border.Visibility = Visibility.Visible;
                Storyboard sbd = border.TryFindResource("ExportTipAnimation") as Storyboard;
                sbd.Begin();

                timerClose = new System.Threading.Timer(new TimerCallback(TimerCall), this, 5000, 0);
            }

        }

        private void TimerCall(object obj)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                Border border = VisualTreeHelpers.FindChild<Border>(this, "Bor_ExportTipBorder");
                Storyboard sbd = border.TryFindResource("ExportTipAnimation2") as Storyboard;
                sbd.Completed += (s, e) => { border.Visibility = Visibility.Collapsed; };
                sbd.Begin();

                timerClose.Dispose();
            }));
        }



        #endregion
    }
}
