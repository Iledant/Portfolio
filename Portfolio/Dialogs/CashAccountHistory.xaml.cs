#nullable enable
using Portfolio.Models;
using Portfolio.Repositories;
using Portfolio.ViewModel;
using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Portfolio.Dialogs
{
    public sealed partial class CashAccountHistory : ContentDialog
    {
        private static readonly CultureInfo _ci = new("fr-FR");
        private static readonly SymbolIcon _addIcon = new(Symbol.Add);
        private static readonly SymbolIcon _modifyIcon = new(Symbol.Edit);
        private readonly CashAccountHistoryViewModel ViewModel;
        private readonly int _portfolioID;
        private double? _amount = null;
        private DateTime? _date = null;
        private string? _type = null;
        private CashAccountMovement? _movement = null;

        public CashAccountHistory()
        {
            InitializeComponent();
            ViewModel = new();
        }

        public CashAccountHistory(int portfolioID)
        {
            InitializeComponent();
            ViewModel = new();
            _portfolioID = portfolioID;
            ViewModel.FetchMovements(portfolioID);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void AddModifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_amount is null || _type is null)
            {
                throw new ArgumentNullException();
            }
            if (_movement == null)
            {
                double value = _type == "Versement" ? (double)_amount : -(double)_amount;
                CashAccountRepository.Add(new CashAccountLine(portfolioID: _portfolioID, portfolioLineID: null, date: _date, value: value));
            }
            else
            {
                CashAccountLine line = new(id: _movement.ID, portfolioID: _portfolioID, date: _date, value: (double)_amount, portfolioLineID: null);
                CashAccountRepository.Update(line);
            }
            ClearMovementPanel();
            ViewModel.FetchMovements(_portfolioID);
        }

        private void ClearMovementPanel()
        {
            AmountTextBox.Text = "";
            DatePicker.SelectedDate = null;
            TypeComboBox.SelectedItem = null;
            AddModifyButton.IsEnabled = false;
            AddModifyButton.Content = _addIcon;
            DeleteButton.IsEnabled = false;
        }

        private void TypeComboBox_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            _type = TypeComboBox.SelectedItem as string;
            CheckIsAddModifyButtonEnabled();
        }

        private void AmountTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            try
            {
                _amount = double.Parse(AmountTextBox.Text, _ci.NumberFormat);
            }
            catch
            {
                _amount = null;
            }
            CheckIsAddModifyButtonEnabled();
        }

        private void CheckIsAddModifyButtonEnabled()
        {
            AddModifyButton.IsEnabled = _date is not null
                && _amount is not null
                && _type is not null;
        }

        private void DatePicker_SelectedDateChanged(DatePicker _, DatePickerSelectedValueChangedEventArgs args)
        {
            _date = args.NewDate?.DateTime;
            CheckIsAddModifyButtonEnabled();
        }

        private void MovementsList_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            _movement = MovementsList.SelectedItem as CashAccountMovement;
            ClearMovementPanel();
            if (_movement == null || _movement.PortfolioLineID is not null)
            {
                return;
            }
            _date = _movement.Date;
            _amount = _movement.Value;
            _type = _movement.Type();
            DatePicker.SelectedDate = _date;
            AmountTextBox.Text = ((double)_amount).ToString("G");
            TypeComboBox.SelectedItem = _type;
            AddModifyButton.Content = _modifyIcon;
            DeleteButton.IsEnabled = true;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _movement = MovementsList.SelectedItem as CashAccountMovement;
            ClearMovementPanel();
            if (_movement == null || _movement.PortfolioLineID is not null)
            {
                return;
            }
            CashAccountRepository.Delete(_movement.ID);
            ViewModel.FetchMovements(_portfolioID);
        }
    }
}
