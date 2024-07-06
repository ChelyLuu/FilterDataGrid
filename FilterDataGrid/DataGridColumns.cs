using System.Windows;
using System.Windows.Controls.Primitives;

namespace FilterDataGrid
{
    public sealed class DataGridTemplateColumn : System.Windows.Controls.DataGridTemplateColumn
    {
        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register("FieldName", typeof(string), typeof(DataGridTemplateColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
                    DependencyProperty.Register("IsColumnFiltered", typeof(bool), typeof(DataGridTemplateColumn),
                new PropertyMetadata(false));

        #endregion Public Fields

        #region Public Properties

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)GetValue(IsColumnFilteredProperty);
            set => SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties
    }

    public class DataGridTextColumn : System.Windows.Controls.DataGridTextColumn
    {

        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register("FieldName", typeof(string), typeof(DataGridTextColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
                    DependencyProperty.Register("IsColumnFiltered", typeof(bool), typeof(DataGridTextColumn),
                new PropertyMetadata(true));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty DataFormatStringProperty =
                    DependencyProperty.Register("DataFormatString", typeof(string), typeof(DataGridTextColumn),
                new PropertyMetadata(""));


        #endregion Public Fields

        #region Public Properties

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)GetValue(IsColumnFilteredProperty);
            set => SetValue(IsColumnFilteredProperty, value);
        }

        public string DataFormatString
        {
            get => (string)GetValue(DataFormatStringProperty);
            set => SetValue(DataFormatStringProperty, value);
        }

        #endregion Public Properties




    }

    public class DataGridCheckBoxColumn : System.Windows.Controls.DataGridCheckBoxColumn
    {

        #region Public Fields

        /// <summary>
        /// FieldName Dependency Property.
        /// </summary>
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register("FieldName", typeof(string), typeof(DataGridCheckBoxColumn),
                new PropertyMetadata(""));

        /// <summary>
        /// IsColumnFiltered Dependency Property.
        /// </summary>
        public static readonly DependencyProperty IsColumnFilteredProperty =
            DependencyProperty.Register("IsColumnFiltered", typeof(bool), typeof(DataGridCheckBoxColumn),
                new PropertyMetadata(true));

        #endregion Public Fields

        #region Public Properties

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        public bool IsColumnFiltered
        {
            get => (bool)GetValue(IsColumnFilteredProperty);
            set => SetValue(IsColumnFilteredProperty, value);
        }

        #endregion Public Properties
    }

    public static class DataGridColumnHelper
    {
        public static string DataGridHeaderToString(DataGridColumnHeader header)
        {
            var columnType = header.Column.GetType();
            string fieldName = null;
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
            return fieldName;
        }

        public static string DataGridColumnToString(System.Windows.Controls.DataGridColumn column)
        {
            var columnType = column.GetType();
            string fieldName = null;
            if (columnType == typeof(DataGridTextColumn))
            {
                var filtercolumn = (DataGridTextColumn)column;
                fieldName = filtercolumn.FieldName;
                //column.CanUserSort = false;
                //currentColumn = column;

            }

            if (columnType == typeof(DataGridTemplateColumn))
            {
                var filtercolumn = (DataGridTemplateColumn)column;
                fieldName = filtercolumn.FieldName;
            }

            if (columnType == typeof(DataGridCheckBoxColumn))
            {
                var filtercolumn = (DataGridCheckBoxColumn)column;
                fieldName = filtercolumn.FieldName;
            }
            return fieldName;
        }
    }
}
