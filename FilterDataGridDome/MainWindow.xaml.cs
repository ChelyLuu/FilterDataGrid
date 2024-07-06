using FilterDataGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Dynamic;

namespace FilterDataGridDome
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


        public ObservableCollection<Employe> Employes { get; set; }

        public ObservableCollection<Employe> DisplayEmployes { get; set; }

        public ObservableCollection<Employe> Employes2 { get; set; }
        public ObservableCollection<Employe> DisplayEmployes2 { get; set; }
        public IEnumerable<Employe> PageEmployes { get; set; }

        public List<FooterItem> FooterList { get; set; } = new List<FooterItem>()
        {
            new FooterItem()
            {
                FieldName = "Age", TotalType = TotalType.Ave, FormatString = "平均:0.00"
            } ,
            new FooterItem()
            {
                FieldName = "Salary", TotalType = TotalType.Sum, FormatString = "总分数:0"
            }  ,
            new FooterItem()
            {
                FieldName = "LastName", TotalType = TotalType.Count, FormatString = "计数:0"
            }
        };
        public List<string> DefaultGroup { get; set; } = new List<string>() { "StartDate" };

        private async void FillData()
        {

            int count = 5000;
            var employe = new List<Employe>(count);

            // for distinct lastname set "true" at CreateRandomEmployee(true)
            await Task.Run(() =>
            {

                for (var i = 0; i < count; i++)
                    Employes.Add(RandomGenerator.CreateRandomEmployee(true));
            });
            DisplayEmployes = new ObservableCollection<Employe>(Employes);
            PageEmployes = Employes2 = new ObservableCollection<Employe>(Employes);
            DisplayEmployes2 = new ObservableCollection<Employe>(DataGrid_CustomizeColumns.PageInitialized(Employes2));
            OnPropertyChanged("DisplayEmployes");
            OnPropertyChanged("DisplayEmployes2");


        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Employes = new ObservableCollection<Employe>();
            FillData();
        }

        private void DataGrid_MainWindow_PageChanged(object sender, RoutedEventArgs e)
        {

            DisplayEmployes2 = new ObservableCollection<Employe>(DataGrid_CustomizeColumns.PageSwitched(PageEmployes));
            OnPropertyChanged("DisplayEmployes2");
        }

        private void DataGrid_CustomizeColumns_GlobalSearch(object sender, RoutedEventArgs e)
        {
            FilterDataGrid.FilterDataGrid filterDataGrid = e.OriginalSource as FilterDataGrid.FilterDataGrid;
            string hiText = filterDataGrid.HiText;

            if (string.IsNullOrEmpty(hiText)) return;

            PageEmployes = Employes2.Where(x => x.FirstName.Contains(hiText) || x.LastName.Contains(hiText) ||
           x.Salary.ToString().Contains(hiText) || x.Age.ToString().Contains(hiText) || x.StartDate.ToString().Contains(hiText));
            //PageEmployes = filterDataGrid.PageFilter<Employe>(Employes2);


            DisplayEmployes2 = new ObservableCollection<Employe>(DataGrid_CustomizeColumns.PageInitialized(PageEmployes));

            OnPropertyChanged("DisplayEmployes2");

        }

        private void DataGrid_CustomizeColumns_CancelGlobalSearch(object sender, RoutedEventArgs e)
        {

            PageEmployes = DisplayEmployes2 = new ObservableCollection<Employe>(DataGrid_CustomizeColumns.PageInitialized(Employes2));

            OnPropertyChanged("DisplayEmployes2");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //Fdg_AutoGenerateColumns.ExportToExcel("AutoGenerateColumns");
            //Console.WriteLine(DataGrid_AutoGenerateColumn.CurrentCell.Item.ToString());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DataGrid_CustomizeColumns.ExportToExcel("CustomizeColumns");
        }

        private void Fdg_AutoGenerateColumns_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            //foreach (DataGridCellInfo item in Fdg_AutoGenerateColumns.SelectedCells)
            //{
            //    Employe employe = (Employe)item.Item;
            //    Console.WriteLine(employe.Age);
            //}   
         //ObservableCollection<Employe> employes= new ObservableCollection<Employe>(  Fdg_AutoGenerateColumns.SelectedItems);
            //DataGridCellInfo[] items = (DataGridCellInfo[])(Fdg_AutoGenerateColumns.SelectedCells.ToArray());
            //foreach (var item in items)
            //{

            //    TextBlock str = ((TextBlock)item.Column.GetCellContent(item.Item));
            //    Console.WriteLine(str.GetValue(str));
            //}
            //items.Sum(x =>x.Column);
            //List<Employe> employes=Fdg_AutoGenerateColumns.SelectedCells as List<Employe>;
            //Console.WriteLine(employes.Count);
        }
    }

    public class Employe
    {
        #region Public Constructors

        public Employe(string lastName, string firstName, double? salary, int? age, DateTime? startDate,
            bool? manager = false)
        {
            LastName = lastName;
            FirstName = firstName;
            Salary = salary;
            Age = age;
            StartDate = startDate;
            Manager = manager;
        }

        #endregion Public Constructors

        #region Public Properties


        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? Manager { get; set; }
        public double? Salary { get; set; }
        public int? Age { get; set; }
        public DateTime? StartDate { get; set; }

        #endregion Public Properties
    }


}
