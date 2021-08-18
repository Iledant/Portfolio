#nullable enable

using Npgsql;
using Portfolio.Models;
using System;
using System.Collections.Generic;

namespace Portfolio.Repositories
{
    public static class PortfolioLineRepository
    {
        public static List<PortFolioLine> Get(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT l.id,l.date,l.fund_id,f.name,l.quantity,l.average_val,l.portfolio_id,c.id,c.name " +
                "FROM portfolio_line l " +
                "JOIN fund f ON l.fund_id=f.id " +
                "JOIN company c ON f.company_id=c.id " +
                "WHERE f.name ILIKE '%' || unaccent(@pattern) || '%' " +
                "OR c.name ILIKE '%' || unaccent(@pattern) || '%' ORDER BY 1";
            using NpgsqlCommand? cmd = new(query, con);
            cmd.Parameters.AddWithValue("pattern", pattern);
            List<PortFolioLine> lines = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(
                    new(
                        id: reader.GetInt32(0),
                        portFolioID: reader.GetInt32(6),
                        date: reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                        fundId: reader.GetInt32(2),
                        fundName: reader.GetString(3),
                        quantity: reader.GetDouble(4),
                        averageVal: reader.IsDBNull(5) ? null : reader.GetDouble(5),
                        companyID: reader.GetInt32(7),
                        companyName: reader.GetString(8))
                    );
            }
            return lines;
        }

        public static List<PortFolioLine> GetFromPortFolio(PortFolio portfolio, string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT l.id,l.date,l.fund_id,f.name,l.quantity,l.average_val,l.portfolio_id,c.id,c.name " +
                "FROM portfolio_line l " +
                "JOIN fund f ON l.fund_id=f.id " +
                "JOIN company c ON f.company_id=c.id " +
                "JOIN portfolio p ON l.portfolio_id=p.id " +
                "WHERE f.name ILIKE '%' || unaccent(@pattern) || '%' " +
                $"OR c.name ILIKE '%' || unaccent(@pattern) || '%' AND p.id=@id ORDER BY 1";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("pattern", pattern);
            _ = cmd.Parameters.AddWithValue("id", portfolio.ID);
            List<PortFolioLine> lines = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(
                    new(
                        id: reader.GetInt32(0),
                        portFolioID: reader.GetInt32(6),
                        date: reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                        fundId: reader.GetInt32(2),
                        fundName: reader.GetString(3),
                        quantity: reader.GetDouble(4),
                        averageVal: reader.IsDBNull(5) ? null : reader.GetDouble(5),
                        companyID: reader.GetInt32(7),
                        companyName: reader.GetString(8))
                    );
            }
            return lines;
        }

        private static DBState CheckQuantity(NpgsqlConnection? con, PortFolioLine portfolioLine)
        {
            if (portfolioLine.Quantity < 0)
            {
                string checkQuery = "SELECT sum(quantity) FROM portfolio_line WHERE fund_id=@fund_id";
                using NpgsqlCommand? checkCmd = new(checkQuery, con);
                _ = checkCmd.Parameters.AddWithValue("fund_id", portfolioLine.FundID);
                try
                {
                    double quantity = -(double)checkCmd.ExecuteScalar();
                    if (quantity > portfolioLine.Quantity)
                    {
                        return DBState.InvalidQuantity;
                    }
                }
                catch (PostgresException exception)
                {
                    if (exception.SqlState != PostgresErrorCodes.NoData)
                    {
                        return DBState.Error;
                    }
                }
            }
            return DBState.OK;
        }

        public static void Insert(PortFolioLine portfolioLine)
        {
            NpgsqlConnection? con = DB.GetConnection();

            DBState checkState = CheckQuantity(con, portfolioLine);
            if (checkState != DBState.OK) {
                DB.State = checkState;
                return;
            }

            string insertQuery = $"INSERT INTO portfolio_line (date,fund_id,quantity,average_val) " +
                $"VALUES(@date,@fund_id,@quantity,@average_val);";
            using NpgsqlCommand? cmd = new(insertQuery, con);
            _ = cmd.Parameters.AddWithValue("date", portfolioLine.Date is null ? DBNull.Value : portfolioLine.Date);
            _ = cmd.Parameters.AddWithValue("fund_id", portfolioLine.FundID);
            _ = cmd.Parameters.AddWithValue("quantity", portfolioLine.Quantity);
            _ = cmd.Parameters.AddWithValue("average_val", portfolioLine.AverageVal is null ? DBNull.Value : portfolioLine.AverageVal);
            try
            {
                _ = cmd.ExecuteNonQuery();
                DB.State = DBState.OK;
            }
            catch (PostgresException exception)
            {
                DB.State = exception.SqlState switch
                {
                    PostgresErrorCodes.UniqueViolation => DBState.AlreadyExists,
                    PostgresErrorCodes.IntegrityConstraintViolation => DBState.NullQuantity,
                    _ => DBState.Error
                };
            }
        }

        public static void Update(PortFolioLine portfolioLine)
        {
            NpgsqlConnection? con = DB.GetConnection();

            DBState checkState = CheckQuantity(con, portfolioLine);
            if (checkState != DBState.OK)
            {
                DB.State = checkState;
                return;
            }

            string query = "UPDATE portfolio_line SET date=@date,fund_id=@fund_id,quantity=@quantity,average_val=@average_val " +
                "WHERE id=@id;";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("date", portfolioLine.Date is null ? DBNull.Value : portfolioLine.Date);
            _ = cmd.Parameters.AddWithValue("fund_id", portfolioLine.FundID);
            _ = cmd.Parameters.AddWithValue("quantity", portfolioLine.Quantity);
            _ = cmd.Parameters.AddWithValue("average_val", portfolioLine.AverageVal is null ? DBNull.Value : portfolioLine.AverageVal);
            _ = cmd.Parameters.AddWithValue("id", portfolioLine.ID);
            try
            {
                _ = cmd.ExecuteNonQuery();
                DB.State = DBState.OK;
            }
            catch (PostgresException exception)
            {
                DB.State = exception.SqlState switch
                {
                    PostgresErrorCodes.UniqueViolation => DBState.AlreadyExists,
                    PostgresErrorCodes.IntegrityConstraintViolation => DBState.NullQuantity,
                    _ => DBState.Error
                };
            }
        }

        public static void Delete(PortFolioLine portfolio)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"DELETE FROM portfolio_line WHERE id={portfolio.ID};";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.ExecuteNonQuery();
        }
    }
}
