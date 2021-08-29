using Portfolio.Models;
using Portfolio.ViewModel;
using System.Globalization;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Portfolio.Pages
{
    public sealed partial class PortFolioFundsLookUp : Page
    {
        private readonly FundsLookUpViewModel ViewModel;
        private readonly CultureInfo _ci = new("fr-FR");

        public Brush CellForeground
        {
            get => (Brush)GetValue(CellForegroundProperty);
            set => SetValue(CellForegroundProperty, value);
        }

        public static readonly DependencyProperty CellForegroundProperty =
            DependencyProperty.Register(nameof(CellForeground),
                typeof(Brush),
                typeof(PortFolioFundsLookUp),
                new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush CellBackground
        {
            get => (Brush)GetValue(CellBackgroundProperty);
            set => SetValue(CellBackgroundProperty, value);
        }

        public static readonly DependencyProperty CellBackgroundProperty =
            DependencyProperty.Register(nameof(CellBackground),
                typeof(Brush),
                typeof(PortFolioPerformancePage),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 10, 16, 16))));

        public Thickness CellPadding
        {
            get => (Thickness)GetValue(CellPaddingProperty);
            set => SetValue(CellPaddingProperty, value);
        }

        public static readonly DependencyProperty CellPaddingProperty =
            DependencyProperty.Register(nameof(CellPadding),
                typeof(int),
                typeof(PortFolioPerformancePage),
                new PropertyMetadata(new Thickness(6)));

        public PortFolioFundsLookUp()
        {
            InitializeComponent();
            ViewModel = new();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PortFolio)
            {
                ViewModel.PortFolio = e.Parameter as PortFolio;
                ViewModel.FetchFunds();
                DrawTable();
            }

            base.OnNavigatedTo(e);
        }

        private void DrawTable()
        {
            AddRowDefinitions();
            AddLines();
        }

        private void AddRowDefinitions()
        {
            foreach (FundLookUp _ in ViewModel.Funds)
            {
                MainGrid.RowDefinitions.Add(new());
            }
        }

        private void AddLines()
        {
            for (int i = 0; i < ViewModel.Funds.Count; i++)
            {
                FundLookUp fund = ViewModel.Funds[i];
                AddCell(fund.FundName, TextAlignment.Left, 0, i + 1);
                AddCell(fund.ActualValue.ToString("C", _ci), TextAlignment.Left, 1, i + 1);
                AddCell(fund.WeekPerformance.ToString("P2"), TextAlignment.Left, 2, i + 1);
                AddCell(fund.MonthPerformance.ToString("P2"), TextAlignment.Left, 3, i + 1);
                AddCell(fund.YearPerformance.ToString("P2"), TextAlignment.Left, 4, i + 1);
            }
        }

        private void AddCell(string text, TextAlignment alignment, int column, int row, Brush background = null, Brush foreground = null)
        {
            TextBlock textblock = new();
            textblock.Text = text;
            textblock.Foreground = foreground ?? CellForeground;
            textblock.TextAlignment = alignment;
            Border border = new();
            border.Child = textblock;
            border.Padding = CellPadding;
            border.Background = background ?? CellBackground;
            MainGrid.Children.Add(border);
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);
        }

    }
}
