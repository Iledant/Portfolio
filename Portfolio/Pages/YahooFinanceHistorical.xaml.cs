using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Portfolio.Pages
{
    /// <summary>
    /// La page permettant de charger l'historique d'une valeur depuis l'API Yahoo finances.
    /// </summary>
    public sealed partial class YahooFinanceHistorical : Page
    {
        private readonly YahooFinanceViewModel ViewModel;

        public YahooFinanceHistorical()
        {
            InitializeComponent();
            ViewModel = new YahooFinanceViewModel();
            ViewModel.GetFunds();
        }

        private async void FetchStockButton_Click(object _1, RoutedEventArgs _2)
        {
            if (StockNameTextBox.Text != "")
            {
                DateTimeOffset beginDate = (BeginDatePicker.SelectedDate != null)
                    ? BeginDatePicker.Date
                    : DateTime.Now.AddDays(-10);
                ProgressBar.IsIndeterminate = true;
                ErrorText.Visibility = Visibility.Collapsed;
                await Task.Delay(1);
                ViewModel.GetHistorical(StockNameTextBox.Text, beginDate.DateTime);
                await Task.Delay(1);
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = 100;
                if (ViewModel.ErrorMessage != "")
                {
                    ErrorText.Visibility = Visibility.Visible;
                }
            }
        }

        private async void FundSelector_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            if (FundSelector.SelectedItem is not null and Fund)
            {
                Fund selectedFund = FundSelector.SelectedItem as Fund;
                ProgressBar.IsIndeterminate = true;
                ErrorText.Visibility = Visibility.Collapsed;
                await Task.Delay(1);
                ViewModel.GetHistorical(selectedFund.YahooCode, DateTime.Now.AddYears(-5));
                await Task.Delay(1);
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = 100;
                if (ViewModel.ErrorMessage != "")
                {
                    ErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    FundRepository.AddHistorical(selectedFund, ViewModel.History);
                }
            }
        }
    }
}
