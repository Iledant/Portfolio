using Portfolio.Controls;
using Portfolio.Repositories;
using System.Collections.Generic;

namespace Portfolio.ViewModel
{
    public enum HeaderName { None = -1, Fund = 0, Quantity = 1, AverageValue = 2, Value = 3, Gain = 4, Performance = 5 };

    public class PortfolioPerformanceViewModel : Bindable
    {
        private HeaderSortState _headerState = HeaderSortState.Ascending;
        private HeaderName _selectedHeader = HeaderName.Fund;

        public List<PortFolioLineValue> Values { get; private set; } = new();
        public HeaderSortState FundHeaderState => GetHeaderState(HeaderName.Fund);
        public HeaderSortState QuantityHeaderState => GetHeaderState(HeaderName.Quantity);
        public HeaderSortState AverageValueHeaderState => GetHeaderState(HeaderName.AverageValue);
        public HeaderSortState ValueHeaderState => GetHeaderState(HeaderName.Value);
        public HeaderSortState GainHeaderState => GetHeaderState(HeaderName.Gain);
        public HeaderSortState PerformanceHeaderState => GetHeaderState(HeaderName.Performance);

        public void FetchValues(int portfolioID)
        {
            Values = PortfolioRepository.GetActualValue(portfolioID);
        }

        private HeaderSortState GetHeaderState(HeaderName name)
        {
            return name == _selectedHeader ? _headerState : HeaderSortState.Neutral;
        }

        public void SelectHeader(HeaderName newSelected)
        {
            if (newSelected != _selectedHeader)
            {
                HeaderName oldSelected = _selectedHeader;
                _selectedHeader = newSelected;
                _headerState = HeaderSortState.Ascending;
                NotifyChange(oldSelected);
                NotifyChange(newSelected);
                return;
            }

            _headerState = _headerState switch
            {
                HeaderSortState.Ascending => HeaderSortState.Descending,
                HeaderSortState.Descending => HeaderSortState.Neutral,
                HeaderSortState.Neutral => HeaderSortState.Ascending,
                _ => throw new System.NotImplementedException()
            };
            NotifyChange(newSelected);
        }

        private void NotifyChange(HeaderName name)
        {
            switch (name)
            {
                case HeaderName.None:
                    break;
                case HeaderName.Fund:
                    OnPropertyChanged(nameof(FundHeaderState));
                    break;
                case HeaderName.Quantity:
                    OnPropertyChanged(nameof(QuantityHeaderState));
                    break;
                case HeaderName.AverageValue:
                    OnPropertyChanged(nameof(AverageValueHeaderState));
                    break;
                case HeaderName.Value:
                    OnPropertyChanged(nameof(ValueHeaderState));
                    break;
                case HeaderName.Gain:
                    OnPropertyChanged(nameof(GainHeaderState));
                    break;
                case HeaderName.Performance:
                    OnPropertyChanged(nameof(PerformanceHeaderState));
                    break;
            }
        }
    }
}
