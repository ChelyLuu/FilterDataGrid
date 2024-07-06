using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FilterDataGrid.Themes
{
    /// <summary>
    /// CustomizeSerach.xaml 的交互逻辑
    /// </summary>
    public partial class CustomizeSerach : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged实现接口
        /// <summary>
        /// OnPropertyChange
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public List<CustomizeFilterColumn> ColumnsList { get; set; }
        public List<OperatorString> OperatorStrings { get; set; }
        //public List<string> OperatorString { get; set; } = new List<string>()
        //{
        //    Properties.Resources.Equal,
        //    Properties.Resources.UnEqual,
        //    Properties.Resources.Contain,
        //    Properties.Resources.UnContain,
        //    Properties.Resources.Greater,
        //    Properties.Resources.GreaterEqual,
        //    Properties.Resources.Less,
        //    Properties.Resources.LessEqual,
        //    Properties.Resources.StartWith,
        //    Properties.Resources.EndWith,
        //    Properties.Resources.Null,
        //    Properties.Resources.UnNull
        //};
        public List<string> AndOr { get; set; } = new List<string>() { Properties.Resources.And, Properties.Resources.Or };

        private ObservableCollection<CustomizeFilter> _currentCustomizeFilters = new ObservableCollection<CustomizeFilter>();
        public ObservableCollection<CustomizeFilter> CurrentCustomizeFilters
        {
            get { return _currentCustomizeFilters; }
            set { _currentCustomizeFilters = value; OnPropertyChanged(nameof(CurrentCustomizeFilters)); }
        }

        public CustomizeSerach(List<CustomizeFilterColumn> lists)
        {
            InitializeComponent();
            this.DataContext = this;

            ColumnsList = lists;

            Loaded += CustomizeSerach_Loaded;
        }

        private void CustomizeSerach_Loaded(object sender, RoutedEventArgs e)
        {
            if (FilterDataGrid.CurrentCustomizeFilters.Count > 0)
            {
                CurrentCustomizeFilters = FilterDataGrid.CurrentCustomizeFilters;
            }
            else
            {
                CurrentCustomizeFilters.Add(new CustomizeFilter()
                {
                    EnableAO = false,
                    AndOr = null,
                });
            }
        }

        private void Border_Dray_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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
                    this.Close();
                };
                story.Begin(this);
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            CustomizeFilterColumn customizeFilterColumn = comboBox.SelectedItem as CustomizeFilterColumn;
            BindComboboxOperator(customizeFilterColumn);
        }

        private void BindComboboxOperator(CustomizeFilterColumn customizeFilterColumn)
        {
            if (customizeFilterColumn.ColumnType == typeof(string))
            {
                OperatorStrings = new List<OperatorString>()
                {
                   new OperatorString(){ IcoText="\xe600", OperatorText= Properties.Resources.Equal, ID="Equal"},
                   new OperatorString() { IcoText = "\xe60d", OperatorText = Properties.Resources.UnEqual, ID="UnEqual" },
                   new OperatorString() { IcoText = "\xe70a", OperatorText = Properties.Resources.Contain, ID="Contain" },
                   new OperatorString() { IcoText = "\xe70b", OperatorText = Properties.Resources.UnContain, ID="UnContain" },
                   new OperatorString() { IcoText = "\xe614", OperatorText = Properties.Resources.StartWith, ID="StartWith" },
                   new OperatorString() { IcoText = "\xe613", OperatorText = Properties.Resources.EndWith, ID="EndWith" },
                   new OperatorString() { IcoText = "\xe6f4", OperatorText = Properties.Resources.Null, ID="Null" },
                   new OperatorString() { IcoText = "\xe6fb", OperatorText = Properties.Resources.UnNull, ID="UnNull" }
                };
            }
            else if (customizeFilterColumn.ColumnType == typeof(bool))
            {
                OperatorStrings = new List<OperatorString>()
                {
                   new OperatorString(){ IcoText="\xe600" ,OperatorText= Properties.Resources.Equal, ID="Equal" },
                   new OperatorString() { IcoText = "\xe60d", OperatorText = Properties.Resources.UnEqual, ID="UnEqual" },
                   new OperatorString() { IcoText = "\xe6f4", OperatorText = Properties.Resources.Null, ID="Null" },
                   new OperatorString() { IcoText = "\xe6fb", OperatorText = Properties.Resources.UnNull, ID="UnNull" }
                };
            }
            else
            {
                OperatorStrings = new List<OperatorString>()
                {
                    new OperatorString(){ IcoText="\xe600" ,OperatorText= Properties.Resources.Equal, ID="Equal" },
                    new OperatorString() { IcoText = "\xe60d", OperatorText = Properties.Resources.UnEqual, ID="UnEqual" },
                    new OperatorString() { IcoText = "\xe6fe", OperatorText = Properties.Resources.Greater, ID="Greater" },
                    new OperatorString() { IcoText = "\xe6fd", OperatorText = Properties.Resources.GreaterEqual, ID="GreaterEqual" },
                    new OperatorString() { IcoText = "\xe70c", OperatorText = Properties.Resources.Less, ID="Less"},
                    new OperatorString() { IcoText = "\xe6ff", OperatorText = Properties.Resources.LessEqual, ID="LessEqual" },
                    new OperatorString() { IcoText = "\xe6f4", OperatorText = Properties.Resources.Null, ID="Null" },
                    new OperatorString() { IcoText = "\xe6fb", OperatorText = Properties.Resources.UnNull, ID="UnNull" }
                };
            }

            OnPropertyChanged(nameof(OperatorStrings));
        }
        private void Btn_AddRow_Click(object sender, RoutedEventArgs e)
        {
            CurrentCustomizeFilters.Add(new CustomizeFilter()
            {
                EnableAO = true,
            });
        }

        private void Btn_RemoveRow_Click(object sender, RoutedEventArgs e)
        {
            var listBoxItem = ((ListBoxItem)Lst_ListBox.ContainerFromElement((Button)sender)).Content;
            CustomizeFilter filter = listBoxItem as CustomizeFilter;
            CurrentCustomizeFilters.Remove(filter);
        }

        private void Btn_AddCondition_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCustomizeFilters.Count > 0)
            {
                CurrentCustomizeFilters.Add(new CustomizeFilter()
                {
                    EnableAO = true,
                });
            }
            else
            {
                CurrentCustomizeFilters.Add(new CustomizeFilter()
                {
                    EnableAO = false,
                    AndOr = null,
                });
            }

        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            CurrentCustomizeFilters = new ObservableCollection<CustomizeFilter>
            {
                new CustomizeFilter()
                {
                    EnableAO = false,
                    AndOr = null,
                }
            };
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {

            if (CurrentCustomizeFilters.Count == 1)
            {
                if (CurrentCustomizeFilters.FirstOrDefault().ColumnsList == null)
                {
                    CurrentCustomizeFilters = new ObservableCollection<CustomizeFilter>();
                }
                else
                {
                    if (string.IsNullOrEmpty(CurrentCustomizeFilters.FirstOrDefault().Operator))
                    {
                        Animation();
                        return;
                    }
                }

            }
            else
            {
                for (int i = 1; i < CurrentCustomizeFilters.Count; i++)
                {

                    if (string.IsNullOrEmpty(CurrentCustomizeFilters[i].AndOr) ||
                       CurrentCustomizeFilters.FirstOrDefault().ColumnsList == null ||
                       string.IsNullOrEmpty(CurrentCustomizeFilters[i].Operator))
                    {
                        Animation();
                        return;
                    }

                }
            }


            var story = (Storyboard)this.Resources["HideWindow"];
            if (story != null)
            {
                story.Completed += delegate
                {
                    this.DialogResult = true;
                };
                story.Begin(this);
            }
        }


        private void Lst_ListBoxPop_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            OperatorString operatorString = (OperatorString)lb.SelectedItem;
            TextBox tb = (TextBox)lb.Tag;
            tb.Text = operatorString.OperatorText;
            tb.Tag = operatorString.ID;
        }

        private void Tog_ToggleButton_Click(object sender, RoutedEventArgs e)
        {

            ToggleButton toggleButton = (ToggleButton)sender;

            ComboBox comboBox = (ComboBox)toggleButton.Tag;
            if (comboBox.SelectedItem == null)
            {
                OperatorStrings = null;
                OnPropertyChanged(nameof(OperatorStrings));
                return;
            }

            CustomizeFilterColumn customizeFilterColumn = comboBox.SelectedItem as CustomizeFilterColumn;
            BindComboboxOperator(customizeFilterColumn);

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

        private void Bod_Mask_MouseDown(object sender, MouseButtonEventArgs e)
        {

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
    }


}
