# FilterDataGrid
项目介绍：FiterDataGrid是基于MaterialDesignInXaml第三方控件库开发的表格控件。控件包含了列头过滤、全局搜索、自定义过滤、多重排序、统计行、分页显示、拖动鼠标统计、布局调整及保存等多功能。
视频介绍：https://t.bilibili.com/951414893368573957?share_source=pc_native
项目引用：
<Application
    ......
    xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes"
    ......>
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <materialDesign:BundledTheme
                    BaseTheme="Light"
                    PrimaryColor="Red"
                    SecondaryColor="Lime" />
                <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml" />
                <ResourceDictionary Source="pack://application:,,,/FilterDataGrid;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>

特别注意：MaterialDesignInXaml第三方控件库请使用4.9.0版本及以下。

功能介绍：
1、列头过滤，可显示多列过滤结果
<img width="873" height="614" alt="image" src="https://github.com/user-attachments/assets/9e1a924f-685e-4823-97b5-967a198d4d2c" />

2、全局搜索，支持关键词高亮显示
<img width="638" height="637" alt="image" src="https://github.com/user-attachments/assets/13fb643f-3a1e-4e57-b0d1-34f8b315ae98" />

3、分页显示功能
<img width="797" height="576" alt="image" src="https://github.com/user-attachments/assets/fc6eddc1-18ec-40c1-bd08-b3e693c6b06f" />

4、列统计功能，支持根据筛选结果实时更新统计结果
<img width="850" height="507" alt="image" src="https://github.com/user-attachments/assets/54303233-81ab-4301-9eda-136621afff4b" />

5、鼠标拖动统计
<img width="850" height="556" alt="image" src="https://github.com/user-attachments/assets/30b10b01-f026-40d4-93b8-27e29f47e061" />

6、界面的调整，调整后可以保存布局，下次加载调整后样式
<img width="300" height="430" alt="image" src="https://github.com/user-attachments/assets/ef1f7b8a-5ab3-4257-9c6c-cad5b10819d1" />

7、列头菜单功能
<img width="305" height="576" alt="image" src="https://github.com/user-attachments/assets/d4fe60b0-afb8-44b8-a281-b0804f752cba" />

8、单元格未显示完，tooltip提示功能
<img width="212" height="349" alt="image" src="https://github.com/user-attachments/assets/bf4ca278-e77b-4114-a10a-9eacf73f96f8" />

9、自定义分组功能
<img width="850" height="620" alt="image" src="https://github.com/user-attachments/assets/68d2dca0-f943-4dc3-a1fd-fedbe4bf81d2" />

10、多言语设计
<img width="830" height="299" alt="image" src="https://github.com/user-attachments/assets/4bf5c4a2-be3b-48e1-adfb-534164d2b432" />











