using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Portfolio.Dialogs
{
    public class AccountItem
    {
        public string Name;
        public int ID;
        public bool IsCashAccount;

        public AccountItem(string text, int id, bool isCashAccount)
        {
            Name = text;
            ID = id;
            IsCashAccount = isCashAccount;
        }
    }

    public sealed partial class PortFolioLineEditDialog : ContentDialog
    {
        private readonly List<Fund> _funds;
        private readonly PortFolioLine _line;
        private double? _quantity;
        private double? _purchaseValue;
        private DateTime? _date;
        private bool _buy;
        private AccountItem _sourceAccount;

        #region constructor
        public PortFolioLineEditDialog()
        {
            InitializeComponent();
        }

        public PortFolioLineEditDialog(PortFolioLine portFolioLine)
        {
            InitializeComponent();
            _line = portFolioLine;
            _funds = FundRepository.Get("");
            FillAccountComboBox();
            FundComboBox.ItemsSource = _funds;
            if (_line.ID == 0)
            {
                _quantity = null;
                _purchaseValue = null;
                _date = null;
                _buy = true;
                _sourceAccount = null;
                FundComboBox.SelectedItem = null;
                QuantityTextBox.Text = "";
                AveragePriceTextBox.Text = "";
                PrimaryButtonText = "Créer";
                Title = "Créer une nouvelle ligne";
            }
            else
            {
                foreach (Fund f in _funds)
                {
                    if (f.ID == _line.FundID)
                    {
                        FundComboBox.SelectedItem = f;
                        break;
                    }
                }
                _date = _line.Date;
                _quantity = _line.Quantity;
                _purchaseValue = Math.Abs((double)_line.PurchaseVal);
                QuantityTextBox.Text = _quantity.ToString();
                AveragePriceTextBox.Text = _purchaseValue.ToString();
                foreach (AccountItem account in AccountSource.Items)
                {
                    if (account.ID == _line.AccountID)
                    {
                        AccountSource.SelectedItem = account;
                        _sourceAccount = account;
                        break;
                    }
                }
                PrimaryButtonText = "Modifier";
                Title = "Modifier la ligne";
            }
            DatePicker.SelectedDate = _date;
            BuyButton.IsChecked = _buy;
            SellButton.IsChecked = !_buy;
            CheckValues();
        }
        #endregion

        #region eventHandlers
        private void ContentDialog_PrimaryButtonClick(ContentDialog _1, ContentDialogButtonClickEventArgs _2)
        {
            Fund fund = FundComboBox.SelectedItem as Fund;
            PortFolioLine line = new(id: _line.ID,
                fundId: fund.ID,
                quantity: _buy ? (double)_quantity: -(double)_quantity,
                purchaseVal: _purchaseValue,
                date: _date,
                portFolioID: _line.PortFolioID,
                accountID: _sourceAccount.ID);
            if (line.ID == 0)
            {
                PortfolioLineRepository.Insert(line);
            }
            else
            {
                PortfolioLineRepository.Update(line);
            }
        }

        private void QuantityTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            try
            {
                _quantity = double.Parse(QuantityTextBox.Text);
            }
            catch (Exception)
            {
                _quantity = null;
            }
            CheckValues();
        }

        private void FundComboBox_SelectionChanged(object _1, SelectionChangedEventArgs _2)
        {
            if (FundComboBox.IsLoaded)
            {
                CheckValues();
            }
        }

        private void DatePicker_DateChanged(object _, DatePickerValueChangedEventArgs e)
        {
            _date = e.NewDate.DateTime;
            CheckValues();
        }

        private void AveragePriceTextBox_TextChanged(object _1, TextChangedEventArgs _2)
        {
            try
            {
                _purchaseValue = double.Parse(AveragePriceTextBox.Text);
            }
            catch
            {
                _purchaseValue = null;
            }
            CheckValues();
        }
        #endregion

        #region private methods
        private void CheckValues()
        {
            IsPrimaryButtonEnabled = _quantity is not null
                && _date is not null
                && _purchaseValue is not null
                && FundComboBox.SelectedItem is not null
                && _sourceAccount is not null;
        }

        private void SellBuyChecked(object sender, RoutedEventArgs e)
        {
            _buy = sender == BuyButton;
        }

        private void FillAccountComboBox()
        {
            List<AccountItem> accountItems = new();
            accountItems.Add(new("Compte courant", 0, true));
            List<MonetaryAccount> monetaryAccounts = PortfolioRepository.GetMonetaryAccounts(_line.PortFolioID);
            foreach (MonetaryAccount account in monetaryAccounts)
            {
                accountItems.Add(new(account.Name, account.ID, false));
            }
            AccountSource.ItemsSource = accountItems;
        }
        #endregion

        private void AccountSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _sourceAccount = AccountSource.SelectedItem as AccountItem;
            CheckValues();
        }
    }
}
