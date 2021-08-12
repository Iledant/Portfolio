#nullable enable

using Npgsql;
using Portfolio.Models;
using System;
using System.Collections.Generic;

namespace Portfolio.Repositories
{
    public class PortFolioLineValue
    {
        public readonly int FundID;
        public readonly string FundName;
        public readonly double FundActualValue;
        public readonly double Quantity;
        public readonly double AverageValue;
        public readonly double Evolution;
        public readonly double Gain;

        public PortFolioLineValue(int fundID, string fundName, double fundActualValue, double quantity, double averageValue)
        {
            FundID = fundID;
            FundName = fundName;
            FundActualValue = fundActualValue;
            Quantity = quantity;
            AverageValue = averageValue;
            Evolution = fundActualValue / averageValue - 1.0;
            Gain = Evolution * Quantity;
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
            string query = "WITH fond_list as (select id from portfolio_line where portfolio_id=1), " +
                 "date_fund_list as (select id, fund_id, quantity, date from portfolio_line where date is not null and average_val is null), " +
                 "avg_fund_list as (select id, fund_id, average_val from portfolio_line where average_val is not null), " +
                 "cal_avg_fund_list as (select pl.id,pl.fund_id,fd.val as average_val FROM date_fund_list pl JOIN fund_data fd ON pl.fund_id = fd.fund_id AND pl.date = fd.date), " +
                 "all_values as (select* FROM cal_avg_fund_list UNION ALL select *FROM avg_fund_list), " +
                 "max_date as (select max(date) as date,fund_id FROM fund_data GROUP BY 2) " +
                 "SELECT f.id,f.name,fd.val,pl.quantity,av.average_val " +
                "FROM all_values av " +
                $"JOIN portfolio_line pl ON av.id = pl.id AND pl.portfolio_id = {portfolioID} " +
                "JOIN max_date md ON av.fund_id = md.fund_id " +
                "JOIN fund_data fd ON av.fund_id = fd.fund_id AND fd.date = md.date " +
                "JOIN fund f ON av.fund_id = f.id";
            using NpgsqlCommand? cmd = new(query, con);
            List<PortFolioLineValue> lines = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(new(
                    fundID: reader.GetInt32(0),
                    fundName: reader.GetString(1),
                    fundActualValue: reader.GetDouble(2),
                    quantity: reader.GetDouble(3),
                    averageValue: reader.GetDouble(3)));
            }
            return lines;
        }

        public static List<FundData> GetHistorical(int portfolioID, DateTime? begin = null, DateTime? end = null)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "WITH fond_list as (select id from portfolio_line where portfolio_id=1), " +
                "date_fund_list as (select id, fund_id, quantity, date from portfolio_line where date is not null and average_val is null), " +
                "avg_fund_list as (select id, fund_id, average_val from portfolio_line where average_val is not null), " +
                "cal_avg_fund_list as (select pl.id,pl.fund_id,fd.val as average_val FROM date_fund_list pl JOIN fund_data fd ON pl.fund_id = fd.fund_id AND pl.date = fd.date), " +
                $"date_limits as (select min(date) as min,max(date) as max FROM fund_data WHERE fund_id IN(SELECT fund_id FROM portfolio WHERE id = {portfolioID})) " +
                "SELECT fd.date,sum(fd.val * pl.quantity) " +
                "FROM portfolio_line pl " +
                "JOIN fund f ON pl.fund_id = f.id " +
                "JOIN fund_data fd ON fd.fund_id = f.id " +
                "WHERE pl.portfolio_id = 1 AND fd.date >= (select min from date_limits) AND fd.date <= (select max from date_limits) AND fd.date >= @begin AND fd.date <= @end " +
                "group by 1 order by 1";
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
