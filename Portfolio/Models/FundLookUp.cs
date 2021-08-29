namespace Portfolio.Models
{
    public class FundLookUp
    {
        public readonly int FundID;
        public readonly string FundName;
        public readonly double ActualValue;
        public readonly double LastWeekValue;
        public readonly double WeekPerformance;
        public readonly double LastMonthValue;
        public readonly double MonthPerformance;
        public readonly double LastYearValue;
        public readonly double YearPerformance;

        public FundLookUp(int fundID = 0, string fundName = "", double actualValue = 0, double lastWeekValue = 0, double lastMonthValue = 0, double lastYearValue = 0)
        {
            FundID = fundID;
            FundName = fundName;
            ActualValue = actualValue;
            LastWeekValue = lastWeekValue;
            LastMonthValue = lastMonthValue;
            LastYearValue = lastYearValue;
            WeekPerformance = LastWeekValue != 0 ? ActualValue / LastWeekValue - 1 : 0;
            MonthPerformance = LastMonthValue != 0 ? ActualValue / LastMonthValue - 1 : 0;
            YearPerformance = LastYearValue != 0 ? ActualValue / LastYearValue - 1 : 0;
        }
    }
}
