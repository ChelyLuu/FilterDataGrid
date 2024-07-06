基于Material Design In XAML材质库创建的DataGrid列头筛选及搜索、统计功能。
代码并非最完美的，仅供大家参考学习。

使用方法：App.xml引用资源

  
<Application.Resources>
   <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <materialDesign:BundledTheme
                    BaseTheme="Light"
                    PrimaryColor="Blue"
                    SecondaryColor="Lime" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml" />
                <!--  Other merged dictionaries here  -->
                <ResourceDictionary Source="pack://application:,,,/FilterDataGrid;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
            <!--  Other app resources here  -->
    </ResourceDictionary>
</Application.Resources>
   
   
 <Window . . .
       xmlns:FD="clr-namespace:FilterDataGrid;assembly=FilterDataGrid"
    <Grid>
 
    </Grid>
</Window>

