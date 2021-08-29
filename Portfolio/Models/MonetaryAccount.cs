namespace Portfolio.Models
{
    public class MonetaryAccount
    {
        public readonly int ID;
        public readonly string Name;
        public readonly int PortfolioID;
        public readonly string PortfolioName;

        public MonetaryAccount(int id = 0, string name = "", int portfolioID = 0, string portfolioName = "")
        {
            ID = id;
            Name = name;
            PortfolioID = portfolioID;
            PortfolioName = portfolioName;
        }

    }
}
