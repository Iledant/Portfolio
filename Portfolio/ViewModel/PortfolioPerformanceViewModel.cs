using Portfolio.Controls;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI.Xaml;

namespace Portfolio.ViewModel
{
    public enum HeaderName { None = -1, Fund = 0, Quantity = 1, AverageValue = 2, Value = 3, Gain = 4, Performance = 5 };

    public class TextCell : ICellContent
    {
        public TextAlignment Alignment;
        public string Text;
        private readonly static CultureInfo _ci = new("fr-FR");

        public TextCell(string text, TextAlignment alignment)
        {
            Text = text;
            Alignment = alignment;
        }

        public static TextCell FromDouble(double value)
        {
            return new(value.ToString("N2", _ci), TextAlignment.Right);
        }

        public static TextCell FromCurrency(double currency)
        {
            return new(currency.ToString("C", _ci), TextAlignment.Right);
        }

        public static TextCell FromPercentage(double percentage)
        {
            return new(percentage.ToString("P", _ci), TextAlignment.Right);
        }

        public TextAlignment GetAlignment()
        {
            return Alignment;
        }

        public string GetText()
        {
            return Text;
        }
    }

    public class PortfolioPerformanceViewModel : Bindable
    {
        private HeaderSortState _headerState = HeaderSortState.Ascending;
        private HeaderName _selectedHeader = HeaderName.Fund;
        private double _totalInitialValue, _totalActualValue, _totalGain, _totalPerformance;
        private TableContent _tableContent;

        public List<PortFolioLineValue> Values { get; private set; } = new();
        //public HeaderSortState FundHeaderState => GetHeaderState(HeaderName.Fund);
        //public HeaderSortState QuantityHeaderState => GetHeaderState(HeaderName.Quantity);
        //public HeaderSortState AverageValueHeaderState => GetHeaderState(HeaderName.AverageValue);
        //public HeaderSortState ValueHeaderState => GetHeaderState(HeaderName.Value);
        //public HeaderSortState GainHeaderState => GetHeaderState(HeaderName.Gain);
        //public HeaderSortState PerformanceHeaderState => GetHeaderState(HeaderName.Performance);

        public TableContent TableContent
        {
            get => _tableContent;
            set
            {
                _tableContent = value;
                OnPropertyChanged(nameof(TableContent));
            }
        }

        public void FetchValues(int portfolioID)
        {
            Values = PortfolioRepository.GetActualValue(portfolioID);
            _totalActualValue = 0;
            _totalInitialValue = 0;
            foreach (PortFolioLineValue line in Values)
            {
                _totalInitialValue += line.AverageValue * line.Quantity;
                _totalActualValue += line.ActualValue * line.Quantity;
            }
            _totalGain = _totalActualValue - _totalInitialValue;
            _totalPerformance = _totalGain / _totalInitialValue;

            List<ICellContent> headers = new();
            headers.Add(new TextCell("Fond", TextAlignment.Center));
            headers.Add(new TextCell("Quantité", TextAlignment.Center));
            headers.Add(new TextCell("PRM", TextAlignment.Center));
            headers.Add(new TextCell("Valeur", TextAlignment.Center));
            headers.Add(new TextCell("Gain", TextAlignment.Center));
            headers.Add(new TextCell("Perf.", TextAlignment.Center));
            headers.Add(new TextCell("sem.", TextAlignment.Center));
            headers.Add(new TextCell("mois", TextAlignment.Center));
            headers.Add(new TextCell("an", TextAlignment.Center));

            List<ICellContent> bottomCells = new();
            bottomCells.Add(new TextCell("Total", TextAlignment.Left));
            bottomCells.Add(new TextCell("", TextAlignment.Left));
            bottomCells.Add(TextCell.FromCurrency(_totalInitialValue));
            bottomCells.Add(TextCell.FromCurrency(_totalActualValue));
            bottomCells.Add(TextCell.FromCurrency(_totalGain));
            bottomCells.Add(TextCell.FromPercentage(_totalPerformance));
            bottomCells.Add(new TextCell("", TextAlignment.Left));
            bottomCells.Add(new TextCell("", TextAlignment.Left));
            bottomCells.Add(new TextCell("", TextAlignment.Left));

            TableContent = new TableContent(headers, GenerateCells(), bottomCells);
        }

        private List<List<ICellContent>> GenerateCells()
        {
            List<List<ICellContent>> cells = new();
            foreach (PortFolioLineValue line in Values)
            {
                List<ICellContent> row = new();
                row.Add(new TextCell(line.FundName, TextAlignment.Left));
                row.Add(TextCell.FromDouble(line.Quantity));
                row.Add(TextCell.FromCurrency(line.AverageValue));
                row.Add(TextCell.FromCurrency(line.ActualValue));
                row.Add(TextCell.FromCurrency(line.Gain));
                row.Add(TextCell.FromPercentage(line.Evolution));
                row.Add(TextCell.FromPercentage(line.WeekGain));
                row.Add(TextCell.FromPercentage(line.MonthGain));
                row.Add(TextCell.FromPercentage(line.YearGain));
                cells.Add(row);
            }
            return cells;
        }

        public void Sort(int index, HeaderSortState sortState)
        {
            if (sortState == HeaderSortState.Neutral)
            {
                Values.Sort(IDCompare);
            }
            else
            {
                Comparison<PortFolioLineValue> comparer = index switch
                {
                    0 => FundCompare(_headerState),
                    1 => QuantityCompare(_headerState),
                    2 => AverageValueCompare(_headerState),
                    3 => ValueCompare(_headerState),
                    4 => GainCompare(_headerState),
                    5 => PerformanceCompare(_headerState),
                    6 => WeekGainCompare(_headerState),
                    7 => MonthGainCompare(_headerState),
                    8 => YearGainCompare(_headerState),
                    _ => throw new ArgumentException()
                };
                Values.Sort(comparer);
            }
            TableContent.Cells = GenerateCells();
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
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.ActualValue.CompareTo(b.ActualValue);
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

        private static Comparison<PortFolioLineValue> WeekGainCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.WeekGain.CompareTo(b.Evolution);
        }

        private static Comparison<PortFolioLineValue> MonthGainCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.MonthGain.CompareTo(b.Evolution);
        }

        private static Comparison<PortFolioLineValue> YearGainCompare(HeaderSortState s)
        {
            int sign = s == HeaderSortState.Ascending ? 1 : -1;
            return (PortFolioLineValue a, PortFolioLineValue b) => sign * a.YearGain.CompareTo(b.Evolution);
        }

        private HeaderSortState GetHeaderState(HeaderName name)
        {
            return name == _selectedHeader ? _headerState : HeaderSortState.Neutral;
        }
    }
}
