using Portfolio.Dialogs;
using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de gestion des fonds d'un portefeuille.
    /// </summary>
    public sealed partial class PortFolioFundsPage : Page
    {
        private PortFolio _portfolio;
        private string _search = "";
        private readonly PortFolioViewModel ViewModel;

        public PortFolioFundsPage()
        {
            InitializeComponent();
            ViewModel = new();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PortFolio)
            {
                _portfolio = e.Parameter as PortFolio;
                ViewModel.Fetch(_portfolio, "");
                ViewModel.FetchCash(_portfolio);
                ViewModel.FetchMonetaryAccountBalances(_portfolio);
            }
            base.OnNavigatedTo(e);
        }

        private void Search_TextChanged(object _, DebounceSearchEventArgs e)
        {
            _search = e.Text;
            ViewModel.Fetch(_portfolio, e.Text);
        }

        private void RemoveCommand_ExecuteRequested(XamlUICommand _, ExecuteRequestedEventArgs args)
        {
            if (args == null || args.Parameter == null)
            {
                return;
            }
            ShowDeleteDialogAndHandle(args.Parameter as PortFolioLine);
        }

        private void AddButton_Click(object _1, RoutedEventArgs _2)
        {
            ShowDialogAndUpdate(new PortFolioLineEditDialog(new PortFolioLine(portFolioID: _portfolio.ID)));
        }

        private void ModifyCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is not null and PortFolioLine)
            {
                PortFolioLineEditDialog dialog = new(args.Parameter as PortFolioLine);
                ShowDialogAndUpdate(dialog);
            }
        }

        private async void ShowDialogAndUpdate(PortFolioLineEditDialog dialog)
        {
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.Fetch(_portfolio, _search);
            }
        }

        private void ListView_DoubleTapped(object _1, DoubleTappedRoutedEventArgs _2)
        {
            if (ListView.SelectedItem is not null)
            {
                PortFolioLineEditDialog dialog = new(ListView.SelectedItem as PortFolioLine);
                ShowDialogAndUpdate(dialog);
            }
        }

        private void DeleteKeyInvoked(KeyboardAccelerator _1, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (ListView.SelectedItem is not null)
            {
                ShowDeleteDialogAndHandle(ListView.SelectedItem as PortFolioLine);
            }
            args.Handled = true;
        }

        private async void ShowDeleteDialogAndHandle(PortFolioLine line)
        {
            DeleteDialog deleteDialog = new("la ligne");

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.Delete(_portfolio, line, _search);
            }
        }

        private async void CashAccountHistoryButton_Click(object _1, RoutedEventArgs _2)
        {
            CashAccountHistory historyDialog = new(_portfolio.ID);

            _ = await historyDialog.ShowAsync();
        }

        private async void MonetayAccountEditButton_Click(object _1, RoutedEventArgs _2)
        {
            MonetaryAccount account = new(id: 0, name: "", portfolioID: _portfolio.ID);
            MonetaryAccountEditDialog dialog = new(account);

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.FetchMonetaryAccountBalances(_portfolio);
            }
        }

        private async void RemoveMonetaryCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args == null || args.Parameter == null)
            {
                return;
            }

            DeleteDialog deleteDialog = new("le fond");

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                MonetaryAccountBalance accountBalance = args.Parameter as MonetaryAccountBalance;
                ViewModel.DeleteMonetaryAccount(accountBalance.ToMonetaryAccount(_portfolio), _portfolio);
            }
        }

        private async void ModifyMonetaryCommand_ExecuteRequested(XamlUICommand _1, ExecuteRequestedEventArgs args)
        {
            if (args == null || args.Parameter == null)
            {
                return;
            }

            MonetaryAccountBalance accountBalance = args.Parameter as MonetaryAccountBalance;
            MonetaryAccountEditDialog dialog = new(accountBalance.ToMonetaryAccount(_portfolio));

            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.FetchMonetaryAccountBalances(_portfolio);
            }
        }
    }
}
