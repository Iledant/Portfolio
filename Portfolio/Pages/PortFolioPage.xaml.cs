using Portfolio.Dialogs;
using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de gestion des fonds d'un portefeuille.
    /// </summary>
    public sealed partial class PortFolioPage : Page
    {
        private PortFolio _portfolio;
        private string _search = "";
        private readonly PortFolioViewModel ViewModel;

        public PortFolioPage()
        {
            InitializeComponent();
            ViewModel = new();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PortFolio and not null)
            {
                _portfolio = e.Parameter as PortFolio;
                Title.Text = $"Portefeuille : {_portfolio.Name}";
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

        private void AddButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Fetch(_portfolio, "");
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (ListView.SelectedItem is not null)
            {
                PortFolioLineEditDialog dialog = new(ListView.SelectedItem as PortFolioLine);
                ShowDialogAndUpdate(dialog);
            }
        }

        private void DeleteKeyInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
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
    }
}
