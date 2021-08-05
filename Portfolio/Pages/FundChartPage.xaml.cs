using Portfolio.Models;
using Portfolio.Repositories;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace Portfolio.Pages
{
    /// <summary>
    /// La page d'affichage du graphique de valeur d'un fond.
    /// </summary>
    public sealed partial class FundChartPage : Page
    {
        private Fund _fund;

        public FundChartPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Fund)
            {
                _fund = e.Parameter as Fund;

            }
            base.OnNavigatedTo(e);

        }

        private void Page_Loaded(object _1, Windows.UI.Xaml.RoutedEventArgs _2)
        {
            if (_fund != null)
            {
                Chart.Values = FundRepository.GetFundDatas(_fund.ID);
            }
        }
    }
}
