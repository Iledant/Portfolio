using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Portfolio.Pages
{
    /// <summary>
    /// La page de recherche de fonds sur l'API de Yahoo finances.
    /// </summary>
    public sealed partial class FindFundPage : Page
    {
        private readonly YahooFinanceViewModel ViewModel;
        private int _companyID = 0;

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

        private void Page_Loaded(object _1, RoutedEventArgs _2)
        {
            ViewModel.GetCompanies();
        }

        private void AddFundButton_Click(object _1, RoutedEventArgs _2)
        {
            ErrorStackPanel.Visibility =
                (FundRepository.AddQuote(QuotesList.SelectedItem as Quote, _companyID) == DBState.AlreadyExists)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void CompanyComboBox_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
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
        }
    }
}
