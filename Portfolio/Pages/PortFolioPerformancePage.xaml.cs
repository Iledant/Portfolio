using Portfolio.Models;
using Portfolio.Repositories;
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
        private PortFolio _portfolio;
        private Thickness _cellPadding = new(6);
        private readonly CultureInfo _ci = new("fr-FR");
        private readonly SolidColorBrush _cellBackground = new(Color.FromArgb(255, 10, 10, 10));
        private readonly SolidColorBrush _pointeredCellBackground = new(Color.FromArgb(255, 20,20,20));
        private double _tableHeight;
        private int _rowsCount;
        private int _pointeredRow = 0;

        public PortFolioPerformancePage()
        {
            InitializeComponent();
        }

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
            border.Padding = _cellPadding;
            border.Background = _cellBackground;
            Table.Children.Add(border);
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);
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
                        for (int i = 0; i < 5; i++)
                        {
                            Border border = Table.Children[5 * row + i] as Border;
                            border.Background = _pointeredCellBackground;
                        }
                    }
                    _pointeredRow = row;
                }
            }
            e.Handled = true;
        }

        private void ClearPointeredRow()
        {
            if (_pointeredRow == 0)
            {
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                (Table.Children[5 * _pointeredRow + i] as Border).Background = _cellBackground;
            }
        }

        private void Table_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ClearPointeredRow();
            _pointeredRow = 0;
        }
    }
}
