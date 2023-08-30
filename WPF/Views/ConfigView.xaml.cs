using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class ConfigView : Window
    {
        private bool @bool;

        public ConfigView(ConfigViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Closed += ConfigView_Closed;
            viewModel.OnWindowCloseRequest += (s, e) => { Close(); };

            TabControl.SelectionChanged += TabControl_SelectionChanged;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedIndex == TabControl.Items.Count - 1)
                KeyDown += TabControl_KeyDown;
            else
                KeyDown -= TabControl_KeyDown;
        }

        private void TabControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (@bool && e.Key == (Key)43)
            {
                (TryFindResource("sb") as Storyboard)?.Begin();
                @bool = false;
            }
            else if (e.Key == (Key)37) @bool = true;
            else @bool = false;
        }

        private void ConfigView_Closed(object sender, EventArgs e)
        {
            (DataContext as ConfigViewModel).CancelCommand.Execute(null);
        }
    }
}