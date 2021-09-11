using System;

namespace Portfolio.Models
{
    public class CashAccountLine
    {
        public readonly int ID;
        public readonly DateTime Date;
        public readonly double Value;
        public readonly int PortfolioID;
        public readonly string PortfolioName;
        public readonly int? PortfolioLineID;

        public CashAccountLine(int id = 0, DateTime? date = null, double value = 0, int portfolioID = 0, string portfolioName = "", int? portfolioLineID = null)
        {
            ID = id;
            Date = date ?? DateTime.Now;
            Value = value;
            PortfolioID = portfolioID;
            PortfolioName = portfolioName;
            PortfolioLineID = portfolioLineID;
        }
    }
}
