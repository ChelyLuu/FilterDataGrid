using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace FilterDataGrid
{
    /// <summary>
    /// Attached Property "FilterState" to Filter Button
    /// </summary>
    public class FilterState : DependencyObject
    {
        #region Public Fields

        public static readonly DependencyProperty IsFilteredProperty = DependencyProperty.RegisterAttached("IsFiltered",
            typeof(bool), typeof(FilterState), new UIPropertyMetadata(false));

        #endregion Public Fields

        #region Public Methods

        public static bool GetIsFiltered(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFilteredProperty);
        }

        public static void SetIsFiltered(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFilteredProperty, value);
        }

        #endregion Public Methods
    }

    public static class Extensions
    {
        #region Public Methods

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

        #endregion Public Methods
    }


    public static class VisualTreeHelpers
    {
        #region Private Methods

        public static T FindVisualChild<T>(this DependencyObject dependencyObject, string name)
                    where T : DependencyObject
        {
            // Search immediate children first (breadth-first)
            var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

            //http://stackoverflow.com/questions/12304904/why-visualtreehelper-getchildrencount-returns-0-for-popup

            if (childrenCount == 0 && dependencyObject is Popup)
            {
                var popup = dependencyObject as Popup;
                return popup.Child?.FindVisualChild<T>(name);
            }

            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                var nameOfChild = child.GetValue(FrameworkElement.NameProperty) as string;

                if (child is T && (name == string.Empty || name == nameOfChild))
                    return (T)child;
                var childOfChild = child.FindVisualChild<T>(name);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        private static IEnumerable<T> GetChildrenOf<T>(this DependencyObject obj, bool recursive) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T) yield return (T)child;

                if (recursive)
                    foreach (var item in child.GetChildrenOf<T>())
                        yield return item;
            }
        }

        private static IEnumerable<T> GetChildrenOf<T>(this DependencyObject obj) where T : DependencyObject
        {
            return obj.GetChildrenOf<T>(false);
        }


        public static List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }


        /// <summary>
        ///     This method is an alternative to WPF's
        ///     <see cref="VisualTreeHelper.GetParent" /> method, which also
        ///     supports content elements. Keep in mind that for content element,
        ///     this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>
        ///     The submitted item's parent, if available. Otherwise
        ///     null.
        /// </returns>
        private static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            var contentElement = child as ContentElement;
            if (contentElement != null)
            {
                var parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                var fce = contentElement as FrameworkContentElement;
                return fce?.Parent;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            var frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                var parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        ///     Returns the first ancester of specified type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            current = VisualTreeHelper.GetParent(current);

            while (current != null)
            {
                if (current is T) return (T)current;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        ///     Returns a specific ancester of an object
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, T lookupItem)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T && current == lookupItem) return (T)current;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        ///     Finds an ancestor object by name and type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, string parentName)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (!string.IsNullOrEmpty(parentName))
                {
                    var frameworkElement = current as FrameworkElement;
                    if (current is T && frameworkElement != null && frameworkElement.Name == parentName)
                        return (T)current;
                }
                else if (current is T)
                {
                    return (T)current;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        ///     Looks for a child control within a parent by name
        /// </summary>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }

                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        ///     Looks for a child control within a parent by type
        /// </summary>
        public static T FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static T FindVisualChild<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            return dependencyObject.FindVisualChild<T>(string.Empty);
        }

        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            Visual foundElement = null;
            if (element is FrameworkElement frameworkElement) frameworkElement.ApplyTemplate();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null) break;
            }

            return foundElement;
        }

        public static DataGridColumnHeader GetHeader(DataGridColumn column, DependencyObject reference)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(reference); i++)
            {
                var child = VisualTreeHelper.GetChild(reference, i);

                if (child is DataGridColumnHeader colHeader && colHeader.Column == column) return colHeader;

                colHeader = GetHeader(column, child);
                if (colHeader != null) return colHeader;
            }

            return null;
        }

        /// <summary>
        ///     Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">
        ///     A direct or indirect child of the
        ///     queried item.
        /// </param>
        /// <returns>
        ///     The first parent item that matches the submitted
        ///     type parameter. If not matching item can be found, a null
        ///     reference is being returned.
        /// </returns>
        public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            var parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            var parent = parentObject as T;
            if (parent != null)
                return parent;
            return TryFindParent<T>(parentObject);
        }

        #endregion Public Methods

        public static ChildItem FindVisualChildItem<ChildItem>(DependencyObject obj, string name) where ChildItem : FrameworkElement
        {
            if (null != obj)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null && child is ChildItem && (child as ChildItem).Name.Equals(name))
                    {
                        return (ChildItem)child;
                    }
                    else
                    {
                        ChildItem childOfChild = FindVisualChildItem<ChildItem>(child, name);
                        if (childOfChild != null && childOfChild is ChildItem && (childOfChild as ChildItem).Name.Equals(name))
                        {
                            return childOfChild;
                        }
                    }
                }
            }
            return null;
        }
    }

    /// <summary>
    ///     Base class for all ViewModel classes in the application. Provides support for
    ///     property changes notification.
    /// </summary>
    [Serializable]
    public abstract class NotifyProperty : INotifyPropertyChanged
    {
        #region Public Events

        /// <summary>
        ///     Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Private Methods

        /// <summary>
        ///     Warns the developer if this object does not have a public property with
        ///     the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyPropertyName(string propertyName)
        {
            // verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
                Debug.Fail("Invalid property name: " + propertyName);
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has a new value.</param>
        public void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Public Methods
    }

    public class FooterItem
    {
        public string FieldName { get; set; }

        public string FormatString { get; set; }

        public TotalType TotalType { get; set; }

    }

    public enum TotalType
    {
        Sum, Ave, Count
    };

    public class ColumnProperty
    {
        public string Name { get; set; }

        public string FieldName { get; set; }

        public int Index { get; set; }

        public double Columnwidth { get; set; }

        public Visibility Visibility { get; set; }

        public Type ColumnType { get; set; }

        public string Alignment { get; set; }

        public string ColumnSort { get; set; }

    }

    public class GroupSortItem
    {
        public string FieldName { get; set; }

        public ListSortDirection SortDirection { get; set; }

        public static ListSortDirection StringToSortDirdection(string sortString)
        {
            if (sortString == "Ascending")
            {
                return ListSortDirection.Ascending;
            }
            else
            {
                return ListSortDirection.Descending;
            }
        }
    }


    public class Pager
    {
        private readonly CollectionView _view;
        private readonly int _pageSize;
        private int _currentPage;
        public Pager(CollectionView view, int pageSize)
        {
            _view = view;
            _pageSize = pageSize;
            _currentPage = 1;
            _view.Filter = FilterMethod;
        }

        public ICollectionView View => _view;
        public void MoveToPage(int page)
        {
            if (page < 1 || page > PageCount)
                return;
            _currentPage = page;
            _view.Refresh();
        }
        public bool FilterMethod(object obj)
        {
            var index = _view.IndexOf(obj);
            return index >= (_currentPage - 1) * _pageSize && index < _currentPage * _pageSize;
        }

        public int PageCount => (int)Math.Ceiling((double)_view.Count / _pageSize);
        public int CurrentPage => _currentPage;
    }

    public class CustomizeFilter
    {
        public bool EnableAO { get; set; }
        public string AndOr { get; set; }
        public CustomizeFilterColumn ColumnsList { get; set; }
        public string OperatorID { get; set; }
        public string Operator { get; set; }
        public string Condition { get; set; }
    }

    public class CustomizeFilterColumn
    {
        public string Header { get; set; }
        public string FieldName { get; set; }
        public Type ColumnType { get; set; }
    }

    public class OperatorString
    {
        public string ID { get; set; }
        public string IcoText { get; set; }
        public string OperatorText { get; set; }
    }
}
