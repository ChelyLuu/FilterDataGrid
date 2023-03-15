using FilterDataGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FilterDataGridDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
             this.DataContext = this;
        }
        public ObservableCollection<Student> dvList { get; set; }
        public List<FooterItem> footerList { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name");
            dt.Columns.Add("age", typeof(int));
            dt.Columns.Add("birthdate", typeof(DateTime));
            dt.Columns.Add("phone");
            dt.Columns.Add("address");
            dt.Columns.Add("marry", typeof(bool));
            dt.Columns.Add("good", typeof(bool));
            dt.Columns.Add("score", typeof(double));
            dt.Columns.Add("remark");


            dt.Rows.Add("张三", "19", "2004-10-19", "18617082298", "安徽安庆", false, true, 89);
            dt.Rows.Add("李四", "20", "2003-6-10", "18617081238", "安徽芜湖", false, false, 90);
            dt.Rows.Add("王五", "19", "2004-8-19", "18617083567", "广东深圳", false, false, 87);
            dt.Rows.Add("王小红", "18", "2005-7-10", "18617087656", "广东广州", false, true, 88);
            dt.Rows.Add("朱小倩", "17", "2006-9-6", "18617087899", "安徽安庆", false, true, 90);
            dt.Rows.Add("黄先生", "21", "2002-10-8", "18617088766", "广东惠州", false, true, 85);
            dt.Rows.Add("李小姐", "19", "2004-3-17", "18617088765", "安徽安庆", false, false, 88);
            dt.Rows.Add("曾小贤", "26", "1992-6-22", "18617089088", "安徽安庆", true, true, 91.786);
            dt.Rows.Add("胡一菲", "20", "2004-7-5", "18617081272", "安徽合肥", false, true, 90);
            dt.Rows.Add("林婉如", "23", "2001-3-6", "18617081236", "安徽芜湖", true, true, 86);

            footerList = new List<FooterItem>()
            {
                new FooterItem() {  FieldName = "Score", TotalType=totalType.Ave, FormatString = "平均：0.00" }
                //new FooterItem() {  FieldName = "Good", TotalType=totalType.Sum }
            };
            OnPropertyChanged("footerList");

            this.dvList = ToObservableCollection<Student>(dt);
            OnPropertyChanged("dvList");

            DataGrid.ItemsSource = dt.DefaultView;

        }



        public ObservableCollection<T> ToObservableCollection<T>(DataTable dt) where T : class, new()
        {
            Type t = typeof(T);
            PropertyInfo[] propertys = t.GetProperties();
            ObservableCollection<T> lst = new ObservableCollection<T>();
            string typeName = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                T entity = new T();
                foreach (PropertyInfo pi in propertys)
                {
                    typeName = pi.Name;
                    if (dt.Columns.Contains(typeName))
                    {
                        if (!pi.CanWrite) continue;
                        object value = dr[typeName];
                        if (value == DBNull.Value) continue;
                        if (pi.PropertyType == typeof(string))
                        {
                            pi.SetValue(entity, value.ToString(), null);
                        }
                        else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(int?))
                        {
                            pi.SetValue(entity, int.Parse(value.ToString()), null);
                        }
                        else if (pi.PropertyType == typeof(DateTime?) || pi.PropertyType == typeof(DateTime))
                        {
                            pi.SetValue(entity, DateTime.Parse(value.ToString()), null);
                        }
                        else if (pi.PropertyType == typeof(float))
                        {
                            pi.SetValue(entity, float.Parse(value.ToString()), null);
                        }
                        else if (pi.PropertyType == typeof(double))
                        {
                            pi.SetValue(entity, double.Parse(value.ToString()), null);
                        }
                        else
                        {
                            pi.SetValue(entity, value, null);
                        }
                    }
                }
                lst.Add(entity);
            }
            return lst;
        }

        public static List<T> TableToListModel<T>(DataTable dt) where T : new()
        {
            // 定义集合    
            List<T> ts = new List<T>();

            // 获得此模型的类型   
            Type type = typeof(T);
            string tempName = "";

            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;  // 检查DataTable是否包含此列    

                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter      
                        if (!pi.CanWrite) continue;

                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }




        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }

    public class Student
    {
        public string Name { get; set; }
        public int? Age { get; set; }
        public bool Marry { get; set; }
        public bool Good { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime? Birthdate { get; set; }
        public double Score { get; set; }
    }
}
