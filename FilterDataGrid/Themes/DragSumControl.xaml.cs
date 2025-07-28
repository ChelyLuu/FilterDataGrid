using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FilterDataGrid.Themes
{
    /// <summary>
    /// DragSumControl.xaml 的交互逻辑
    /// </summary>
    public partial class DragSumControl : UserControl
    {
        public DragSumControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(DragSumControl), new PropertyMetadata(Properties.Resources.Count+"\r\n"+Properties.Resources.Avg+"\r\n"+Properties.Resources.Sum));


        //鼠标是否按下
        bool _isMouseDown = false;
        //鼠标按下的位置
        Point _mouseDownPosition;
        //鼠标按下控件的位置
        Point _mouseDownControlPosition;
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var c = sender as Border;
            
            _isMouseDown = true;
            _mouseDownPosition = e.GetPosition(this);
            var transform = c.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                c.RenderTransform = transform;
            }
            _mouseDownControlPosition = new Point(transform.X, transform.Y);
            c.CaptureMouse();
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                var c = sender as Border;
                var pos = e.GetPosition(this);
                var dp = pos - _mouseDownPosition;
                var transform = c.RenderTransform as TranslateTransform;
                transform.X = _mouseDownControlPosition.X + dp.X;
                transform.Y = _mouseDownControlPosition.Y + dp.Y;
            }
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var c = sender as Border;
            _isMouseDown = false;
            c.ReleaseMouseCapture();
        }

    }
}
