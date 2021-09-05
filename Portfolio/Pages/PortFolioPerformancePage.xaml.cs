using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using System.Globalization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de présentation des performances d'un portefeuille.
    /// </summary>
    public sealed partial class PortFolioPerformancePage : Page
    {
        #region private members
        private PortFolio _portfolio;
        private readonly CultureInfo _ci = new("fr-FR");
        private readonly PortfolioPerformanceViewModel ViewModel;
        #endregion

        #region constructor
        public PortFolioPerformancePage()
        {
            InitializeComponent();
            ViewModel = new();
        }
        #endregion

        #region private methods
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is not PortFolio)
            {
                throw new ArgumentException();
            }
            _portfolio = e.Parameter as PortFolio;

            ViewModel.FetchValues(_portfolio.ID);
            base.OnNavigatedTo(e);
        }
        #endregion

        private void PerformanceTable_SortClicked(object sender, Controls.HeaderSort e)
        {
            ViewModel.Sort(e.Index, e.State);
        }
    }
}
