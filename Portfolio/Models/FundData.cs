using System;

namespace Portfolio.Models
{
    public class FundData
    {
        public readonly int ID;
        public readonly int FundID;
        public readonly DateTime Date;
        public readonly double Val;

        public FundData(int id, int fundId, DateTime date, double val)
        {
            ID = id;
            FundID = fundId;
            Date = date;
            Val = val;
        }
    }
}
