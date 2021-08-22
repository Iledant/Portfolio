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
            if (averageValue != 0)
            {
                Evolution = actualValue / averageValue - 1.0;
            }
            else
            {
                Evolution = 0;
            }
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
            string query = $"INSERT INTO portfolio (name,comment) VALUES(@name,@comment);";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", portfolio.Name);
            _ = cmd.Parameters.AddWithValue("comment",
                Repository.ConvertNullableStringParam(portfolio.Comment));
            try
            {
                _ = cmd.ExecuteNonQuery();
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
                "JOIN(SELECT fund_id, max(date) FROM fund_data GROUP BY 1 ORDER BY 1) av ON av.fund_id = f.id "+
                "JOIN fund_data fd ON av.max = fd.Date AND av.fund_id = fd.fund_id "+
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
    }
}
