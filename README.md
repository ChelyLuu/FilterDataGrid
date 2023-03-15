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

![image](https://user-images.githubusercontent.com/73624088/225265390-f9e90483-5a6e-402e-828a-fa8117407791.png)

支持列头右键

![image](https://user-images.githubusercontent.com/73624088/225265847-f24406f9-ef58-4990-88dc-c9d0ba52705e.png)

支持全局搜索（Ctrl+F）

![image](https://user-images.githubusercontent.com/73624088/225266292-50c16732-48f5-4962-8233-56a334071589.png)

