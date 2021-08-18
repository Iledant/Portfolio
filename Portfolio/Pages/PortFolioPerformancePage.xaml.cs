﻿using Portfolio.Models;
using Portfolio.ViewModel;
using System;
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
        private int _cellBeginIndex = 0;
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
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 10, 16, 16))));

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

            ViewModel.FetchValues(_portfolio.ID);
            GenerateTable();
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
            UpdateCellTexts();
        }

        private void QuantitySort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Quantity);
            UpdateCellTexts();
        }

        private void AverageValueSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.AverageValue);
            UpdateCellTexts();
        }

        private void ValueSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Value);
            UpdateCellTexts();
        }

        private void GainSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Gain);
            UpdateCellTexts();
        }

        private void PerformanceSort_Clicked(object sender, EventArgs e)
        {
            ViewModel.SelectHeader(HeaderName.Performance);
            UpdateCellTexts();
        }
        #endregion

        #region private methods
        private void GenerateTable()
        {
            for (int i = 0; i < ViewModel.Values.Count; i++)
            {
                Table.RowDefinitions.Add(new RowDefinition());
            }

            _cellBeginIndex = Table.Children.Count;
            for (int i = 0; i < ViewModel.Values.Count; i++)
            {
                AddCell(ViewModel.Values[i].FundName, TextAlignment.Left, 0, i + 1);
                AddCell(ViewModel.Values[i].Quantity.ToString("N2", _ci), TextAlignment.Right, 1, i + 1);
                AddCell(ViewModel.Values[i].AverageValue.ToString("C", _ci), TextAlignment.Left, 2, i + 1);
                AddCell(ViewModel.Values[i].FundActualValue.ToString("C", _ci), TextAlignment.Left, 3, i + 1);
                AddCell(ViewModel.Values[i].Gain.ToString("C", _ci), TextAlignment.Left, 4, i + 1);
                AddCell(ViewModel.Values[i].Evolution.ToString("P", _ci), TextAlignment.Left, 5, i + 1);
            }
            Table.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _tableHeight = Table.DesiredSize.Height;
            _rowsCount = ViewModel.Values.Count + 1;
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

        private void UpdateCellTexts()
        {
            Border border;
            TextBlock textblock;
            for (int i = 0; i < ViewModel.Values.Count; i++)
            {
                border = Table.Children[_cellBeginIndex + i * 6] as Border;
                textblock = border.Child as TextBlock;
                textblock.Text = ViewModel.Values[i].FundName;
                border = Table.Children[_cellBeginIndex + i * 6 + 1] as Border;
                textblock = border.Child as TextBlock;
                textblock.Text = ViewModel.Values[i].Quantity.ToString("N2", _ci);
                border = Table.Children[_cellBeginIndex + i * 6 + 2] as Border;
                textblock = border.Child as TextBlock;
                textblock.Text = ViewModel.Values[i].AverageValue.ToString("C", _ci);
                border = Table.Children[_cellBeginIndex + i * 6 + 3] as Border;
                textblock = border.Child as TextBlock;
                textblock.Text = ViewModel.Values[i].FundActualValue.ToString("C", _ci);
                border = Table.Children[_cellBeginIndex + i * 6 + 4] as Border;
                textblock = border.Child as TextBlock;
                textblock.Text = ViewModel.Values[i].Gain.ToString("C", _ci);
                border = Table.Children[_cellBeginIndex + i * 6 + 5] as Border;
                textblock = border.Child as TextBlock;
                textblock.Text = ViewModel.Values[i].Evolution.ToString("P", _ci);
            }
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
