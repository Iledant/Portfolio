using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public class PortfolioHistoricalViewModel : Bindable
    {
        private List<FundData> _values = new();
        private bool _isAverageEnabled;
        private int _averageCount = 5;

        public bool IsAverageEnabled
        {
            get => _isAverageEnabled;
            set
            {
                _isAverageEnabled = value;
                OnPropertyChanged(nameof(IsAverageEnabled));
            }
        }

        public int AverageCount
        {
            get => _averageCount;
            set
            {
                _averageCount = value;
                OnPropertyChanged(nameof(AverageCount));
            }
        }

        public List<FundData> Values
        {
            get => _values;
            set
            {
                _values = value;
                OnPropertyChanged(nameof(Values));
            }
        }

        public void FetchValues(PortFolio portFolio, DateTime? begin = null, DateTime? end = null)
        {
            Values = PortfolioRepository.GetHistorical(portFolio.ID, begin, end);
        }
    }
}
