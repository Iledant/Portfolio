using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page d'affichage de l'historique du portefeuille.
    /// </summary>
    public sealed partial class PortFolioHistoricalPage : Page
    {
        private PortFolio _portfolio;
        private readonly PortfolioHistoricalViewModel ViewModel;

        public PortFolioHistoricalPage()
        {
            InitializeComponent();
            ViewModel = new();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is null or not PortFolio)
            {
                throw new ArgumentException();
            }
            _portfolio = e.Parameter as PortFolio;

            ViewModel.FetchValues(_portfolio);
            base.OnNavigatedTo(e);
        }
    }
}
