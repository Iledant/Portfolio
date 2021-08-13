using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// Pour en savoir plus sur le modèle d'élément Contrôle utilisateur, consultez la page https://go.microsoft.com/fwlink/?LinkId=234236

namespace Portfolio.Controls
{
    public class HistoricalPeriod
    {
        public string DisplayText;
        public DateTime? Since;
    }

    public class PeriodChangedEventArgs : EventArgs
    {
        public DateTime? Since { get; set; }
    }

    public sealed partial class PeriodComboBox : UserControl
    {
        private readonly List<HistoricalPeriod> _periods = new();

        public event EventHandler<PeriodChangedEventArgs> PeriodChanged;

        public PeriodComboBox()
        {
            InitializeComponent();
            FillComboBox();
        }

        private void FillComboBox()
        {
            _periods.Add(new HistoricalPeriod { DisplayText = "Tout l'historique", Since = null });
            _periods.Add(new HistoricalPeriod { DisplayText = "2 ans", Since = DateTime.Now.AddYears(-2) });
            _periods.Add(new HistoricalPeriod { DisplayText = "Un an", Since = DateTime.Now.AddYears(-1) });
            _periods.Add(new HistoricalPeriod { DisplayText = "6 mois", Since = DateTime.Now.AddMonths(-6) });
            _periods.Add(new HistoricalPeriod { DisplayText = "3 mois", Since = DateTime.Now.AddMonths(-3) });
            _periods.Add(new HistoricalPeriod { DisplayText = "2 mois", Since = DateTime.Now.AddMonths(-2) });
            _periods.Add(new HistoricalPeriod { DisplayText = "1 mois", Since = DateTime.Now.AddMonths(-1) });
            Combo.ItemsSource = _periods;
            Combo.SelectedIndex = 0;
        }

        private void Combo_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            if (Combo.SelectedItem is null)
            {
                return;
            }
            HistoricalPeriod item = Combo.SelectedItem as HistoricalPeriod;
            PeriodChanged?.Invoke(this, new PeriodChangedEventArgs { Since = item.Since });
        }
    }
}
