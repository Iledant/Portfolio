#nullable enable

using Npgsql;
using Portfolio.Models;
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

        public MonetaryAccount ToMonetaryAccount(PortFolio portFolio)
        {
            return new(id: ID, name: Name, portfolioID: portFolio.ID, portfolioName: portFolio.Name);
        }
    }

    public static class MonetaryAccountRepository
    {
        public static List<MonetaryAccountBalance> GetBalancesFromPortfolio(int portfolioId)
        {
            List<MonetaryAccountBalance> lines = new();
            NpgsqlConnection? con = DB.GetConnection();
            string getBalancesQry = "SELECT ma.id,ma.name,COALESCE(line.val,0) FROM monetary_account ma " +
                "LEFT JOIN (SELECT SUM(val) AS val, account_id FROM monetary_account_line " +
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

        public static void Insert(MonetaryAccount account)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string insertQry = "INSERT INTO monetary_account (name,portfolio_id) VALUES(@name,@portfolio_id)";
            using NpgsqlCommand? cmd = new(insertQry, con);
            _ = cmd.Parameters.AddWithValue("name", account.Name);
            _ = cmd.Parameters.AddWithValue("portfolio_id", account.PortfolioID);
            try
            {
                _ = cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                DB.State = DBState.Error;
            }
        }

        public static void Update(MonetaryAccount account)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string insertQry = $"UPDATE monetary_account SET name=@name,portfolio_id=@portfolio_id WHERE id={account.ID}";
            using NpgsqlCommand? cmd = new(insertQry, con);
            _ = cmd.Parameters.AddWithValue("name", account.Name);
            _ = cmd.Parameters.AddWithValue("portfolio_id", account.PortfolioID);
            try
            {
                _ = cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                DB.State = DBState.Error;
            }
        }

        public static void Delete(MonetaryAccount account)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string insertQry = $"DELETE FROM monetary_account WHERE id={account.ID}";
            using NpgsqlCommand? cmd = new(insertQry, con);
            try
            {
                _ = cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                DB.State = DBState.Error;
            }
        }
    }
}
