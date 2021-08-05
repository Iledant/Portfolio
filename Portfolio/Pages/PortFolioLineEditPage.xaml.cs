using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PortFolioLineEditPage : Page
    {
        private PortFolioLine _line;
        private Fund _fund;
        private double? _quantity = null;
        private double? _averageValue = null;
        private DateTime? _date = null;

        public PortFolioLineEditPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PortFolioLine and not null)
            {
                _line = e.Parameter as PortFolioLine;

                _fund = _line.ID == 0 ? new Fund() : new Fund(id: _line.FundID, name: _line.FundName);
                OkButton.Content = _line.ID == 0 ? "Créer" : "Modifier";
                FundName.Text = _fund.Name;
                Quantity.Text = _line.Quantity.ToString();
                _date = _line.Date;
                if (_line.Date != null)
                {
                    DatePicker.SelectedDate = _line.Date;
                }
                _quantity = _line.Quantity;
                _averageValue = _line.AverageVal;
                AveragePrice.Text = _line.AverageVal is not null ? _line.AverageVal.ToString() : "";
            }
            base.OnNavigatedTo(e);
        }


        private void Page_Loaded(object _1, RoutedEventArgs _2)
        {
            _ = FundName.Focus(FocusState.Programmatic);
        }

        private void CancelButton_Click(object _1, RoutedEventArgs _2)
        {
            _ = Frame.Navigate(typeof(PortFolioPage));
        }

        private void OkButton_Click(object _1, RoutedEventArgs _2)
        {
            PortFolioLine line = new(id: _line.ID,
                fundId: _fund.ID,
                quantity: _quantity ?? 1.0,
                averageVal: _averageValue,
                date: _date,
                portFolioID: _line.PortFolioID);
            _ = line.ID == 0 ?
                PortfolioLineRepository.Insert(line)
                : PortfolioLineRepository.Update(line);
            _ = Frame.Navigate(typeof(PortFolioPage),
                PortfolioRepository.GetByID(_line.PortFolioID));
        }

        private void CheckIfIsEnabled()
        {
            bool valueOrDate = _averageValue is not null || _date is not null;
            FundErrorCross.Visibility = _fund.ID == 0 ? Visibility.Visible : Visibility.Collapsed;
            QuantityErrorCross.Visibility = _quantity is null ? Visibility.Visible : Visibility.Collapsed;
            DateErrorCross.Visibility = valueOrDate ? Visibility.Collapsed : Visibility.Visible;
            AveragePriceErrorCross.Visibility = valueOrDate ? Visibility.Collapsed : Visibility.Visible;
            OkButton.IsEnabled = _fund.ID != 0 &&
                _quantity is not null &&
                valueOrDate;
        }

        private void Quantity_TextChanged(object _1, TextChangedEventArgs _2)
        {
            try
            {
                _quantity = double.Parse(Quantity.Text);
            }
            catch (Exception)
            {
                _quantity = null;
            }
            CheckIfIsEnabled();
        }

        private void DatePicker_DateChanged(object _1, DatePickerValueChangedEventArgs e)
        {
            _date = e.NewDate.DateTime;
            CheckIfIsEnabled();
        }

        private void AveragePRice_TextChanged(object _1, TextChangedEventArgs _2)
        {
            try
            {
                _averageValue = double.Parse(AveragePrice.Text);
            }
            catch (Exception)
            {
                _averageValue = null;
            }
            CheckIfIsEnabled();
        }

        private async void FundName_TextChanged(AutoSuggestBox _, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                async Task<bool> UserKeepsTyping()
                {
                    string txt = FundName.Text;
                    await Task.Delay(500);
                    return txt != FundName.Text;
                }
                if (await UserKeepsTyping())
                {
                    return;
                }

                List<Fund> suggestions = FundRepository.GetSuggestions(FundName.Text);
                FundName.ItemsSource = suggestions;
            }
        }

        private void FundName_SuggestionChosen(AutoSuggestBox _, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Fund)
            {
                FundName.Text = (args.SelectedItem as Fund).Name;
            }
        }

        private void FundName_QuerySubmitted(AutoSuggestBox _1, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion is not null and Fund)
            {
                _fund = args.ChosenSuggestion as Fund;
                FundName.Text = _fund.Name;
                CheckIfIsEnabled();
            }
        }
    }
}
