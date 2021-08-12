using Portfolio.Models;
using Portfolio.ViewModel;
using Portfolio.Dialogs;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Portfolio.Repositories;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de gestion des fonds.
    /// </summary>
    public sealed partial class FundsPage : Page
    {
        private readonly FundsViewModel ViewModel;
        private string _search;

        public object FundEditContentDialog { get; private set; }

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

            ShowEditDialogAndHandle( args.Parameter as Fund);
        }


        private void SearchBox_DataContextChanged(object _, DebounceSearchEventArgs e)
        {
            _search = e.Text;
            ViewModel.Fetch(_search);
        }

        private void AddButton_Click(object _1, RoutedEventArgs _2)
        {
            ShowEditDialogAndHandle(new Fund());
        }

        private void GridView_DoubleTapped(object _1, DoubleTappedRoutedEventArgs _2)
        {
            if (FundsList.SelectedItem is not Fund)
            {
                return;
            }
            _ = Frame.Navigate(typeof(FundChartPage), FundsList.SelectedItem);

        }

        private async void ShowEditDialogAndHandle(Fund fund)
        {
            FundEditDialog dialog = new(fund);
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (DB.State == DBState.AlreadyExists)
                {

                }
                else
                {
                    ViewModel.Fetch(_search);
                }
            }
        }
    }
}
