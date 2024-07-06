using System.Windows;

namespace FilterDataGridDome
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            FilterDataGrid.Properties.Resources.Culture = new System.Globalization.CultureInfo("zh_CN");
        }
    }
}
