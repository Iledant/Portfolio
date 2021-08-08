#nullable enable

using Npgsql;
using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using YahooFinanceApi;

namespace Portfolio.Repositories
{
    public static class FundRepository
    {
        public static List<Fund> Get(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT f.id,f.name,f.comment,f.isin,f.yahoo_code,c.id,c.name " +
                $"FROM fund f " +
                $"JOIN company c ON f.company_id=c.id " +
                $"WHERE f.name ILIKE $$%{pattern}%$$ OR f.isin ILIKE $$%{pattern}%$$ " +
                $"OR f.yahoo_code ILIKE $$%{pattern}%$$ OR c.name ILIKE $$%{pattern}%$$";
            using NpgsqlCommand? cmd = new(query, con);
            List<Fund> funds = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                funds.Add(new(
                    id: reader.GetInt32(0),
                    name: reader.GetString(1),
                    comment: Repository.ReadNullableString(reader, 2),
                    isin: Repository.ReadNullableString(reader, 3),
                    yahooCode: Repository.ReadNullableString(reader, 4),
                    companyId: reader.GetInt32(5),
                    companyName: reader.GetString(6)));
            }
            return funds;
        }

        public static List<Fund> GetSuggestions(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT f.id,f.name,f.comment,f.isin,f.yahoo_code,c.id,c.name " +
                $"FROM fund f " +
                $"JOIN company c ON f.company_id=c.id " +
                $"WHERE f.name ILIKE $$%{pattern}%$$ OR f.isin ILIKE $$%{pattern}%$$ " +
                $"OR f.yahoo_code ILIKE $$%{pattern}%$$ OR c.name ILIKE $$%{pattern}%$$ ORDER BY 2,7 LIMIT 8";
            using NpgsqlCommand? cmd = new(query, con);
            List<Fund> funds = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                funds.Add(new(
                    id: reader.GetInt32(0),
                    name: reader.GetString(1),
                    comment: Repository.ReadNullableString(reader, 2),
                    isin: Repository.ReadNullableString(reader, 3),
                    yahooCode: Repository.ReadNullableString(reader, 4),
                    companyId: reader.GetInt32(5),
                    companyName: reader.GetString(6)));
            }
            return funds;
        }

        public static DBState Insert(Fund fund)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "INSERT INTO fund (name,comment,isin,yahoo_code,company_id) VALUES(@name,@comment,@isin,@yahoo_code,@company_id);";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", fund.Name);
            _ = cmd.Parameters.AddWithValue("comment",
                Repository.ConvertNullableStringParam(fund.Comment));
            _ = cmd.Parameters.AddWithValue("isin",
                Repository.ConvertNullableStringParam(fund.ISIN));
            _ = cmd.Parameters.AddWithValue("yahoo_code",
                Repository.ConvertNullableStringParam(fund.YahooCode));
            _ = cmd.Parameters.AddWithValue("company_id", fund.CompanyID);
            try
            {
                _ = cmd.ExecuteNonQuery();
            }
            catch (PostgresException exception)
            {
                if (exception.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return DBState.AlreadyExists;
                }
            }
            return DBState.OK;
        }

        public static DBState Update(Fund fund)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "UPDATE fund SET name=@name,comment=@comment,isin=@isin," +
                "yahoo_code=@yahoo_code,company_id=@company_id WHERE id=@id;";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", fund.Name);
            _ = cmd.Parameters.AddWithValue("comment",
                Repository.ConvertNullableStringParam(fund.Comment));
            _ = cmd.Parameters.AddWithValue("isin",
                Repository.ConvertNullableStringParam(fund.ISIN));
            _ = cmd.Parameters.AddWithValue("yahoo_code",
                Repository.ConvertNullableStringParam(fund.YahooCode));
            _ = cmd.Parameters.AddWithValue("company_id", fund.CompanyID);
            _ = cmd.Parameters.AddWithValue("id", fund.ID);
            try
            {
                _ = cmd.ExecuteNonQuery();
            }
            catch (PostgresException exception)
            {
                if (exception.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return DBState.AlreadyExists;
                }
            }
            return DBState.OK;
        }

        public static void Delete(Fund fund)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"DELETE FROM fund WHERE id={fund.ID};";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.ExecuteNonQuery();
        }

        public static void AddHistorical(Fund fund, IReadOnlyList<Candle> historical)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string insertQuery = "INSERT INTO fund_data_import (date,val) " +
                "SELECT * FROM unnest(@d,@v) AS d";
            using NpgsqlCommand? importCmd = new(insertQuery, con);
            importCmd.Parameters.Add(new NpgsqlParameter<DateTime[]>("d",
                historical.Select(e => e.DateTime).ToArray()));
            importCmd.Parameters.Add(new NpgsqlParameter<double[]>("v",
                historical.Select(e => (double)e.Close).ToArray()));
            importCmd.ExecuteNonQuery();

            string updateQuery = "INSERT INTO fund_data (fund_id,date,val) " +
                "SELECT @id,date,val FROM fund_data_import " +
                "WHERE @id,date,val NOT IN " +
                "  (SELECT fund_id,date,val FROM fund_data) q";
            using NpgsqlCommand? insertCmd = new(updateQuery, con);
            insertCmd.Parameters.AddWithValue("id", fund.ID);
            insertCmd.ExecuteNonQuery();
        }

        public static List<FundData> GetFundDatas(int fundID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT id,date,val FROM fund_data WHERE fund_id=@fund_id";
            using NpgsqlCommand? cmd = new(query, con);
            cmd.Parameters.AddWithValue("fund_id", fundID);
            List<FundData> datas = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                datas.Add(new(
                    id: reader.GetInt32(0),
                    fundId: fundID,
                    date: reader.GetDateTime(1),
                    val: reader.GetDouble(2)));
            }
            return datas;
        }

        public static DBState AddQuote(Quote quote, int companyID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "INSERT INTO fund (name,comment,isin,yahoo_code,company_id) VALUES(@name,null,null,@yahoo_code,@company_id);";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", quote.Longname== "" ? quote.Shortname : quote.Longname);
            _ = cmd.Parameters.AddWithValue("yahoo_code",
                Repository.ConvertNullableStringParam(quote.Symbol));
            _ = cmd.Parameters.AddWithValue("company_id", companyID);
            try
            {
                _ = cmd.ExecuteNonQuery();
            }
            catch (PostgresException exception)
            {
                if (exception.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return DBState.AlreadyExists;
                }
            }
            return DBState.OK;
        }
    }
}

