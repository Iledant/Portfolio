using Windows.UI.Xaml.Controls;
using System;
using Portfolio.Models;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de gestion des onglets de la gestion d'un portefeuille
    /// </summary>
    public sealed partial class PortFolioMainPage : Page
    {
        private PortFolio _portfolio;

        public PortFolioMainPage()
        {
            InitializeComponent();
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

        private void NavigationView_SelectionChanged(NavigationView _1, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem selected = args.SelectedItem as NavigationViewItem;
            _ = selected.Tag switch
            {
                "Funds" => contentFrame.Navigate(typeof(PortFolioPage), _portfolio),
                "Performance" => contentFrame.Navigate(typeof(PortFolioPerformancePage), _portfolio),
                "Historical" => contentFrame.Navigate(typeof(PortFolioHistoricalPage), _portfolio),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
