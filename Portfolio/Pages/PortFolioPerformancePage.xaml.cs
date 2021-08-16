using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    /// <summary>
    /// La page de présentation des performances d'un portefeuille.
    /// </summary>
    public sealed partial class PortFolioPerformancePage : Page
    {
        #region private members
        private PortFolio _portfolio;
        private readonly CultureInfo _ci = new("fr-FR");
        private double _tableHeight;
        private int _rowsCount;
        private int _pointeredRow = 0;
        private readonly PortfolioPerformanceViewModel ViewModel;
        #endregion

        #region constructor
        public PortFolioPerformancePage()
        {
            InitializeComponent();
            ViewModel = new();
        }
        #endregion

        #region property dependency
        public Brush CellBackground
        {
            get { return (Brush)GetValue(CellBackgroundProperty); }
            set { SetValue(CellBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CellBackgroundProperty =
            DependencyProperty.Register(nameof(CellBackground),
                typeof(Brush),
                typeof(PortFolioPerformancePage),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 10,16,16))));

        public Brush PointeredCellBackground
        {
            get { return (Brush)GetValue(PointeredCellBackgroundProperty); }
            set { SetValue(PointeredCellBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PointeredCellBackgroundProperty =
            DependencyProperty.Register(nameof(PointeredCellBackground),
                typeof(Brush),
                typeof(PortFolioPerformancePage),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 19, 31, 32))));

        public Thickness CellPadding
        {
            get { return (Thickness)GetValue(CellPaddingProperty); }
            set { SetValue(CellPaddingProperty, value); }
        }

        public static readonly DependencyProperty CellPaddingProperty =
            DependencyProperty.Register(nameof(CellPadding),
                typeof(int),
                typeof(PortFolioPerformancePage),
                new PropertyMetadata(new Thickness(6)));
        #endregion

        #region events methods
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is not PortFolio)
            {
                throw new ArgumentException();
            }
            _portfolio = e.Parameter as PortFolio;

            FetchValuesAndGenerateTable();
            base.OnNavigatedTo(e);
        }

        private void Table_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Pointer ptr = e.Pointer;
            if (ptr.PointerDeviceType == PointerDeviceType.Mouse)
            {
                PointerPoint point = e.GetCurrentPoint(Table);
                int row = (int)(point.Position.Y * _rowsCount / _tableHeight);
                if (row != _pointeredRow)
                {
                    ClearPointeredRow();
                    if (row > 0)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Border border = Table.Children[6 * row + i] as Border;
                            border.Background = PointeredCellBackground;
                        }
                    }
                    _pointeredRow = row;
                }
            }
            e.Handled = true;
        }

        private void Table_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ClearPointeredRow();
            _pointeredRow = 0;
        }

        private void FundSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Fund);
        }

        private void QuantitySort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Quantity);
        }

        private void AverageValueSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.AverageValue);
        }

        private void ValueSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Value);
        }

        private void GainSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Gain);
        }

        private void PerformanceSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Performance);
        }
        #endregion

        #region private methods
        private void FetchValuesAndGenerateTable()
        {
            List<PortFolioLineValue> values = PortfolioRepository.GetActualValue(_portfolio.ID);

            RowDefinition rowDefinition = new();
            rowDefinition.Height = GridLength.Auto;
            for (int i = 0; i < values.Count; i++)
            {
                Table.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < values.Count; i++)
            {
                AddCell(values[i].FundName, TextAlignment.Left, 0, i + 1);
                AddCell(values[i].Quantity.ToString("N2", _ci), TextAlignment.Right, 1, i + 1);
                AddCell(values[i].AverageValue.ToString("C", _ci), TextAlignment.Left, 2, i + 1);
                AddCell(values[i].FundActualValue.ToString("C", _ci), TextAlignment.Left, 3, i + 1);
                AddCell(values[i].Gain.ToString("C", _ci), TextAlignment.Left, 4, i + 1);
                AddCell(values[i].Evolution.ToString("P", _ci), TextAlignment.Left, 5, i + 1);
            }
            Table.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _tableHeight = Table.DesiredSize.Height;
            _rowsCount = values.Count + 1;
        }

        private void AddCell(string text, TextAlignment alignment, int column, int row)
        {
            TextBlock textblock = new();
            textblock.Text = text;
            textblock.TextAlignment = alignment;
            Border border = new();
            border.Child = textblock;
            border.Padding = CellPadding;
            border.Background = CellBackground;
            Table.Children.Add(border);
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);
        }

        private void ClearPointeredRow()
        {
            if (_pointeredRow == 0)
            {
                return;
            }

            for (int i = 0; i < 6; i++)
            {
                (Table.Children[6 * _pointeredRow + i] as Border).Background = CellBackground;
            }
        }
        #endregion
    }
}
