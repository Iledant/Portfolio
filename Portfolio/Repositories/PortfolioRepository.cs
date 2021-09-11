#nullable enable

using Npgsql;
using Portfolio.Models;
using System;
using System.Collections.Generic;

namespace Portfolio.Repositories
{
    public class PortFolioLineValue
    {
        public int FundID;
        public string FundName;
        public double ActualValue;
        public double Quantity;
        public double AverageValue;
        public double Evolution;
        public double Gain;

        public PortFolioLineValue(int fundID = 0, string fundName = "", double actualValue = 0, double quantity = 0, double averageValue = 0)
        {
            FundID = fundID;
            FundName = fundName;
            ActualValue = actualValue;
            Quantity = quantity;
            AverageValue = averageValue;
            Evolution = averageValue != 0 ? actualValue / averageValue - 1.0 : 0;
            Gain = (ActualValue - AverageValue) * Quantity;
        }
    }

    public static class PortfolioRepository
    {
        public static List<PortFolio> Get(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "WITH lines AS (SELECT count(1),portfolio_id FROM portfolio_line GROUP BY 2) " +
                "SELECT p.id,p.name,p.comment,lines.count FROM portfolio p " +
                "LEFT JOIN lines ON p.id = lines.portfolio_id " +
                "WHERE name ILIKE '%' || unaccent(@pattern) || '%' " +
                "ORDER BY 2,1";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("pattern", pattern);
            List<PortFolio> portfolios = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                portfolios.Add(new(
                    id: reader.GetInt32(0),
                    name: reader.GetString(1),
                    comment: Repository.ReadNullableString(reader, 2),
                    lines: Repository.ReadNullableInt(reader, 3)));
            }
            return portfolios;
        }

