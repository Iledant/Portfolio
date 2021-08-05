using System;

namespace Portfolio.Models
{
    public class FundDataImport
    {
        readonly public DateTime Date;
        readonly public double Val;

        public FundDataImport(DateTime date, double val)
        {
            Date = date;
            Val = val;
        }
    }
}
