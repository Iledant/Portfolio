using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// Pour plus d'informations sur le modèle d'élément Boîte de dialogue de contenu, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace Portfolio.Dialogs
{
    public sealed partial class PortFolioLineEditDialog : ContentDialog
    {
        private readonly List<Fund> _funds;
        private readonly PortFolioLine _line;
        private double? _quantity;
        private double? _averageValue;
        private DateTime? _date;

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
            FundComboBox.ItemsSource = _funds;
            if (_line.ID == 0)
            {
                _quantity = null;
                _averageValue = null;
                _date = null;
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
                _averageValue = _line.AverageVal;
                QuantityTextBox.Text = _line.Quantity.ToString();
                AveragePriceTextBox.Text = (_line.AverageVal is not null) ? _line.AverageVal.ToString() : "";
                PrimaryButtonText = "Modifier";
                Title = "Modifier la ligne";
            }
            DatePicker.SelectedDate = _date;
        }
        #endregion

        #region eventHandlers
        private void ContentDialog_PrimaryButtonClick(ContentDialog _1, ContentDialogButtonClickEventArgs _2)
        {
            Fund fund = FundComboBox.SelectedItem as Fund;
            PortFolioLine line = new(id: _line.ID,
                fundId: fund.ID,
                quantity: _quantity ?? 1.0,
                averageVal: _averageValue,
                date: _date,
                portFolioID: _line.PortFolioID);
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
            CheckValues();
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
                _averageValue = double.Parse(AveragePriceTextBox.Text);
            }
            catch
            {
                _averageValue = null;
            }
            CheckValues();
        }
        #endregion

        #region private methods

        private void CheckValues()
        {
            IsPrimaryButtonEnabled = _quantity is not null && (_date is not null || _averageValue is not null) && FundComboBox.SelectedItem is not null;
        }
        #endregion
    }
}
