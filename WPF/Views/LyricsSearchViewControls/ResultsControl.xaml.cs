using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WPF.Views.LyricsSearchViewControls
{
    public partial class ResultsControl : UserControl
    {
        public Expander ParentExpander
        {
            get { return (Expander)GetValue(ParentExpanderProperty); }
            set { SetValue(ParentExpanderProperty, value); }
        }

        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty ParentExpanderProperty =
            DependencyProperty.Register("ParentExpander", typeof(Expander), typeof(ResultsControl), new PropertyMetadata());

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ResultsControl), new PropertyMetadata());

        public ResultsControl()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0)
            {
                ParentExpander.IsExpanded = true;
            }
        }

        private void DataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var dg = sender as DataGrid;
            dg.Columns.Clear();

            if (dg.ItemsSource is ObservableCollection<Dictionary<string, string>> dicts && dicts.Any())
            {
                foreach (var prop in dicts.First())
                {
                    dg.Columns.Add(new DataGridTextColumn()
                    {
                        Header = AddSpacesToSentence(prop.Key),
                        Binding = new Binding($"[{prop.Key}]"),
                        IsReadOnly = true,
                        Width = DataGridLength.Auto
                    });
                }
            }
            ParentExpander.IsExpanded = true;
        }

        private string AddSpacesToSentence(string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //workaround for scroll over datagrid
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - e.Delta);
        }
    }
}