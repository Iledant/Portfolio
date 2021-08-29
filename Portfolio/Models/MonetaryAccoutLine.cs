using System;

namespace Portfolio.Models
{
    public class MonetaryAccoutLine
    {
        public readonly int ID;
        public readonly DateTime Date;
        public readonly double Value;
        public readonly int MonetaryAccountID;
        public readonly string MonetaryAccountName;

        public MonetaryAccoutLine(int id = 0, DateTime? date = null, double value = 0, int monetaryAccountID = 0, string monetaryAccountName = "")
        {
            ID = id;
            Date = date ?? DateTime.Now;
            Value = value;
            MonetaryAccountID = monetaryAccountID;
            MonetaryAccountName = monetaryAccountName;
        }
    }

}
