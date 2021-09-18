#nullable enable

using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Repositories
{
    public class MonetaryAccountBalance
    {
        public int ID;
        public string Name;
        public double Balance;

        public MonetaryAccountBalance(int id = 0, string name = "", double balance = 0)
        {
            ID = id;
            Name = name;
            Balance = balance;
        }
    }

    public static class MonetaryAccountRepository
    {
        public static List<MonetaryAccountBalance> GetBalancesFromPortfolio(int portfolioId)
        {
            List<MonetaryAccountBalance> lines = new();
            NpgsqlConnection? con = DB.GetConnection();
            string getBalancesQry = "SELECT ma.id,ma.name,line.val FROM monetary_account ma " +
                "JOIN (SELECT SUM(val) AS val, account_id FROM monetary_account_line " +
                "   GROUP BY 2 ORDER BY 2) line ON line.account_id = ma.id " +
                $"WHERE ma.portfolio_id = {portfolioId};";
            using NpgsqlCommand? cmd = new(getBalancesQry, con);
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(new(id: reader.GetInt32(0), name: reader.GetString(1), balance: reader.GetDouble(2)));
            }
            return lines;
        }
    }
}
