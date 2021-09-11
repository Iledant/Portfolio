using Portfolio.Models;
using Portfolio.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Portfolio.ViewModel
{
    public class CashAccountMovement
    {
        private static readonly CultureInfo _ci = new("fr-FR");

        public string Amount;
        public int ID;
        public DateTime Date;
        public int? PortfolioLineID;
        public readonly double Value;

        public string DateString()
        {
            return Date.ToString("dd/MM/yy", _ci);
        }

        public string Type()
        {
            return PortfolioLineID is null ? Value >= 0 ? "Versement" : "Retrait" : "Virement";
        }

        public CashAccountMovement(CashAccountLine line)
        {
            ID = line.ID;
            Value = line.Value;
            Date = line.Date;
            PortfolioLineID = line.PortfolioLineID;
            Amount = Value.ToString("C", _ci);
        }

        public CashAccountLine ToCashAccountLine(int portfolioID)
        {
            return new CashAccountLine(id: ID, date: Date, value: Value, portfolioLineID: PortfolioLineID, portfolioID: portfolioID);
        }
    }


    public class CashAccountHistoryViewModel : Bindable
    {
        private List<CashAccountMovement> _movements = new();

        public List<CashAccountMovement> Movements
        {
            get => _movements;
            set
            {
                _movements = value;
                OnPropertyChanged(nameof(Movements));
            }
        }

        public void FetchMovements(int portfolioID)
        {
            List<CashAccountLine> lines = PortfolioRepository.GetCashAmountLines(portfolioID);
            List<CashAccountMovement> movements = new();
            foreach (CashAccountLine line in lines)
            {
                movements.Add(new(line));
            }
            Movements = movements;
        }
    }
}
