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

        private static Comparison<PortFolioLineValue> FundCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.FundName.CompareTo(b.FundName);
        }

        private static int IDCompare(PortFolioLineValue a, PortFolioLineValue b) => a.FundID.CompareTo(b.FundID);

        private static Comparison<PortFolioLineValue> QuantityCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.Quantity.CompareTo(b.Quantity);
        }

        private static Comparison<PortFolioLineValue> AverageValueCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.AverageValue.CompareTo(b.AverageValue);
        }

        private static Comparison<PortFolioLineValue> ValueCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.FundActualValue.CompareTo(b.FundActualValue);
        }

        private static Comparison<PortFolioLineValue> GainCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.Gain.CompareTo(b.Gain);
        }

        private static Comparison<PortFolioLineValue> PerformanceCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.Evolution.CompareTo(b.Evolution);
        }

        private HeaderSortState GetHeaderState(HeaderName name)
        {
            return name == _selectedHeader ? _headerState : HeaderSortState.Neutral;
        }

        private void SortValues()
        {
            if (_headerState == HeaderSortState.Neutral)
            {
                Values.Sort(IDCompare);
                return;
            }

            Comparison<PortFolioLineValue> comparer = _selectedHeader switch
            {
                HeaderName.None => IDCompare,
                HeaderName.Fund => FundCompare(_headerState),
                HeaderName.Quantity => QuantityCompare(_headerState),
                HeaderName.AverageValue => AverageValueCompare(_headerState),
                HeaderName.Value => ValueCompare(_headerState),
                HeaderName.Gain => GainCompare(_headerState),
                HeaderName.Performance => PerformanceCompare(_headerState),
                _ => throw new ArgumentException()
            };

            Values.Sort(comparer);
        }

        private void NotifyChange(HeaderName name)
        {
            string propertyName = name switch
            {
                HeaderName.None => null,
                HeaderName.Fund =>
                    nameof(FundHeaderState),
                HeaderName.Quantity =>
                    nameof(QuantityHeaderState),
                HeaderName.AverageValue =>
                    nameof(AverageValueHeaderState),
                HeaderName.Value =>
                    nameof(ValueHeaderState),
                HeaderName.Gain =>
                    nameof(GainHeaderState),
                HeaderName.Performance =>
                    nameof(PerformanceHeaderState),
                _ => throw new ArgumentException()
            };
            OnPropertyChanged(propertyName);
    }
    }
}
