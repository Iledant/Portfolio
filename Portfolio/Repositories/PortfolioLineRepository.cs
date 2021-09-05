#nullable enable

using Npgsql;
using Portfolio.Models;
using System.Collections.Generic;

namespace Portfolio.Repositories
{
    public static class PortfolioLineRepository
    {
        public static List<PortFolioLine> Get(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT l.id,l.date,l.fund_id,f.name,l.quantity,l.purchase_val,l.portfolio_id,c.id,c.name,l.account_id " +
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
                        purchaseVal: reader.IsDBNull(5) ? null : reader.GetDouble(5),
                        companyID: reader.GetInt32(7),
                        companyName: reader.GetString(8),
                        accountID: reader.GetInt32(9))
                    );
            }
            return lines;
        }

        public static (List<PortFolioLine>, double) GetFromPortFolio(PortFolio portfolio, string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string getLinesQry = "SELECT l.id,l.date,l.fund_id,f.name,l.quantity,l.purchase_val,l.portfolio_id,c.id,c.name,l.account_id " +
                "FROM portfolio_line l " +
                "JOIN fund f ON l.fund_id=f.id " +
                "JOIN company c ON f.company_id=c.id " +
                "JOIN portfolio p ON l.portfolio_id=p.id " +
                "WHERE f.name ILIKE '%' || unaccent(@pattern) || '%' " +
                $"OR c.name ILIKE '%' || unaccent(@pattern) || '%' AND p.id=@id ORDER BY 1";
            List<PortFolioLine> lines = new();
            double cash = 0;
            using (NpgsqlCommand? cmd = new(getLinesQry, con))
            {
                _ = cmd.Parameters.AddWithValue("pattern", pattern);
                _ = cmd.Parameters.AddWithValue("id", portfolio.ID);
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
                            purchaseVal: reader.IsDBNull(5) ? null : reader.GetDouble(5),
                            companyID: reader.GetInt32(7),
                            companyName: reader.GetString(8),
                            accountID: reader.GetInt32(9))
                        );
                }
            }

            string getCashQry = $"SELECT sum(val) FROM cash_account_line WHERE portfolio_id={portfolio.ID}";
            using (NpgsqlCommand? cmd = new(getCashQry, con))
            {
                using NpgsqlDataReader? reader = cmd.ExecuteReader();
                _ = reader.Read();
                cash = reader.GetDouble(0);
            }
            return (lines, cash);
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

        private static void InsertAccountLine(PortFolioLine portfolioLine, int id)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string accountInsertQry = portfolioLine.AccountID == 0 ?
                $"INSERT INTO cash_account_line (date,val,portfolio_id,portfolio_line_id) VALUES (@date,{-portfolioLine.Quantity * portfolioLine.PurchaseVal},{portfolioLine.PortFolioID},@id)" :
                $"INSERT INTO monetary_accout_line (date,val,account_id,portfolio_line_id) VALUES (@date,{-portfolioLine.Quantity * portfolioLine.PurchaseVal},{portfolioLine.AccountID},@id)";
            using NpgsqlCommand? cmd = new(accountInsertQry, con);
            _ = cmd.Parameters.AddWithValue("date", portfolioLine.Date);
            _ = cmd.Parameters.AddWithValue("id", id);
            _ = cmd.ExecuteNonQuery();
        }

        public static void Insert(PortFolioLine portfolioLine)
        {
            NpgsqlConnection? con = DB.GetConnection();

            DBState checkState = CheckQuantity(con, portfolioLine);
            if (checkState != DBState.OK)
            {
                DB.State = checkState;
                return;
            }

            string portfolioLineInsertQry = "INSERT INTO portfolio_line (date,fund_id,quantity,purchase_val,account_id) " +
                "VALUES(@date,@fund_id,@quantity,@purchase_val,@account_id) RETURNING id;";

            int id;
            try
            {
                using (NpgsqlCommand? cmd = new(portfolioLineInsertQry, con))
                {
                    _ = cmd.Parameters.AddWithValue("date", portfolioLine.Date);
                    _ = cmd.Parameters.AddWithValue("fund_id", portfolioLine.FundID);
                    _ = cmd.Parameters.AddWithValue("quantity", portfolioLine.Quantity);
                    _ = cmd.Parameters.AddWithValue("purchase_val", portfolioLine.PurchaseVal);
                    _ = cmd.Parameters.AddWithValue("account_id", portfolioLine.AccountID);
                    id = (int)cmd.ExecuteScalar();
                }

                InsertAccountLine(portfolioLine, id);

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

            string query = "UPDATE portfolio_line SET date=@date,fund_id=@fund_id,quantity=@quantity,purchase_val=@purchase_val,account_id=@account_id " +
                "WHERE id=@id;";
            try
            {
                using (NpgsqlCommand? cmd = new(query, con))
                {
                    _ = cmd.Parameters.AddWithValue("date", portfolioLine.Date);
                    _ = cmd.Parameters.AddWithValue("fund_id", portfolioLine.FundID);
                    _ = cmd.Parameters.AddWithValue("quantity", portfolioLine.Quantity);
                    _ = cmd.Parameters.AddWithValue("purchase_val", portfolioLine.PurchaseVal);
                    _ = cmd.Parameters.AddWithValue("id", portfolioLine.ID);
                    _ = cmd.Parameters.AddWithValue("account_id", portfolioLine.AccountID);
                    _ = cmd.ExecuteNonQuery();
                }

                string deleteQuery = $"DELETE FROM cash_account_line WHERE portfolio_line_id = {portfolioLine.ID}";
                using (NpgsqlCommand? cmd = new(deleteQuery, con))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                deleteQuery = $"DELETE FROM monetary_account_line WHERE portfolio_line_id = {portfolioLine.ID}";
                using (NpgsqlCommand? cmd = new(deleteQuery, con))
                {
                    _ = cmd.ExecuteNonQuery();
                }

                InsertAccountLine(portfolioLine, portfolioLine.ID);

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

        public static void Delete(PortFolioLine line)
        {
            NpgsqlConnection? con = DB.GetConnection();

            string portfolioDeleteQry = $"DELETE FROM portfolio_line WHERE id={line.ID};";
            using NpgsqlCommand? cmd = new(portfolioDeleteQry, con);
            _ = cmd.ExecuteNonQuery();
        }
    }
}
