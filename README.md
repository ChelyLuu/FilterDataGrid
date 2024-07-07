基于Material Design In XAML材质库创建的DataGrid列头筛选及搜索、统计、分页、分组等功能。
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

2、列头过滤

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/0f889d21-f8e2-43b4-92d1-ef369aa049b7)

3、全局搜索，高亮显示

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/f27b6b6b-b53e-4152-9247-135ab22dacbf)

4、排序功能，可多次排序

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/b51c3b5a-339a-4443-a7ab-3cb7473bb5f7)

5、行统计功能

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/5c4a0fad-4f00-4b6c-b1d1-c661344a873d)

6、分组功能

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/1beb316b-dc05-4715-9458-11997990ce87)

7、分页功能

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/f22bf873-fdd3-4732-81da-e3943908007a)

8、鼠标拖动统计功能

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/8ba206c3-11ff-4309-9df8-56fc71f188d1)

9、自定义过滤功能

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/f0a0ece7-d61e-4990-9212-b7a73e26f9d6)

10、界面调整功能

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/10a8743d-a931-4921-850a-cd34504c3697)

11、右键菜单

![image](https://github.com/ChelyLuu/FilterDataGrid/assets/73624088/affacfc2-bcb5-48ed-a14b-b0f6726cc244)

12、支持多语言
