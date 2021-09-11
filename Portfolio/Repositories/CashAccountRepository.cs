#nullable enable

using Npgsql;
using Portfolio.Models;
using System;

namespace Portfolio.Repositories
{
    public static class CashAccountRepository
    {
        public static void Add(CashAccountLine line)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "INSERT INTO cash_account_line (portfolio_id,val,date,portfolio_line_id) " +
                "VALUES(@portfolio_id,@val,@date,@portfolio_line_id);";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("portfolio_id", line.PortfolioID);
            _ = cmd.Parameters.AddWithValue("portfolio_line_id", Repository.ConvertNullableIntParam(line.PortfolioLineID));
            _ = cmd.Parameters.AddWithValue("val", line.Value);
            _ = cmd.Parameters.AddWithValue("date", line.Date);
            try
            {
                _ = cmd.ExecuteNonQuery();
                DB.State = DBState.OK;
            }
            catch (Exception e)
            {
                DB.State = DBState.Error;
                Log.AddLine(e.Message);
            }
        }

        public static void Update(CashAccountLine line)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "UPDATE cash_account_line SET portfolio_id=@portfolio_id,val=@val,date=@date," +
                "portfolio_line_id=@portfolio_line_id WHERE id=@id;";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("portfolio_id", line.PortfolioID);
            _ = cmd.Parameters.AddWithValue("portfolio_line_id", Repository.ConvertNullableIntParam(line.PortfolioLineID));
            _ = cmd.Parameters.AddWithValue("val", line.Value);
            _ = cmd.Parameters.AddWithValue("date", line.Date);
            _ = cmd.Parameters.AddWithValue("id", line.ID);
            try
            {
                _ = cmd.ExecuteNonQuery();
                DB.State = DBState.OK;
            }
            catch (Exception)
            {
                DB.State = DBState.Error;
            }
        }

        public static void Delete(int ID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"DELETE FROM cash_account_line WHERE id={ID};";
            using NpgsqlCommand? cmd = new(query, con);
            try
            {
                _ = cmd.ExecuteNonQuery();
                DB.State = DBState.OK;
            }
            catch (Exception)
            {
                DB.State = DBState.Error;
            }
        }
    }
}
