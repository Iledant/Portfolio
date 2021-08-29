using System;

namespace Portfolio.Models
{
    public class CashAccountLine
    {
        public readonly int ID;
        public readonly int CashAccountID;
        public readonly DateTime Date;
        public readonly double Value;
        public readonly int PortfolioID;
        public readonly string PortfolioName;

        public CashAccountLine(int id = 0, int cashAccountID = 0, DateTime? date = null, double value = 0, int portfolioID = 0, string portfolioName = "")
        {
            ID = id;
            CashAccountID = cashAccountID;
            Date = date ?? DateTime.Now;
            Value = value;
            PortfolioID = portfolioID;
            PortfolioName = portfolioName;
        }
    }
}
