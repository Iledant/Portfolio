#nullable enable

namespace Portfolio.Models
{
    public class Company
    {
        readonly public int ID;
        readonly public string Name;
        readonly public string? Comment;
        readonly public int? FundCount;

        public Company(int id = 0, string name = "", string? comment = null, int? fundCount = null)
        {
            ID = id;
            Name = name;
            Comment = comment;
            FundCount = fundCount;
        }
    }
}
