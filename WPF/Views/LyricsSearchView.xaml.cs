using System;
using System.Windows;
using WPF.ViewModels;

namespace WPF.Views
{
    public partial class LyricsSearchView : Window
    {
        public LyricsSearchView(LyricsSearchViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;

            Closed += LyricsSearchView_Closed;
            viewModel.OnWindowCloseRequest += (s, e) => { Close(); };
        }

        private void LyricsSearchView_Closed(object sender, EventArgs e)
        {
            ((LyricsSearchViewModel)DataContext).CancelCommand.Execute(null);
        }
    }
}