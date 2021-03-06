#nullable enable

using System;

namespace Portfolio.Models
{
    public class PortFolioLine
    {
        public readonly int ID;
        public readonly int PortFolioID;
        public readonly DateTime? Date;
        public readonly int FundID;
        public readonly string FundName;
        public readonly int CompanyID;
        public readonly string CompanyName;
        public readonly double Quantity;
        public readonly double? PurchaseVal;
        public readonly int AccountID;

        public PortFolioLine(int id = 0,
            int portFolioID = 0,
            int fundId = 0,
            string fundName = "",
            int companyID = 0,
            string companyName = "",
            double quantity = 0,
            DateTime? date = null,
            double? purchaseVal = null,
            int accountID = 0)
        {
            ID = id;
            PortFolioID = portFolioID;
            FundID = fundId;
            FundName = fundName;
            CompanyID = companyID;
            CompanyName = companyName;
            Quantity = quantity;
            Date = date;
            PurchaseVal = purchaseVal;
            AccountID = accountID;
        }
    }
}
