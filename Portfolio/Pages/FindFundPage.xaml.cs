using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Portfolio.Pages
{
    /// <summary>
    /// La page de recherche de fonds sur l'API de Yahoo finances.
    /// </summary>
    public sealed partial class FindFundPage : Page
    {
        private readonly APIFinanceViewModel ViewModel;
        private int _companyID = 0;
        private int _morningstarCompanyID = 0;

        public FindFundPage()
        {
            InitializeComponent();
            ViewModel = new();
            ViewModel.GetCompanies();
        }

        private void QuoteSearchButton_Click(object _1, RoutedEventArgs _2)
        {
            ViewModel.PickStocks(QuoteTextBox.Text);
        }

        private async void AddFundButton_Click(object _1, RoutedEventArgs _2)
        {
            (Fund fund, DBState state) = FundRepository.AddQuote(QuotesList.SelectedItem as Quote, _companyID);
            ErrorStackPanel.Visibility = (state == DBState.AlreadyExists)
                ? Visibility.Visible
                : Visibility.Collapsed;
            OkStackPanel.Visibility = (state == DBState.OK) ? Visibility.Visible : Visibility.Collapsed;
            await Task.Delay(1);
            await FundRepository.UpdateHistorical(fund);
        }

        private void CompanyComboBox_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            HideStackPanels();
            if (CompanyComboBox.SelectedItem is Company)
            {
                _companyID = (CompanyComboBox.SelectedItem as Company).ID;
                AddFundButton.IsEnabled = true;
            }
            else
            {
                AddFundButton.IsEnabled = false;
            }
        }

        private void QuotesList_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            CompanyComboBox.IsEnabled = QuotesList.SelectedItem is Quote;
            HideStackPanels();
        }

        private void QuoteTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            HideStackPanels();
        }

        private void HideStackPanels()
        {
            ErrorStackPanel.Visibility = Visibility.Collapsed;
            OkStackPanel.Visibility = Visibility.Collapsed;
        }

        private async void MorningstarSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (MorningstarSearch.Text != "")
            {
                MorningstarProgressBar.IsIndeterminate = true;
                await Task.Delay(1);
                ViewModel.MorningstarSearch(MorningstarSearch.Text);
                MorningstarProgressBar.IsIndeterminate = false;
                await Task.Delay(1);
            }
        }

        private void MorningstarCompanyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HideMorningstarStackPanels();
            if (MorningstarCompanyComboBox.SelectedItem is Company)
            {
                _morningstarCompanyID = (MorningstarCompanyComboBox.SelectedItem as Company).ID;
                MorningstarAddFundButton.IsEnabled = true;
            }
            else
            {
                MorningstarAddFundButton.IsEnabled = false;
            }
        }

        private async void MorningstarAddFundButton_Click(object sender, RoutedEventArgs e)
        {
            (Fund fund, DBState state) = FundRepository.AddMorningstarFund(MorningstarStocksList.SelectedItem as MorningstarResponseLine, _morningstarCompanyID);
            MorningstarErrorStackPanel.Visibility = (state == DBState.AlreadyExists)
                ? Visibility.Visible
                : Visibility.Collapsed;
            MorningstarOkStackPanel.Visibility = (state == DBState.OK) ? Visibility.Visible : Visibility.Collapsed;
            await Task.Delay(1);
            await FundRepository.UpdateMorningstarHistorical(new FundRepository.FundUpdateLine(fundID: fund.ID, morningstarID: fund.MorningstarID, firstDate: null));
        }

        private void HideMorningstarStackPanels()
        {
            MorningstarErrorStackPanel.Visibility = Visibility.Collapsed;
            MorningstarOkStackPanel.Visibility = Visibility.Collapsed;
        }

        private void MorningstarStocksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MorningstarCompanyComboBox.IsEnabled = MorningstarStocksList.SelectedItem is MorningstarResponseLine;
            HideMorningstarStackPanels();
        }
    }
}
