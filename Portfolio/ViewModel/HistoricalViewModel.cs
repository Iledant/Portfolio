using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;
using YahooFinanceApi;

namespace Portfolio.ViewModel
{
    public class HistoricalViewModel : Bindable
    {
        private IReadOnlyList<Candle> _history;
        private string _errorMessage;
        private List<Fund> _funds;

        public IReadOnlyList<Candle> History
        {
            get => _history;

            set
            {
                _history = value;
                OnPropertyChanged(nameof(History));
            }
        }

        public List<Fund> Funds
        {
            get => _funds;

            set
            {
                _funds = value;
                OnPropertyChanged(nameof(Funds));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public async void GetHistorical(string pattern, DateTime startTime, DateTime? endTime = null)
        {
            if (endTime is null)
            {
                endTime = DateTime.Now;
            }
            Yahoo.IgnoreEmptyRows = true;
            try
            {
                History = await Yahoo.GetHistoricalAsync(pattern, startTime, endTime);
                ErrorMessage = "";
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Invalid ticker"))
                {
                    ErrorMessage = "Impossible de trouver la valeur";
                }
            }
        }

        public void GetFunds()
        {
            Funds = FundRepository.Get("");
        }
    }
}
