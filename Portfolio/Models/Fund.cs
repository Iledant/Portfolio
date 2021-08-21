#nullable enable

namespace Portfolio.Models
{
    public class Fund
    {
        public readonly int ID;
        public readonly string Name;
        public readonly string? ISIN;
        public readonly string? YahooCode;
        public readonly string? Comment;
        public readonly int CompanyID;
        public readonly string CompanyName;
        public readonly string? MorningstarID;

        public Fund(int id = 0, string name = "", int companyId = 0, string companyName = "", string? morningstarID = null, string? isin = null, string? yahooCode = null, string? comment = null)
        {
            ID = id;
            Name = name;
            ISIN = isin;
            YahooCode = yahooCode;
            Comment = comment;
            CompanyID = companyId;
            CompanyName = companyName;
            MorningstarID = morningstarID;
        }
    }
}
