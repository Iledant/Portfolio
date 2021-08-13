using Portfolio.Controls;
using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    public class HistoricalPeriod
    {
        public string DisplayText;
        public DateTime? Since;
    }

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

            base.OnNavigatedTo(e);
        }

        private void PeriodComboBox_PeriodChanged(object _1, PeriodChangedEventArgs e)
        {
            if (e is null || _portfolio is null)
            {
                return;
            }
            ViewModel.FetchValues(_portfolio, e.Since);
        }
    }
}
