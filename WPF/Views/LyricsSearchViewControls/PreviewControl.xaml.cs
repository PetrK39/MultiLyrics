using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WPF.Views.LyricsSearchViewControls
{
    public partial class PreviewControl
    {
        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(PreviewControl), new PropertyMetadata());

        public Expander ParentExpander
        {
            get { return (Expander)GetValue(ParentExpanderProperty); }
            set { SetValue(ParentExpanderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for expander.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParentExpanderProperty =
            DependencyProperty.Register("ParentExpander", typeof(Expander), typeof(PreviewControl), new PropertyMetadata());

        public PreviewControl()
        {
            InitializeComponent();
        }

        private void TextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is TextBlock tb && !string.IsNullOrWhiteSpace(tb.Text))
            {
                ParentExpander.IsExpanded = true;
                ScrollViewer.ScrollToBottom();
            }
            else
                ParentExpander.IsExpanded = false;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var sw = (ScrollViewer)sender;

            //if (sw.VerticalOffset == 0 && e.Delta > 0) ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
            //if (sw.VerticalOffset == sw.ScrollableHeight && e.Delta < 0) ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);

            if (e.Delta < 0)
                if (ScrollViewer.VerticalOffset < ScrollViewer.ScrollableHeight)
                    ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
                else
                    sw.ScrollToVerticalOffset(sw.VerticalOffset - e.Delta);

            if (e.Delta > 0)
                if (sw.VerticalOffset > 0)
                    sw.ScrollToVerticalOffset(sw.VerticalOffset - e.Delta);
                else
                    ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
        }
    }
}