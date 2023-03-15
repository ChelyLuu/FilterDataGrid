using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace FilterDataGrid
{
   public class FilterDataGrid : DataGrid, INotifyPropertyChanged
    {
        public FilterDataGrid()
        {
            this.KeyDown += FilterDataGrid_KeyDown_Search;

            CommandBindings.Add(new CommandBinding(ShowFilter, ShowFilterCommand, CanShowFilter));
            CommandBindings.Add(new CommandBinding(ApplyFilter, ApplyFilterCommand, CanApplyFilter)); // Ok
            CommandBindings.Add(new CommandBinding(CancelFilter, CancelFilterCommand));
            CommandBindings.Add(new CommandBinding(CheckedAll, CheckedAllCommand));
            CommandBindings.Add(new CommandBinding(IsChecked, IsCheckedCommand));
            CommandBindings.Add(new CommandBinding(RemoveFilter, RemoveFilterCommand, CanRemoveFilter));
            CommandBindings.Add(new CommandBinding(RemoveAllFilter, RemoveAllFilterCommand, CanRemoveAllFilter));
            CommandBindings.Add(new CommandBinding(RemoveThisFilter, RemoveThisFilterCommand, CanRemoveThisFilter));


            CommandBindings.Add(new CommandBinding(AscendingSort, AscendingSortCommand, CanAscendingSort));
            CommandBindings.Add(new CommandBinding(DescendingSort, DescendingSortCommand, CanDescendingSort));
            CommandBindings.Add(new CommandBinding(RemoveSort, RemoveSortCommand, CanRemoveSort));
            CommandBindings.Add(new CommandBinding(RemoveAllSort, RemoveAllSortCommand, CanRemoveAllSort));
            CommandBindings.Add(new CommandBinding(ClickSort, ClickSortCommand));

            CommandBindings.Add(new CommandBinding(SearchPanel, SearchPanelCommand, CanSearchPanel));
            CommandBindings.Add(new CommandBinding(CloseSearchPanel, CloseSearchPanelCommand));
            CommandBindings.Add(new CommandBinding(ClearSearchWord, ClearSearchWordCommand));
            CommandBindings.Add(new CommandBinding(AppleSearchWord, AppleSearchWordCommand));

        }

        #region 命令
        public static readonly ICommand ShowFilter = new RoutedCommand();
        public static readonly ICommand ApplyFilter = new RoutedCommand();
        public static readonly ICommand CancelFilter = new RoutedCommand();
        public static readonly ICommand CheckedAll = new RoutedCommand();
        public static readonly ICommand IsChecked = new RoutedCommand();
        public static readonly ICommand RemoveFilter = new RoutedCommand();
        public static readonly ICommand RemoveAllFilter = new RoutedCommand();
        public static readonly ICommand RemoveThisFilter = new RoutedCommand();

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

        #endregion

        #region INotifyPropertyChanged实现接口
        /// <summary>
        ///     OnPropertyChange
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Private Fields

        private ScrollViewer TotalScroll;
        private DataGridColumnHeadersPresenter columnHeadersPresenter;
        private bool pending;
        private bool search;
        private Button button;
        private const bool DebugMode = false;
        private Cursor cursor;

        private double minHeight;
        private double minWidth;
        private double sizableContentHeight;
        private double sizableContentWidth;
        private Grid sizableContentGrid;

        private List<FilterItemDate> treeview;
        private List<FilterItem> listBoxItems;

        private DataGrid TotalRow;
        private Popup popup;
      
        private string globalSearchWord;
        private string fieldName;
        private string lastFilter;
        private string searchText;
        private TextBox searchTextBox;
        private Thumb thumb;

        private Type collectionType;
        private Type fieldType;


        private readonly Dictionary<string, Predicate<object>> criteria = new Dictionary<string, Predicate<object>>();

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        ///     Language
        /// </summary>
        public Local FilterLanguage
        {
            get => (Local)GetValue(FilterLanguageProperty);
            set => SetValue(FilterLanguageProperty, value);
        }

        /// <summary>
        ///     Display items count
        /// </summary>
        public int ItemsSourceCount { get; set; }

        /// <summary>
        ///     Show status bar
        /// </summary>
        public bool ShowStatusBar
        {
            get => (bool)GetValue(ShowStatusBarProperty);
            set => SetValue(ShowStatusBarProperty, value);
        }

        /// <summary>
        ///     Show rows count
        /// </summary>
        public bool ShowRowsCount
        {
            get => (bool)GetValue(ShowRowsCountProperty);
            set => SetValue(ShowRowsCountProperty, value);
        }

        /// <summary>
        ///     Show rows count
        /// </summary>
        public List<FooterItem> FooterItems
        {
            get => (List<FooterItem>)GetValue(FooterItemsProperty);
            set => SetValue(FooterItemsProperty, value);
        }

        /// <summary>
        ///     Instance of Loc
        /// </summary>
        public Loc Translate { get; private set; }

        /// <summary>
        /// Treeview ItemsSource
        /// </summary>
        public List<FilterItemDate> TreeviewItems
        {
            get => treeview ?? new List<FilterItemDate>();
            set
            {
                treeview = value;
                OnPropertyChanged(nameof(TreeviewItems));
            }
        }

        /// <summary>
        /// ListBox ItemsSource
        /// </summary>
        public List<FilterItem> ListBoxItems
        {
            get => listBoxItems ?? new List<FilterItem>();
            set
            {
                listBoxItems = value;
                OnPropertyChanged(nameof(ListBoxItems));
            }
        }

        public Type FieldType
        {
            get => fieldType;
            set
            {
                fieldType = value;
                OnPropertyChanged();
            }
        }

        private bool? _selectAllCheck = false;
        public bool? SelectAllCheck
        {
            get { return _selectAllCheck; }
            set { _selectAllCheck = value; OnPropertyChanged(nameof(SelectAllCheck)); }
        }

        private Visibility _itemControlVisibility = Visibility.Visible;
        public Visibility ItemControlVisibility
        {
            get { return _itemControlVisibility; }
            set { _itemControlVisibility = value; OnPropertyChanged(nameof(ItemControlVisibility)); }
        }


        private Visibility _treeViewVisibility = Visibility.Collapsed;
        public Visibility TreeViewVisibility
        {
            get { return _treeViewVisibility; }
            set { _treeViewVisibility = value; OnPropertyChanged(nameof(TreeViewVisibility)); }
        }


        private Visibility _searchPanelVisibility = Visibility.Collapsed;
        public Visibility SearchPanelVisibility
        {
            get { return _searchPanelVisibility; }
            set { _searchPanelVisibility = value; OnPropertyChanged(nameof(SearchPanelVisibility)); }
        }


        private DataView totalRowItemSource;
        public DataView TotalRowItemSource
        {
            get { return totalRowItemSource; }
            set { totalRowItemSource = value; OnPropertyChanged(nameof(TotalRowItemSource)); }
        }

        public Visibility ShowTotalFooter { get; set; } = Visibility.Collapsed;


        #endregion Public Properties

        #region Private Properties

        private FilterCommon CurrentFilter { get; set; }
        private ICollectionView CollectionViewSource { get; set; }
        private ICollectionView ItemCollectionView { get; set; }
        private List<FilterCommon> GlobalFilterList { get; } = new List<FilterCommon>();

        /// <summary>
        /// Popup filtered items (ListBox/TreeView)
        /// </summary>
        private IEnumerable<FilterItem> PopupViewItems =>
            ItemCollectionView?.OfType<FilterItem>().Where(c => c.Level != 0) ?? new List<FilterItem>();

        /// <summary>
        /// Popup source collection (ListBox/TreeView)
        /// </summary>
        private IEnumerable<FilterItem> SourcePopupViewItems =>
            ItemCollectionView?.SourceCollection.OfType<FilterItem>().Where(c => c.Level != 0) ?? new List<FilterItem>();

        #endregion Private Properties

        #region Public DependencyProperty


        /// <summary>
        ///     Excluded Fields on AutoColumn
        /// </summary>
        public static readonly DependencyProperty ExcludeFieldsProperty =
            DependencyProperty.Register("ExcludeFields",
                typeof(string),
                typeof(FilterDataGrid),
                new PropertyMetadata(""));

        /// <summary>
        ///     Language displayed
        /// </summary>
        public static readonly DependencyProperty FilterLanguageProperty =
            DependencyProperty.Register("FilterLanguage",
                typeof(Local),
                typeof(FilterDataGrid),
                new PropertyMetadata(Local.English));


        /// <summary>
        ///     Show statusbar
        /// </summary>
        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register("ShowStatusBar",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        /// <summary>
        ///     Show Rows Count
        /// </summary>
        public static readonly DependencyProperty ShowRowsCountProperty =
            DependencyProperty.Register("ShowRowsCount",
                typeof(bool),
                typeof(FilterDataGrid),
                new PropertyMetadata(false));

        /// <summary>
        ///     Show Rows Count
        /// </summary>
        public static readonly DependencyProperty FooterItemsProperty =
            DependencyProperty.Register("FooterItems",
                typeof(List<FooterItem>),
                typeof(FilterDataGrid),
                new PropertyMetadata(new List<FooterItem>()));


        #endregion Public DependencyProperty


        /// <summary>
        ///     Initialize datagrid
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {

            base.OnInitialized(e);

            try
            {
                Translate = new Loc { Language = FilterLanguage };
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

                    // scroll to top on reload collection
                    //var scrollViewer = GetTemplateChild("DG_ScrollViewer") as ScrollViewer;
                    //scrollViewer?.ScrollToTop();
                }

                CollectionViewSource = System.Windows.Data.CollectionViewSource.GetDefaultView(ItemsSource);

                // set Filter, contribution : STEFAN HEIMEL
                if (CollectionViewSource.CanFilter) CollectionViewSource.Filter = AllFilter;

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


                #region Total
                if (FooterItems.Count > 0)
                {

                    ShowTotalFooter = Visibility.Visible;
                    OnPropertyChanged(nameof(ShowTotalFooter));

                    ScrollViewer sv = base.GetTemplateChild("DG_ScrollViewer") as ScrollViewer;
                    sv.ScrollChanged += new ScrollChangedEventHandler(Sv_ScrollChanged); ;

                    TotalRow = (DataGrid)sv.Template.FindName("TotalRow", sv);

                    int displayindex = 0;
                    TotalRow.Columns?.Clear();

                    foreach (var item in this.Columns)
                    {
                        System.Windows.Controls.DataGridTextColumn cl = new System.Windows.Controls.DataGridTextColumn
                        {
                            Header = item.Header,
                            Width = item.Width,
                            DisplayIndex = item.DisplayIndex = displayindex++
                        };

                        Binding widthBd = new Binding
                        {
                            Source = item,
                            Mode = BindingMode.TwoWay,
                            Path = new PropertyPath(DataGridColumn.WidthProperty)
                        };
                        BindingOperations.SetBinding(cl, System.Windows.Controls.DataGridTextColumn.WidthProperty, widthBd);

                        Binding visibleBd = new Binding
                        {
                            Source = item,
                            Mode = BindingMode.TwoWay,
                            Path = new PropertyPath(DataGridColumn.VisibilityProperty)
                        };
                        BindingOperations.SetBinding(cl, System.Windows.Controls.DataGridTextColumn.VisibilityProperty, visibleBd);

                        Binding indexBd = new Binding
                        {
                            Source = item,
                            Mode = BindingMode.TwoWay,
                            Path = new PropertyPath(DataGridColumn.DisplayIndexProperty)
                        };
                        BindingOperations.SetBinding(cl, System.Windows.Controls.DataGridTextColumn.DisplayIndexProperty, indexBd);

                        if (item.GetType().Name == "DataGridTextColumn")
                            cl.Binding = (item as System.Windows.Controls.DataGridTextColumn).Binding;
                        else if (item.GetType().Name == "DataGridCheckBoxColumn")
                            cl.Binding = (item as System.Windows.Controls.DataGridCheckBoxColumn).Binding;


                        TotalRow.Columns.Add(cl);
                    }

                    TotalScroll = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(TotalRow, 0), 0) as ScrollViewer;
                    CollectionViewSource.Refresh();
                }
                #endregion


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.OnItemsSourceChanged : {ex.Message}");
                throw;
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
                        Binding = new Binding(e.PropertyName) { ConverterCulture = Translate.Culture /* StringFormat */ },
                        FieldName = e.PropertyName,
                        Header = e.Column.Header.ToString(),
                        IsColumnFiltered = true
                    };

                    //Binding binding = new Binding(column.DataFormartString);
                    ////fieldType = Nullable.GetUnderlyingType(e.PropertyType) ?? e.PropertyType;

                    //// apply the format string provided
                    ////if (fieldType == typeof(DateTime))
                    //column.ClipboardContentBinding.StringFormat = "";

                    e.Column = column;
                }
                if (e.Column.GetType() == typeof(System.Windows.Controls.DataGridCheckBoxColumn))
                {
                    var column = new DataGridCheckBoxColumn
                    {
                        Binding = new Binding(e.PropertyName) { ConverterCulture = Translate.Culture /* StringFormat */ },
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
                throw;
            }
        }

        /// <summary>
        ///     Adding Rows count
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoadingRow(DataGridRowEventArgs e)
        {
            if (ShowRowsCount)
                e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = (ScrollViewer)sender;
            TotalScroll.ScrollToHorizontalOffset(sv.HorizontalOffset);
        }

        private void CollectionViewSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (FooterItems.Count > 0)
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
                foreach (PropertyInfo property in ps)
                {
                    List<FooterItem> fis = FooterItems.Where(x => x.FieldName == property.Name).ToList();
                    if (fis.Count() > 0)
                    {
                        FooterItem fi = fis[0];
                        double totalValue = 0;
                        foreach (var item in Items)
                        {
                            object tmpValue = property.GetValue(item, null);

                            if (fi.TotalType == totalType.Sum)
                            {
                                if (property.PropertyType == typeof(int))
                                {
                                    totalValue = (double)tmpValue + (double)totalValue;
                                }
                                else if (property.PropertyType == typeof(double))
                                {
                                    totalValue = (double)tmpValue + (double)totalValue;
                                }
                            }
                            else if (fi.TotalType == totalType.Count)
                            {
                                totalValue = Items.Count;
                            }
                            else
                            {
                                totalValue = (double)tmpValue + (double)totalValue;
                            }

                        }
                        if (fi.TotalType == totalType.Ave)
                        {
                            totalValue = totalValue / Items.Count;
                        }
                        dataTable.Rows[0][property.Name] = ((double)totalValue).ToString(fi.FormatString);
                    }

                }
                TotalRowItemSource = dataTable.DefaultView;
            }
        }

        #region 命令绑定

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
                //_ = CommitEdit(DataGridEditingUnit.Row, true);
                CommitEdit();
                CommitEdit(DataGridEditingUnit.Row, true);

                // navigate up to the current header and get column type
                var header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(button);
                var columnType = header.Column.GetType();

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


                #region 原代码
                // get field name from binding Path
                if (columnType == typeof(DataGridTextColumn))
                {
                    var column = (DataGridTextColumn)header.Column;
                    fieldName = column.FieldName;
                    //column.CanUserSort = false;
                    //currentColumn = column;

                }

                if (columnType == typeof(DataGridTemplateColumn))
                {
                    var column = (DataGridTemplateColumn)header.Column;
                    fieldName = column.FieldName;
                    //column.CanUserSort = false;
                    //currentColumn = column;
                }

                if (columnType == typeof(DataGridCheckBoxColumn))
                {
                    var column = (DataGridCheckBoxColumn)header.Column;
                    fieldName = column.FieldName;
                    //column.CanUserSort = false;
                    //currentColumn = column;

                }

                #endregion


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
                                    Translate = Translate,
                                    FieldProperty = fieldProperty
                                };

                // list of all item values, filtered and unfiltered (previous filtered items)
                var sourceObjectList = new List<object>();


                List<FilterItem> filterItemList = null;//new List<FilterItem>();

                // get the list of values distinct from the list of raw values of the current column
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        int itemCount;
                        if (IsReadOnly == true || CanUserAddRows == false) itemCount = Items.Count; else itemCount = Items.Count - 1;

                        if (fieldType == typeof(DateTime))
                        {
                            for (int i = 0; i < itemCount; i++)
                            {
                                sourceObjectList.Add((object)((DateTime?)fieldProperty?.GetValue(Items[i]))?.Date);
                            }
                        }

                        else
                        {
                            for (int i = 0; i < itemCount; i++)
                            {
                                sourceObjectList.Add(fieldProperty?.GetValue(Items[i]));
                            }
                        }
                        sourceObjectList = sourceObjectList.Distinct().ToList();
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
                            c.Label = (bool)c.Content ? Translate.IsTrue : Translate.IsFalse;
                        });

                    // add a empty item(if exist) at the bottom of the list
                    if (!emptyItem) return;

                    sourceObjectList.Insert(sourceObjectList.Count, null);

                    filterItemList.Add(new FilterItem
                    {
                        FieldType = fieldType,
                        Content = null,
                        Label = Translate.Empty,
                        Level = -1,
                        Initialize = CurrentFilter?.PreviouslyFilteredItems?.Contains(null) == false
                    });
                });

                SelectAllCheck = filterItemList[0].IsChecked;
                foreach (var item in filterItemList)
                {
                    if (item.IsChecked != SelectAllCheck)
                    {
                        SelectAllCheck = null;
                    }
                }


                // ItemsSource (ListBow/TreeView)
                if (fieldType == typeof(DateTime))
                {
                    ItemControlVisibility = Visibility.Collapsed;
                    TreeViewVisibility = Visibility.Visible;
                    OnPropertyChanged("ItemControlVisibility");
                    OnPropertyChanged("TreeViewVisibility");
                    TreeviewItems = BuildTree(filterItemList);
                }
                else
                {
                    ItemControlVisibility = Visibility.Visible;
                    TreeViewVisibility = Visibility.Collapsed;
                    OnPropertyChanged("ItemControlVisibility");
                    OnPropertyChanged("TreeViewVisibility");
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
                        // result of the research
                        var searchResult = PopupViewItems.Where(c => c.IsChecked).ToList();

                        // unchecked : all items except searchResult
                        var uncheckedItems = SourcePopupViewItems.Except(searchResult).ToList();
                        uncheckedItems.AddRange(searchResult.Where(c => c.IsChecked == false));

                        previousFiltered.ExceptWith(searchResult.Select(c => c.Content));

                        previousFiltered.UnionWith(uncheckedItems.Select(c => c.Content));

                        blankIsUnchecked = uncheckedItems.Any(c => c.Level == -1);
                    }
                    else
                    {
                        // changed popup items
                        var changedItems = PopupViewItems.Where(c => c.IsChanged).ToList();

                        var checkedItems = changedItems.Where(c => c.IsChecked);
                        var uncheckedItems = changedItems.Where(c => !c.IsChecked).ToList();

                        // previous item except unchecked items checked again
                        previousFiltered.ExceptWith(checkedItems.Select(c => c.Content));
                        previousFiltered.UnionWith(uncheckedItems.Select(c => c.Content));

                        blankIsUnchecked = uncheckedItems.Any(c => c.Level == -1);
                    }

                    // two values, null and string.empty
                    if (CurrentFilter.FieldType != typeof(DateTime) &&
                        previousFiltered.Any(c => c == null || c.ToString() == string.Empty))
                    {
                        // if (blank) item is unchecked, add string.Empty.
                        // at this step, the null value is already added previously
                        if (blankIsUnchecked)
                            previousFiltered.Add(string.Empty);

                        // if (blank) item is rechecked, remove string.Empty.
                        else
                            previousFiltered.RemoveWhere(item => item?.ToString() == string.Empty);
                    }

                    // add a filter if it is not already added previously
                    if (!CurrentFilter.IsFiltered) CurrentFilter.AddFilter(criteria);

                    // add current filter to GlobalFilterList
                    if (GlobalFilterList.All(f => f.FieldName != CurrentFilter.FieldName))
                        GlobalFilterList.Add(CurrentFilter);

                    // set the current field name as the last filter name
                    lastFilter = CurrentFilter.FieldName;
                });

                // apply filter
                CollectionViewSource.Refresh();

                // remove the current filter if there is no items to filter
                if (!CurrentFilter.PreviouslyFilteredItems.Any())
                    RemoveCurrentFilter();

                // set button icon (filtered or not)
                FilterState.SetIsFiltered(button, CurrentFilter?.IsFiltered ?? false);

                this.Focus();
                if (FilterState.GetIsFiltered(button) == true)
                {
                    button.Tag = "True";
                }
                else
                {
                    button.Tag = "false";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FilterDataGrid.ApplyFilterCommand error : {ex.Message}");
                throw;
            }
            finally
            {
                //ReactivateSorting();
                //ResetCursor();
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

                OnPropertyChanged("SelectAllCheck");
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

                OnPropertyChanged("SelectAllCheck");
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

                foreach (var col in Columns)
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


                    button = VisualTreeHelpers.GetHeader(col, this)
                        ?.FindVisualChild<Button>("FilterButton");

                    if (button == null || string.IsNullOrEmpty(fieldName)) continue;

                    button.Tag = "false";

                    CurrentFilter = GlobalFilterList.FirstOrDefault(c => c.FieldName == fieldName);

                    if (CurrentFilter != null) RemoveCurrentFilter();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(DebugMode, $"FilterDataGrid.RemoveAllFilterCommand error : {ex.Message}");
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
            e.CanExecute = GlobalFilterList.Count > 0;
        }

        private void RemoveThisFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
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
            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);

            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            List<SortDescription> items = CollectionViewSource.SortDescriptions.Where(f => f.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header)).ToList();
            foreach (var item in items)
            {
                CollectionViewSource.SortDescriptions.Remove(item);
            }
            TextBlock text = VisualTreeHelpers.FindChild<TextBlock>(header, "SortUPArrow");
            text.Tag = "Asc";
            CollectionViewSource.SortDescriptions.Add(new SortDescription(DataGridColumnHelper.DataGridHeaderToString(header), ListSortDirection.Ascending));
            this.Focus();
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
        }

        private void DescendingSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);

            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            List<SortDescription> items = CollectionViewSource.SortDescriptions.Where(f => f.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header)).ToList();
            foreach (var item in items)
            {
                CollectionViewSource.SortDescriptions.Remove(item);
            }
            TextBlock text = VisualTreeHelpers.FindChild<TextBlock>(header, "SortUPArrow");
            text.Tag = "Desc";
            CollectionViewSource.SortDescriptions.Add(new SortDescription(DataGridColumnHelper.DataGridHeaderToString(header), ListSortDirection.Descending));
            this.Focus();
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
        }

        private void RemoveSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);

            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;
            List<SortDescription> items = CollectionViewSource.SortDescriptions.Where(f => f.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header)).ToList();
            foreach (var item in items)
            {
                CollectionViewSource.SortDescriptions.Remove(item);
            }
            TextBlock text = VisualTreeHelpers.FindChild<TextBlock>(header, "SortUPArrow");
            text.Tag = "Null";
            this.Focus();
        }

        private void CanRemoveSort(object sender, CanExecuteRoutedEventArgs e)
        {
            var mi = e.Parameter as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var header = cm.PlacementTarget as DataGridColumnHeader;

            List<SortDescription> listSort = CollectionViewSource.SortDescriptions.Cast<SortDescription>().ToList();
            int count1 = listSort.Where(f => f.PropertyName == DataGridColumnHelper.DataGridHeaderToString(header)).Count();
            if (count1 > 0)
                e.CanExecute = true;
        }

        private void RemoveAllSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);
            List<DataGridColumnHeader> columnHeaders = VisualTreeHelpers.GetVisualChildCollection<DataGridColumnHeader>(this);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                TextBlock text = VisualTreeHelpers.FindChild<TextBlock>(columnHeader, "SortUPArrow");
                text.Tag = "Null";
            }
            CollectionViewSource.SortDescriptions.Clear();

            this.Focus();
        }

        private void CanRemoveAllSort(object sender, CanExecuteRoutedEventArgs e)
        {

            List<SortDescription> listSort = CollectionViewSource.SortDescriptions.Cast<SortDescription>().ToList();
            int count1 = listSort.Count();

            if (count1 > 0)
                e.CanExecute = true;
        }

        private void ClickSortCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);

            var mi = e.OriginalSource as Grid;
            var header = VisualTreeHelpers.FindAncestor<DataGridColumnHeader>(mi);

            List<DataGridColumnHeader> columnHeaders = VisualTreeHelpers.GetVisualChildCollection<DataGridColumnHeader>(this);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                if (columnHeader == null) continue;
                if (columnHeader != header)
                {
                    TextBlock text1 = VisualTreeHelpers.FindChild<TextBlock>(columnHeader, "SortUPArrow");
                    text1.Tag = "Null";
                }
            }
            CollectionViewSource.SortDescriptions.Clear();

            TextBlock text = VisualTreeHelpers.FindChild<TextBlock>(mi, "SortUPArrow");
            if (text.Tag.ToString() == "Asc")
            {
                CollectionViewSource.SortDescriptions.Add(new SortDescription(DataGridColumnHelper.DataGridHeaderToString(header), ListSortDirection.Descending));
                text.Tag = "Desc";
            }
            else
            {
                CollectionViewSource.SortDescriptions.Add(new SortDescription(DataGridColumnHelper.DataGridHeaderToString(header), ListSortDirection.Ascending));
                text.Tag = "Asc";
            }

            this.Focus();
        }

        #endregion

        private void SearchPanelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SearchPanelVisibility = Visibility.Visible;
        }

        private void CanSearchPanel(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Items.Count > 0 && SearchPanelVisibility == Visibility.Collapsed)
                e.CanExecute = true;
        }

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
            CollectionViewSource.Refresh();
        }

        private void AppleSearchWordCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CommitEdit();
            CommitEdit(DataGridEditingUnit.Row, true);

            globalSearchWord = ((TextBox)e.Parameter).Text;
            CollectionViewSource.Refresh();

            HighlighterKeyWords(globalSearchWord, this);
        }

     

        #endregion




        public bool AllFilter(object sourceObject)
        {
            if (globalSearchWord == null)
            {
                return Filter(sourceObject);
            }
            else
            {
                return Filter2(sourceObject) && Filter(sourceObject);
            }

        }

        private bool Filter2(object sourceObject)
        {
            Type typeName = sourceObject.GetType();
            PropertyInfo[] ps = typeName.GetProperties();
            bool filter = ps[0].GetValue(sourceObject, null).ToString().Contains(globalSearchWord);
            for (int i = 1; i < ps.Count(); i++)
            {
                filter = filter || ps[i].GetValue(sourceObject, null).ToString().Contains(globalSearchWord);
            }
            return filter;
        }

        private void HighlighterKeyWords(string keyWords, DependencyObject depObj)
        {
            if (String.IsNullOrEmpty(keyWords))
                return;

            IEnumerable<TextBlock> tbList = FindVisualChildren<TextBlock>(depObj);
            foreach (TextBlock tb in tbList)
            {

                string[] arr = keyWords.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                StringBuilder pattern = new StringBuilder();
                foreach (string item in arr)
                {
                    pattern.AppendFormat("({0})+|", item);
                }
                Regex regex = new Regex(pattern.ToString().TrimEnd('|'), RegexOptions.IgnoreCase);

                string[] substrings = regex.Split(tb.Text);
                tb.Inlines.Clear();
                foreach (var item in substrings)
                {

                    if (regex.Match(item).Success)
                    {
                        var paletteHelper = new PaletteHelper();
                        ITheme theme = paletteHelper.GetTheme();
                        Run runx = new Run(item);
                        runx.Background = new SolidColorBrush(theme.PrimaryMid.Color);
                        tb.Inlines.Add(runx);
                    }
                    else
                    {
                        tb.Inlines.Add(item);
                    }
                }
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
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
                                 Label = year.FirstOrDefault()?.Date.ToString("yyyy", Translate.Culture),
                                 Initialize = true, // default state
                                 FieldType = fieldType,

                                 Children = year.GroupBy(date => date.Date.Month)
                                     .Select(month => new FilterItemDate
                                     {
                                         Level = 2,
                                         Content = month.Key,
                                         Label = month.FirstOrDefault()?.Date.ToString("MMMM", Translate.Culture),
                                         Initialize = true, // default state
                                         FieldType = fieldType,

                                         Children = month.GroupBy(date => date.Date.Day)
                                             .Select(day => new FilterItemDate
                                             {
                                                 Level = 3,
                                                 Content = day.Key,
                                                 Label = day.FirstOrDefault()?.Date.ToString("dd", Translate.Culture),
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
                                Label = Translate.Empty, // translation
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
        ///     Aggregate list of predicate as filter
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool Filter(object o)
        {
            return criteria.Values
                   .Aggregate(true, (prevValue, predicate) => prevValue && predicate(o));
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
        ///     Filter current list in popup
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool SearchFilter(object obj)
        {
            var item = (FilterItem)obj;
            if (string.IsNullOrEmpty(searchText) || item == null || item.Level == 0) return true;

            var content = Convert.ToString(item.Content, Translate.Culture);

            // Contains

            return Translate.Culture.CompareInfo.IndexOf(content ?? string.Empty, searchText, CompareOptions.OrdinalIgnoreCase) >= 0;

            // StartsWith preserve RangeOverflow
            //if (searchLength > item.ContentLength) return false;

            //return Translate.Culture.CompareInfo.IndexOf(content ?? string.Empty, searchText, 0, searchLength, CompareOptions.OrdinalIgnoreCase) >= 0;
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

        private void FilterDataGrid_KeyDown_Search(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
                SearchPanelVisibility = Visibility.Visible;
        }


    }
}
