using Portfolio.Models;
using Portfolio.Repositories;
using System;
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
        private DateTime? _since = null;

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
                Chart.Values = FundRepository.GetFundDatas(_fund.ID, _since);
                Title.Text += $"« {_fund.Name} »";
            }
            base.OnNavigatedTo(e);

        }

        private void PeriodComboBox_PeriodChanged(object _, Controls.PeriodChangedEventArgs e)
        {
            if (e is not null)
            {
                _since = e.Since;
            }
            if (e is null ||_fund is null)
            {
                return;
            }
            Chart.Values = FundRepository.GetFundDatas(_fund.ID, _since);
        }
    }
}
