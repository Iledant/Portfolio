using Portfolio.Controls;
using Portfolio.Repositories;
using System;
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

        private static int IDCompare(PortFolioLineValue a, PortFolioLineValue b) => a.FundID.CompareTo(b.FundID);

        private static int FundAscCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return string.Compare(a.FundName, b.FundName);

        }
        private static int FundDescCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return -string.Compare(a.FundName, b.FundName);
        }

        private static int QuantityAscCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return a.Quantity.CompareTo(b.Quantity);
        }

        private static int QuantityDescCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return -a.Quantity.CompareTo(b.Quantity);
        }

        private static int AverageValueAscCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return a.AverageValue.CompareTo(b.AverageValue);
        }

        private static int AverageValueDescCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return -a.AverageValue.CompareTo(b.AverageValue);
        }

        private static int ValueAscCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return a.FundActualValue.CompareTo(b.FundActualValue);
        }

        private static int ValueDescCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return -a.FundActualValue.CompareTo(b.FundActualValue);
        }

        private static int GainAscCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return a.Gain.CompareTo(b.Gain);
        }

        private static int GainDescCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return -a.Gain.CompareTo(b.Gain);
        }

        private static int PerformanceAscCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return a.Evolution.CompareTo(b.Evolution);
        }

        private static int PerformanceDescCompare(PortFolioLineValue a, PortFolioLineValue b)
        {
            return -a.Evolution.CompareTo(b.Evolution);
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

            }
            else
            {
                _headerState = _headerState switch
                {
                    HeaderSortState.Ascending => HeaderSortState.Descending,
                    HeaderSortState.Descending => HeaderSortState.Neutral,
                    HeaderSortState.Neutral => HeaderSortState.Ascending,
                    _ => throw new NotImplementedException()
                };
                NotifyChange(newSelected);
            }

            SortValues();
        }

        private void SortValues()
        {
            if (_headerState == HeaderSortState.Neutral)
            {
                Values.Sort(IDCompare);
                return;
            }

            switch (_selectedHeader)
            {
                case HeaderName.None:
                    Values.Sort(IDCompare);
                    break;
                case HeaderName.Fund:
                    Values.Sort(_headerState == HeaderSortState.Ascending ? FundAscCompare : FundDescCompare);
                    break;
                case HeaderName.Quantity:
                    Values.Sort(_headerState == HeaderSortState.Ascending ? QuantityAscCompare : QuantityDescCompare);
                    break;
                case HeaderName.AverageValue:
                    Values.Sort(_headerState == HeaderSortState.Ascending ? AverageValueAscCompare : AverageValueDescCompare);
                    break;
                case HeaderName.Value:
                    Values.Sort(_headerState == HeaderSortState.Ascending ? ValueAscCompare : ValueDescCompare);
                            break;
                case HeaderName.Gain:
                    Values.Sort(_headerState == HeaderSortState.Ascending ? GainAscCompare: GainDescCompare);
                    break;
                case HeaderName.Performance:
                    Values.Sort(_headerState == HeaderSortState.Ascending ? PerformanceAscCompare: PerformanceDescCompare);
                    break;
                default:
                    throw new NotImplementedException();
            }
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
