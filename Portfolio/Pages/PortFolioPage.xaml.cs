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
        private string _search;
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

        private async void RemoveCommand_ExecuteRequested(XamlUICommand _, ExecuteRequestedEventArgs args)
        {
            if (args == null || args.Parameter == null)
            {
                return;
            }

            DeleteDialog deleteDialog = new("la compagnie");

            ContentDialogResult result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                PortFolioLine line = args.Parameter as PortFolioLine;
                ViewModel.Delete(_portfolio, line, _search);
            }
        }

        private void AddCommand_ExecuteRequested(XamlUICommand _1, ExecuteRequestedEventArgs _2)
        {
            AddPortFolioLine();
        }

        private void AddPortFolioLine()
        {
            _ = Frame.Navigate(typeof(PortFolioLineEditPage), new PortFolioLine(portFolioID: _portfolio.ID));
        }

        private void AddButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AddPortFolioLine();
        }

        private void ModifyCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (args.Parameter is not null and PortFolioLine)
            {
                _ = Frame.Navigate(typeof(PortFolioLineEditPage), args.Parameter as PortFolioLine);
            }
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Fetch(_portfolio, "");
        }
    }
}
