using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using System.Collections.Generic;
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
        private readonly List<HistoricalPeriod> _periods = new();
        
        public PortFolioHistoricalPage()
        {
            InitializeComponent();
            ViewModel = new();
            FillPeriodComboBox();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is null or not PortFolio)
            {
                throw new ArgumentException();
            }
            _portfolio = e.Parameter as PortFolio;

            PeriodComboBox.SelectedItem = _periods[0];
            base.OnNavigatedTo(e);
        }

        private void PeriodComboBox_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            if (PeriodComboBox.SelectedItem is null)
            {
                return;
            }
            HistoricalPeriod item = PeriodComboBox.SelectedItem as HistoricalPeriod;
            ViewModel.FetchValues(_portfolio, item.Since);
        }

        private void FillPeriodComboBox()
        {
            _periods.Add(new HistoricalPeriod { DisplayText = "Tout l'historique", Since = null });
            _periods.Add(new HistoricalPeriod { DisplayText = "Un an", Since = DateTime.Now.AddYears(-1) });
            _periods.Add(new HistoricalPeriod { DisplayText = "6 mois", Since = DateTime.Now.AddMonths(-6) });
            _periods.Add(new HistoricalPeriod { DisplayText = "3 mois", Since = DateTime.Now.AddMonths(-3) });
            _periods.Add(new HistoricalPeriod { DisplayText = "1 mois", Since = DateTime.Now.AddMonths(-1) });
            PeriodComboBox.ItemsSource = _periods;
        }
    }
}
