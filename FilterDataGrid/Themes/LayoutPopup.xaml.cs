using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;

namespace FilterDataGrid.Themes
{
    /// <summary>
    /// LayoutPopup.xaml 的交互逻辑
    /// </summary>
    public partial class LayoutPopup : Window
    {
        DataGrid dg;
        public List<string> customGroups;
        public List<GroupSortItem> groupSortItems;
        public LayoutPopup(DataGrid _dg)
        {
            InitializeComponent();
            dg = _dg;
            Loaded += LayoutPopup_Loaded;
        }

        private void LayoutPopup_Loaded(object sender, RoutedEventArgs e)
        {
            if (dg.GridLinesVisibility == DataGridGridLinesVisibility.All)
                Ckb_ShowVerticalGridLine.IsChecked = true;
        }

        private void Border_Dray_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Btn_CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            var story = (Storyboard)this.Resources["HideWindow"];
            if (story != null)
            {
                story.Completed += delegate
                {
                    Cob_ColumnName.SelectedItem = null;
                    this.Close();
                };
                story.Begin(this);
            }

        }

        private void Cob_ColumnName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Cob_ColumnName.SelectedItem == null) return;

            Tbn_ShowHideColumn.IsEnabled = true;
            Sld_ColumnWidth.IsEnabled = true;

            ColumnProperty cp = Cob_ColumnName.SelectedItem as ColumnProperty;
            if (cp == null) return;
            Sld_ColumnWidth.Value = Math.Round(cp.Columnwidth, 0);

            if (cp.ColumnType == typeof(DataGridTextColumn))
                Cob_Alignment.IsEnabled = true;
            else Cob_Alignment.IsEnabled = false;

            Cob_Alignment.SelectedItem = null;
        }

        private void Sld_ColumnWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ColumnProperty cp = Cob_ColumnName.SelectedItem as ColumnProperty;
            if (dg.Columns[cp.Index].GetType() == typeof(DataGridTextColumn))
            {
                DataGridTextColumn dtc = dg.Columns[cp.Index] as DataGridTextColumn;
                if (dtc == null) return;
                dtc.Width = Sld_ColumnWidth.Value;
            }
            else if (dg.Columns[cp.Index].GetType() == typeof(DataGridCheckBoxColumn))
            {
                DataGridCheckBoxColumn dtc = dg.Columns[cp.Index] as DataGridCheckBoxColumn;
                if (dtc == null) return;
                dtc.Width = Sld_ColumnWidth.Value;
            }
            else
            {
                DataGridTemplateColumn dtc = dg.Columns[cp.Index] as DataGridTemplateColumn;
                if (dtc == null) return;
                dtc.Width = Sld_ColumnWidth.Value;
            }

            cp.Columnwidth = Sld_ColumnWidth.Value;
        }

        private void Tbn_ShowHideColumn_Click(object sender, RoutedEventArgs e)
        {
            ColumnProperty cp = Cob_ColumnName.SelectedItem as ColumnProperty;
            if (Tbn_ShowHideColumn.IsChecked == false)
            {
                dg.Columns[cp.Index].Visibility = Visibility.Collapsed;
                cp.Visibility = Visibility.Collapsed;
            }
            else
            {
                dg.Columns[cp.Index].Visibility = Visibility.Visible;
                cp.Visibility = Visibility.Visible;
            }
        }

        private void Cob_Alignment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Cob_Alignment.SelectedItem == null) return;

            ColumnProperty cp = Cob_ColumnName.SelectedItem as ColumnProperty;
            DataGridTextColumn dtc = dg.Columns[cp.Index] as DataGridTextColumn;
            ComboBoxItem cobitem = Cob_Alignment.SelectedItem as ComboBoxItem;

            if (cobitem.Tag.ToString() == "Left")
            {
                dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Left") as Style;
                cp.Alignment = "Left";
            }
            else if (cobitem.Tag.ToString() == "Center")
            {
                dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Center") as Style;
                cp.Alignment = "Center";
            }
            else if (cobitem.Tag.ToString() == "Right")
            {
                dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Right") as Style;
                cp.Alignment = "Right";
            }
            else
            {
                dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Default") as Style;
                cp.Alignment = "Default";
            }
        }

        private void Ckb_ShowCenter_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataGridColumn dataGridColumn in dg.Columns)
            {
                if (dataGridColumn.GetType() == typeof(DataGridTextColumn))
                {
                    DataGridTextColumn dtc = dataGridColumn as DataGridTextColumn;
                    if (Ckb_ShowCenter.IsChecked == true)
                    {
                        dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Center") as Style;
                    }
                    else
                    {
                        dtc.CellStyle = Application.Current.TryFindResource("HighLightCellStyle_Default") as Style;
                    }

                }
            }
        }

        private void Ckb_TwoColor_Click(object sender, RoutedEventArgs e)
        {
            if (Ckb_TwoColor.IsChecked == true)
            {
                dg.RowStyle = Application.Current.TryFindResource("DataGridRowStyle_ChangeColor") as Style;
            }
            else
            {
                dg.RowStyle = Application.Current.TryFindResource("DataGridRowStyle_Default") as Style;
            }

        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Layout/"))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Layout/");
                directoryInfo.Create();
            }

            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Layout/" + Txb_FileName.Text;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            XmlDocument tDoc = new XmlDocument();

            // 一些声明信息
            XmlDeclaration xmlDecl = tDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            tDoc.AppendChild(xmlDecl);

            // 新建根节点
            XmlElement root = tDoc.CreateElement("DataGrid");
            root.SetAttribute("FileName", Txb_FileName.Text);
            tDoc.AppendChild(root);

            // root 下的子节点
            XmlElement headerHeight = tDoc.CreateElement("ColumnHeaderHeight");
            headerHeight.InnerText = Sdr_HeaderHeight.Value.ToString();
            root.AppendChild(headerHeight);

            XmlElement rowHeight = tDoc.CreateElement("RowHeight");
            rowHeight.InnerText = Sdr_RowHeight.Value.ToString();
            root.AppendChild(rowHeight);

            XmlElement rowHeaderWidth = tDoc.CreateElement("RowHeaderWidth");
            rowHeaderWidth.InnerText = Sdr_RowHeaderWidth.Value.ToString();
            root.AppendChild(rowHeaderWidth);

            XmlElement groupRowHeight = tDoc.CreateElement("GroupRowHeight");
            groupRowHeight.InnerText = Sdr_GroupRowHeight.Value.ToString();
            root.AppendChild(groupRowHeight);

            XmlElement showCenter = tDoc.CreateElement("ShowInCenter");
            showCenter.InnerText = Ckb_ShowCenter.IsChecked.ToString();
            root.AppendChild(showCenter);

            XmlElement showGlobalSearch = tDoc.CreateElement("ShowGlobalSearchPanel");
            showGlobalSearch.InnerText = Ckb_ShowGlobalSearch.IsChecked.ToString();
            root.AppendChild(showGlobalSearch);

            XmlElement changeColor = tDoc.CreateElement("ChangeColor");
            changeColor.InnerText = Ckb_TwoColor.IsChecked.ToString();
            root.AppendChild(changeColor);

            XmlElement verticalGridLine = tDoc.CreateElement("ShowVerticalGridLine");
            verticalGridLine.InnerText = Ckb_ShowVerticalGridLine.IsChecked.ToString();
            root.AppendChild(verticalGridLine);

            XmlElement columns = tDoc.CreateElement("Columns");
            root.AppendChild(columns);

            List<ColumnProperty> columnPropertys = Cob_ColumnName.ItemsSource as List<ColumnProperty>;
            foreach (var item in columnPropertys)
            {
                XmlElement column = tDoc.CreateElement(item.Name.ToString());
                columns.AppendChild(column);

                XmlElement columnHeader = tDoc.CreateElement("Header");
                columnHeader.InnerText = item.Name.ToString();
                column.AppendChild(columnHeader);

                XmlElement columnWidth = tDoc.CreateElement("Width");
                columnWidth.InnerText = item.Columnwidth.ToString();
                column.AppendChild(columnWidth);

                XmlElement columnAlign = tDoc.CreateElement("Alignment");
                columnAlign.InnerText = item.Alignment;
                column.AppendChild(columnAlign);

                XmlElement columnVisibly = tDoc.CreateElement("Visibility");
                columnVisibly.InnerText = item.Visibility.ToString();
                column.AppendChild(columnVisibly);

                //XmlElement columnSort = tDoc.CreateElement("Sort");
                //columnSort.InnerText = item.ColumnSort.ToString();
                //column.AppendChild(columnSort);
            }

            XmlElement groupColumns = tDoc.CreateElement("Group");
            root.AppendChild(groupColumns);
            foreach (var item in customGroups)
            {
                XmlElement columnHeader = tDoc.CreateElement("Header");
                columnHeader.InnerText = item;
                groupColumns.AppendChild(columnHeader);
            }

            XmlElement sortColumns = tDoc.CreateElement("Sort");
            root.AppendChild(sortColumns);
            foreach (var item in groupSortItems)
            {
                XmlElement columnHeader = tDoc.CreateElement(item.FieldName);
                columnHeader.InnerText = item.SortDirection.ToString();
                sortColumns.AppendChild(columnHeader);
            }



            tDoc.Save(filePath);

            Txb_TipContent.Text = Properties.Resources.SaveCompleted;
            Animation();
        }

        private void Btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Layout/" + Txb_FileName.Text;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            Txb_TipContent.Text = Properties.Resources.ResetCompleted;
            Animation();
        }

        private void Ckb_ShowGlobalSearch_Click(object sender, RoutedEventArgs e)
        {
            FilterDataGrid filterDataGrid = this.DataContext as FilterDataGrid;
            if (Ckb_ShowGlobalSearch.IsChecked == true)
            {

                filterDataGrid.SearchPanelVisibility = Visibility.Visible;
            }
            else
            {
                filterDataGrid.SearchPanelVisibility = Visibility.Collapsed;
            }
        }

        private void Ckb_ShowVerticalGridLine_Click(object sender, RoutedEventArgs e)
        {
            if (Ckb_ShowVerticalGridLine.IsChecked == true)
            {

                dg.GridLinesVisibility = DataGridGridLinesVisibility.All;
            }
            else
            {
                dg.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
            }
        }

        private void Btn_CloseMask_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TransformGroup transformGroup = new TransformGroup();
                ScaleTransform scaleTransform = new ScaleTransform();
                transformGroup.Children.Add(scaleTransform);
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.30),
                    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseIn },
                    FillBehavior = FillBehavior.Stop
                };
                animation.Completed += Animation_Completed;
                Bod_Mask.RenderTransform = transformGroup;
                Bod_Mask.RenderTransformOrigin = new Point(0.5, 0.5);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            });
        }

        private void Animation()
        {
            Dispatcher.Invoke(() =>
            {
                Bod_Mask.Visibility = Visibility.Visible;
                TransformGroup transformGroup = new TransformGroup();
                ScaleTransform scaleTransform = new ScaleTransform();
                transformGroup.Children.Add(scaleTransform);
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.30),
                    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut },
                    FillBehavior = FillBehavior.Stop
                };
                Bod_Mask.RenderTransform = transformGroup;
                Bod_Mask.RenderTransformOrigin = new Point(0.5, 0.5);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            });
        }
        private void Animation_Completed(object sender, EventArgs e)
        {
            Bod_Mask.Visibility = Visibility.Collapsed;
        }

    }
}
