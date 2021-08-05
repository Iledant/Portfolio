using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace Portfolio.Pages
{
    /// <summary>
    /// La page de gestion des fonds.
    /// </summary>
    public sealed partial class FundsPage : Page
    {
        private readonly FundsViewModel ViewModel;
        private string _search;

        public FundsPage()
        {
            InitializeComponent();
            ViewModel = new();
        }

        private void Page_Loaded(object _1, RoutedEventArgs _2)
        {
            ViewModel.Fetch("");
        }

        private async void DeleteCommand_ExecuteRequested(XamlUICommand _, ExecuteRequestedEventArgs args)
        {
            if (args == null)
            {
                return;
            }
            DeleteDialog deleteDialog = new("le fond");

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Fund fund = args.Parameter as Fund;
                ViewModel.Delete(fund, _search);
            }
        }

        private void EditCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args == null)
            {
                return;
            }

            Fund fund = args.Parameter as Fund;
            _ = Frame.Navigate(typeof(FundEditPage), fund);
        }


        private void SearchBox_DataContextChanged(object _, DebounceSearchEventArgs e)
        {
            _search = e.Text;
            ViewModel.Fetch(_search);
        }

        private void AddCommand_ExecuteRequested(XamlUICommand _1, ExecuteRequestedEventArgs _2)
        {
            _ = Frame.Navigate(typeof(FundEditPage), new Fund());
        }

        private void AddButton_Click(object _1, RoutedEventArgs _2)
        {
            _ = Frame.Navigate(typeof(FundEditPage), new Fund());
        }

        private void GridView_DoubleTapped(object _1, DoubleTappedRoutedEventArgs _2)
        {
            if (FundsList.SelectedItem is not Fund)
            {
                return;
            }
            _ = Frame.Navigate(typeof(FundChartPage), FundsList.SelectedItem);

        }
    }
}
