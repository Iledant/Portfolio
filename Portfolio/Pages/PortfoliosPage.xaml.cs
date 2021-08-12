using Portfolio.Dialogs;
using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de gestion des portefeuilles.
    /// </summary>
    public sealed partial class PortfoliosPage : Page
    {
        private readonly PortFoliosViewModel ViewModel;
        private string _search = "";

        public PortfoliosPage()
        {
            InitializeComponent();
            ViewModel = new();
        }

        private void Page_Loaded(object _1, Windows.UI.Xaml.RoutedEventArgs _2)
        {
            ViewModel.Fetch("");
        }

        private void SearchBox_DataContextChanged(object sender, DebounceSearchEventArgs e)
        {
            _search = e.Text;
            ViewModel.Fetch(_search);
        }

        private async void DeleteCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args == null)
            {
                return;
            }
            DeleteDialog deleteDialog = new("le portefeuille");

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                PortFolio portfolio= args.Parameter as PortFolio;
                ViewModel.Delete(portfolio, _search);
            }
        }

        private void EditCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args == null)
            {
                return;
            }

            ShowEditDialogAndHandle(args.Parameter as PortFolio);
        }

        private void AddButton_Click(object _1, Windows.UI.Xaml.RoutedEventArgs _2)
        {
            AddPortFolio();
        }

        private void AddPortFolio()
        {
            ShowEditDialogAndHandle(new PortFolio());
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (PortFoliosList.SelectedItem is not PortFolio)
            {
                return;
            }
            _ = Frame.Navigate(typeof(PortFolioMainPage), PortFoliosList.SelectedItem);
        }

        private void DetailCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is not null and PortFolio)
            {
                _ = Frame.Navigate(typeof(PortFolioMainPage), args.Parameter);
            }
        }

        private async void ShowEditDialogAndHandle(PortFolio portfolio)
        {
            PortfolioEditDialog dialog = new(portfolio);
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
