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
                ISINTextBlock.Text = _fund.ISIN ?? "-";
                YahooCodeTextBlock.Text = _fund.YahooCode?? "-";
                CommentTextBlock.Text = _fund.Comment ?? "-";
                Chart.Values = FundRepository.GetFundDatas(_fund.ID);
                Title.Text += $"« {_fund.Name} »";
            }
            base.OnNavigatedTo(e);

        }
    }
}