        public static PortFolio GetByID(int id)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT id,name,comment FROM portfolio WHERE id=@id";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("id", id);
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            _ = reader.Read();
            return new(id: reader.GetInt32(0),
                name: reader.GetString(1),
                comment: Repository.ReadNullableString(reader, 2));
        }

        public static void Insert(PortFolio portfolio)
        {
            NpgsqlConnection? con = DB.GetConnection();
            try
            {
                string createPortFolioQry = "INSERT INTO portfolio (name,comment) VALUES(@name,@comment) RETURNING id;";
                int portfolioID;
                using (NpgsqlCommand? cmd = new(createPortFolioQry, con))
                {
                    _ = cmd.Parameters.AddWithValue("name", portfolio.Name);
                    _ = cmd.Parameters.AddWithValue("comment",
                        Repository.ConvertNullableStringParam(portfolio.Comment));
                    using NpgsqlDataReader? reader = cmd.ExecuteReader();
                    reader.Read();
                    portfolioID = reader.GetInt32(0);
                }

                string insertNullLineCashAccountQry = $"INSERT INTO cash_account_line (portfolio_id,date,val) VALUES({portfolioID},now(),0)";
                using NpgsqlCommand? insertNullLineCashAccountCmd = new(insertNullLineCashAccountQry, con);
                _ = insertNullLineCashAccountCmd.ExecuteNonQuery();
                DB.State = DBState.OK;
            }
            catch (PostgresException exception)
            {
                if (exception.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    DB.State = DBState.AlreadyExists;
                }
                DB.State = DBState.Error;
            }
        }

        public static void Update(PortFolio portfolio)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"UPDATE portfolio SET name=@name,comment=@comment WHERE id=@id;";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", portfolio.Name);
            _ = cmd.Parameters.AddWithValue("comment",
                Repository.ConvertNullableStringParam(portfolio.Comment));
            _ = cmd.Parameters.AddWithValue("id", portfolio.ID);
            try
            {
                _ = cmd.ExecuteNonQuery();
            }
            catch (PostgresException exception)
            {
                if (exception.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    DB.State = DBState.AlreadyExists;
                }
                DB.State = DBState.Error;
            }
        }

        public static void Delete(PortFolio portfolio)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"DELETE FROM portfolio WHERE id={portfolio.ID};";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.ExecuteNonQuery();
        }

        public static List<PortFolio> GetSuggestions(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT id,name FROM portfolio WHERE name ILIKE '%' || unaccent(@pattern) || '%' " +
                "ORDER BY 2 LIMIT 8";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("pattern", pattern);
            List<PortFolio> companies = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                companies.Add(new(
                    id: reader.GetInt32(0),
                    name: reader.GetString(1),
                    comment: null));
            }
            return companies;
        }

        public static List<PortFolioLineValue> GetActualValue(int portfolioID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            List<PortFolioLineValue> fundPerfs = new();

            string historicalQuery = "SELECT pl.fund_id,pl.date,pl.quantity,pl.purchase_val FROM portfolio_line pl " +
                "WHERE pl.portfolio_id=@portfolio_id ORDER BY 1,2";
            using (NpgsqlCommand? cmd = new(historicalQuery, con))
            {
                _ = cmd.Parameters.AddWithValue("portfolio_id", portfolioID);
                using NpgsqlDataReader? reader = cmd.ExecuteReader();
                PortFolioLineValue line = new();

                while (reader.Read())
                {
                    int fundId = reader.GetInt32(0);
                    DateTime date = reader.GetDateTime(1);
                    double quantity = reader.GetDouble(2);
                    double value = reader.GetDouble(3);
                    if (fundId != line.FundID)
                    {
                        if (line.FundID != 0)
                        {
                            fundPerfs.Add(line);
                        }
                        line = new(fundID: fundId, quantity: quantity, averageValue: value);
                    }
                    else
                    {
                        if (quantity > 0)
                        {
                            line.AverageValue = (quantity * value + line.AverageValue * line.Quantity) / (quantity + line.Quantity);
                        }
                        line.Quantity += quantity;
                    }
                }
                if (line.FundID != 0)
                {
                    fundPerfs.Add(line);
                }
            }

            string nameAndActualValueQuery = "SELECT DISTINCT f.id,f.name,fd.val FROM fund f " +
                "JOIN portfolio_line pf ON pf.fund_id = f.id " +
                "JOIN(SELECT fund_id, max(date) FROM fund_data GROUP BY 1 ORDER BY 1) av ON av.fund_id = f.id " +
                "JOIN fund_data fd ON av.max = fd.Date AND av.fund_id = fd.fund_id " +
                "WHERE pf.portfolio_id = @portfolio_id ORDER BY 1";
            using (NpgsqlCommand? cmd = new(nameAndActualValueQuery, con))
            {
                cmd.Parameters.AddWithValue("portfolio_id", portfolioID);
                using var reader = cmd.ExecuteReader();
                int i = 0;
                while (reader.Read())
                {
                    int fundId = reader.GetInt32(0);
                    PortFolioLineValue line = fundPerfs[i];
                    if (line.FundID == fundId)
                    {
                        line.FundName = reader.GetString(1);
                        line.ActualValue = reader.GetDouble(2);
                        line.Gain = line.Quantity * (line.ActualValue - line.AverageValue);
                        line.Evolution = line.ActualValue / line.AverageValue - 1;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    i++;
                }
            }
            return fundPerfs;
        }

        public static List<FundData> GetHistorical(int portfolioID, DateTime? begin = null, DateTime? end = null)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "WITH fond_list AS (SELECT id FROM portfolio_line WHERE portfolio_id=1), " +
                "date_fund_list AS (SELECT id, fund_id, quantity, date " +
                "   FROM portfolio_line WHERE date is NOT null and average_val is null), " +
                "avg_fund_list AS (SELECT id, fund_id, average_val " +
                "   FROM portfolio_line WHERE average_val is NOT null), " +
                "cal_avg_fund_list AS (SELECT pl.id,pl.fund_id,fd.val AS average_val " +
                "   FROM date_fund_list pl JOIN fund_data fd ON pl.fund_id = fd.fund_id AND pl.date = fd.date), " +
                $"date_limits AS (SELECT min(date) AS min,max(date) AS max " +
                $"  FROM fund_data WHERE fund_id IN (SELECT fund_id FROM portfolio WHERE id = {portfolioID})) " +
                "SELECT fd.date,sum(fd.val * pl.quantity) " +
                "FROM portfolio_line pl " +
                "JOIN fund f ON pl.fund_id = f.id " +
                "JOIN fund_data fd ON fd.fund_id = f.id " +
                "WHERE pl.portfolio_id = 1 AND fd.date >= (SELECT min FROM date_limits) " +
                "   AND fd.date <= (SELECT max FROM date_limits) AND fd.date >= @begin AND fd.date <= @end " +
                "GROUP BY 1 ORDER BY 1";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("begin", begin ?? DateTime.MinValue);
            _ = cmd.Parameters.AddWithValue("end", end ?? DateTime.MaxValue);
            List<FundData> lines = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(new(id: 0, fundId: 0, date: reader.GetDateTime(0), val: reader.GetDouble(1)));
            }
            return lines;
        }

        public static List<FundLookUp> GetFundLookUps(int portfolioID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            List<FundLookUp> funds = new();

            string query = "WITH funds AS (SELECT DISTINCT f.id,f.name FROM fund f " +
                "JOIN portfolio_line pl ON pl.fund_id = f.id " +
                $"WHERE pl.portfolio_id = {portfolioID}), " +
                "last_val AS(SELECT fund_id, max(date) FROM fund_data GROUP BY 1 ORDER BY 1), " +
                "fd_d7 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '7'), " +
                "fd_d8 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '8'), " +
                "fd_d9 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '9'), " +
                "fd_d31 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '31'), " +
                "fd_d32 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '32'), " +
                "fd_d33 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '33'), " +
                "fd_d365 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '365'), " +
                "fd_d366 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '366'), " +
                "fd_d367 AS(SELECT fund_id, val FROM fund_data WHERE date = current_date - integer '367') " +
                "SELECT f.id,f.name,fd.val,COALESCE(fd_d7.val, fd_d8.val, fd_d9.val)," +
                    "COALESCE(fd_d31.val, fd_d32.val, fd_d33.val)," +
                    "COALESCE(fd_d365.val, fd_d366.val, fd_d367.val) FROM funds f " +
                "JOIN last_val ON last_val.fund_id = f.id " +
                "JOIN fund_data fd ON fd.fund_id = last_val.fund_id AND fd.date = last_val.max " +
                "LEFT JOIN fd_d7 ON fd_d7.fund_id = f.id " +
                "LEFT JOIN fd_d8 ON fd_d8.fund_id = f.id " +
                "LEFT JOIN fd_d9 ON fd_d9.fund_id = f.id " +
                "LEFT JOIN fd_d31 ON fd_d31.fund_id = f.id " +
                "LEFT JOIN fd_d32 ON fd_d32.fund_id = f.id " +
                "LEFT JOIN fd_d33 ON fd_d33.fund_id = f.id " +
                "LEFT JOIN fd_d365 ON fd_d365.fund_id = f.id " +
                "LEFT JOIN fd_d366 ON fd_d366.fund_id = f.id " +
                "LEFT JOIN fd_d367 ON fd_d367.fund_id = f.id";
            using NpgsqlCommand? cmd = new(query, con);
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                funds.Add(new(fundID: reader.GetInt32(0),
                    fundName: reader.GetString(1),
                    actualValue: reader.GetDouble(2),
                    lastWeekValue: reader.GetDouble(3),
                    lastMonthValue: reader.GetDouble(4),
                    lastYearValue: reader.GetDouble(5)));
            }
            return funds;
        }

        public static List<MonetaryAccount> GetMonetaryAccounts(int portfolioID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string selectQry = "SELECT m.id,m.name,p.name FROM monetary_account m " +
                $"JOIN portfolio p ON m.portfolio_id=p.id WHERE p.id = {portfolioID}";
            using NpgsqlCommand? selectCmd = new(selectQry, con);
            using NpgsqlDataReader? reader = selectCmd.ExecuteReader();
            List<MonetaryAccount> accounts = new();
            while (reader.Read())
            {
                accounts.Add(new(id: reader.GetInt32(0), name: reader.GetString(1), portfolioID: portfolioID, portfolioName: reader.GetString(2)));
            }
            return accounts;
        }

        public static List<CashAccountLine> GetCashAmountLines(int portfolioID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string selectQry = $"SELECT id,date,val,portfolio_line_id FROM cash_account_line WHERE portfolio_id = {portfolioID} ORDER BY 2";
            using NpgsqlCommand? selectCmd = new(selectQry, con);
            using NpgsqlDataReader? reader = selectCmd.ExecuteReader();
            List<CashAccountLine> lines = new();
            while (reader.Read())
            {
                lines.Add(new(id: reader.GetInt32(0),
                    date: reader.GetDateTime(1),
                    portfolioID: portfolioID,
                    value: reader.GetDouble(2),
                    portfolioLineID: Repository.ReadNullableInt(reader, 3)));
            }
            return lines;
        }
    }
}
