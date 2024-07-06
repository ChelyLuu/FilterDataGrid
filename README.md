基于Material Design In XAML材质库创建的DataGrid列头筛选及搜索、统计功能。
代码并非最完美的，仅供大家参考学习。

使用方法：App.xml引用资源

  
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <materialDesign:BundledTheme
                BaseTheme="Dark"
                PrimaryColor="Red"
                SecondaryColor="Lime" />
            <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml" />
            <ResourceDictionary Source="pack://application:,,,/FilterDataGrid;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
   
   
 <Window . . .
       xmlns:filterDG="clr-namespace:FilterDataGrid;assembly=FilterDataGrid"
    <Grid>
 
    </Grid>
</Window>

1、显示行号
![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/c1194c2d-0435-4d5f-bd9f-940e0eff353f)


