#nullable enable

using Newtonsoft.Json;
using Npgsql;
using Portfolio.Models;
using Portfolio.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace Portfolio.Repositories
{
    public static class FundRepository
    {
        private readonly static HttpClient _client = new();
        private readonly static DateTime earliestMorningstarDate = new(1991,1,1);

        public static List<Fund> Get(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT f.id,f.name,f.comment,f.isin,f.yahoo_code,c.id,c.name,f.morningstar_id " +
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
                    companyName: reader.GetString(6),
                    morningstarID: Repository.ReadNullableString(reader, 7)));
            }
            return funds;
        }

        public static List<Fund> GetNotNullYahooCode()
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT f.id,f.name,f.comment,f.isin,f.yahoo_code,c.id,c.name,f.morningstar_id " +
                $"FROM fund f " +
                $"JOIN company c ON f.company_id=c.id " +
                $"WHERE yahoo_code IS NOT NULL AND morningstar_id IS NULL";
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
                    companyName: reader.GetString(6),
                    morningstarID: Repository.ReadNullableString(reader, 7)));
            }
            return funds;
        }

        public static List<Fund> GetSuggestions(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT f.id,f.name,f.comment,f.isin,f.yahoo_code,c.id,c.name,f.morningstar_id " +
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
                    companyName: reader.GetString(6),
                    morningstarID: Repository.ReadNullableString(reader, 7)));
            }
            return funds;
        }

        public static void Insert(Fund fund)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "INSERT INTO fund (name,comment,isin,yahoo_code,morningstar_id,company_id) " +
                "VALUES(@name,@comment,@isin,@yahoo_code,@morningstar_id,@company_id);";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", fund.Name);
            _ = cmd.Parameters.AddWithValue("comment",
                Repository.ConvertNullableStringParam(fund.Comment));
            _ = cmd.Parameters.AddWithValue("isin",
                Repository.ConvertNullableStringParam(fund.ISIN));
            _ = cmd.Parameters.AddWithValue("yahoo_code",
                Repository.ConvertNullableStringParam(fund.YahooCode));
            _ = cmd.Parameters.AddWithValue("morningstar_id",
                Repository.ConvertNullableStringParam(fund.MorningstarID));
            _ = cmd.Parameters.AddWithValue("company_id", fund.CompanyID);
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

        public static void Update(Fund fund)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = "UPDATE fund SET name=@name,comment=@comment,isin=@isin," +
                "yahoo_code=@yahoo_code,company_id=@company_id,morningstar_id=@morningstar_id WHERE id=@id;";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", fund.Name);
            _ = cmd.Parameters.AddWithValue("comment",
                Repository.ConvertNullableStringParam(fund.Comment));
            _ = cmd.Parameters.AddWithValue("isin",
                Repository.ConvertNullableStringParam(fund.ISIN));
            _ = cmd.Parameters.AddWithValue("yahoo_code",
                Repository.ConvertNullableStringParam(fund.YahooCode));
            _ = cmd.Parameters.AddWithValue("morningstar_id",
                Repository.ConvertNullableStringParam(fund.MorningstarID));
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
                    DB.State = DBState.AlreadyExists;
                }
                DB.State = DBState.Error;
            }
        }

        public static void Delete(Fund fund)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string deleteDataQuery = $"DELETE FROM fund_data WHERE fund_id={fund.ID}";
            using NpgsqlCommand? deleteDatacmd = new(deleteDataQuery, con);
            _ = deleteDatacmd.ExecuteNonQuery();

            string deleteFundQuery = $"DELETE FROM fund WHERE id={fund.ID};";
            using NpgsqlCommand? deleteFundcmd = new(deleteFundQuery, con);
            _ = deleteFundcmd.ExecuteNonQuery();
        }

        public static async Task UpdateHistorical(Fund fund)
        {
            NpgsqlConnection? con = DB.GetConnection();

            IReadOnlyList<Candle> historical = await Yahoo.GetHistoricalAsync(fund.YahooCode);

            using NpgsqlCommand? deleteCmd = new("DELETE FROM fund_data_import", con);
            _ = deleteCmd.ExecuteNonQuery();

            string insertQuery = "INSERT INTO fund_data_import (date,val) " +
                "SELECT * FROM unnest(@d,@v) AS d";
            using NpgsqlCommand? importCmd = new(insertQuery, con);
            _ = importCmd.Parameters.Add(new NpgsqlParameter<DateTime[]>("d",
                historical.Select(e => e.DateTime).ToArray()));
            _ = importCmd.Parameters.Add(new NpgsqlParameter<double[]>("v",
                historical.Select(e => (double)e.Close).ToArray()));
            _ = importCmd.ExecuteNonQuery();

            string updateQuery = "INSERT INTO fund_data (fund_id,date,val) " +
                "SELECT @id,date,val FROM fund_data_import " +
                "WHERE (@id,date,val) NOT IN " +
                "  (SELECT fund_id,date,val FROM fund_data)";
            using NpgsqlCommand? insertCmd = new(updateQuery, con);
            _ = insertCmd.Parameters.AddWithValue("id", fund.ID);
            _ = insertCmd.ExecuteNonQuery();
        }

        private static List<FundUpdateLine> GetNotNullMorningstarID(NpgsqlConnection? con)
        {
            string query = "SELECT f.id,f.morningstar_id,MIN(fd.date) FROM fund f " +
                "LEFT JOIN fund_data fd ON fd.fund_id = f.id " +
                "WHERE f.morningstar_id is not null " +
                "GROUP BY 1,2";
            using NpgsqlCommand? cmd = new(query, con);
            List<FundUpdateLine> lines = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(new(fundID: reader.GetInt32(0), morningstarID: reader.GetString(1), firstDate: Repository.ReadNullableDateTime(reader, 2)));
            }
            return lines;
        }

        private static async Task<List<ParsedHistoryDetail>?> GetMorningstarHistorical(string morningstarID, DateTime? begin = null, DateTime? end = null)
        {
            string endDate = (end ?? DateTime.Now).ToString("yyyy-MM-dd");
            string beginDate = (begin ?? new DateTime(1991, 11, 29)).ToString("yyyy-MM-dd");
            string url = $"https://tools.morningstar.fr/api/rest.svc/timeseries_price/ok91jeenoo?" +
                $"id={morningstarID}&currencyId=EUR&idtype=Morningstar&frequency=daily&" +
                $"startDate={beginDate}&endDate={endDate}&outputType=JSON";

            try
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();
                MorningStarPayloadRoot? root = JsonConvert.DeserializeObject<MorningStarPayloadRoot>(content);
                return root?.TimeSeries?.Security?[0].HistoryDetail?.ConvertAll<ParsedHistoryDetail>(e => new ParsedHistoryDetail(e));
            }
            catch (Exception e)
            {
                Log.AddLine(e.Message, LogState.Error);
                return null;
            }
        }

        public static List<FundData> GetFundDatas(int fundID, DateTime? since = null)
        {
            since ??= DateTime.MinValue;
            NpgsqlConnection? con = DB.GetConnection();
            string query = "SELECT id,date,val FROM fund_data WHERE fund_id=@fund_id AND date >= @since";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("fund_id", fundID);
            _ = cmd.Parameters.AddWithValue("since", since);
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

        public static (Fund?, DBState) AddQuote(Quote quote, int companyID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            int id;
            string query = "INSERT INTO fund (name,comment,isin,yahoo_code,company_id,morningstar_id) " +
                "VALUES(@name,null,null,@yahoo_code,@company_id,null) " +
                "RETURNING id;";
            string name = quote.Longname == "" ? quote.Shortname : quote.Longname;
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", name);
            _ = cmd.Parameters.AddWithValue("yahoo_code",
                Repository.ConvertNullableStringParam(quote.Symbol));
            _ = cmd.Parameters.AddWithValue("company_id", companyID);
            try
            {
                id = (int) cmd.ExecuteScalar();
            }
            catch (PostgresException exception)
            {
                return exception.SqlState == PostgresErrorCodes.UniqueViolation ?
                    ((Fund?, DBState))(null, DBState.AlreadyExists)
                    : ((Fund?, DBState))(null, DBState.Error);
            }
            Fund fund = new(id, name, companyID, yahooCode: quote.Symbol, morningstarID: null);
            return (fund, DBState.OK);
        }

        public static (Fund?, DBState) AddMorningstarFund(MorningstarResponseLine line, int companyID)
        {
            NpgsqlConnection? con = DB.GetConnection();
            int id;
            string upsertQuery = "INSERT INTO fund (name,morningstar_id,company_id) " +
                "VALUES(@name,@morningstar_id,@company_id) " +
                "ON CONFLICT(name) DO UPDATE SET morningstar_id=excluded.morningstar_id " +
                "RETURNING id";
            using NpgsqlCommand? upsertCmd = new(upsertQuery, con);
            _ = upsertCmd.Parameters.AddWithValue("name", line.Name);
            _ = upsertCmd.Parameters.AddWithValue("morningstar_id", line.MorningStarID);
            _ = upsertCmd.Parameters.AddWithValue("company_id", companyID);
            try
            {
                id = (int)upsertCmd.ExecuteScalar();
            }
            catch (PostgresException exception)
            {
                return exception.SqlState == PostgresErrorCodes.UniqueViolation ?
                    (null, DBState.AlreadyExists)
                    : (null, DBState.Error);
            }
            Fund fund = new(id, line.Name, companyID, morningstarID: line.MorningStarID);
            return (fund, DBState.OK);
        }

        public class FundUpdateLine
        {
            public int FundID;
            public string MorningstarID;
            public DateTime? FirstDate;

            public FundUpdateLine(int fundID, string morningstarID, DateTime? firstDate)
            {
                FundID = fundID;
                MorningstarID = morningstarID;
                FirstDate = firstDate;
            }
        }

        public static async Task UpdateMorningstarHistorical()
        {
            NpgsqlConnection? con = DB.GetConnection();
            List<FundUpdateLine> lines = GetNotNullMorningstarID(con);

            foreach (FundUpdateLine line in lines)
            {
                await UpdateMorningstarHistorical(line);
            }
        }

        public static async Task UpdateMorningstarHistorical(FundUpdateLine line)
        {
            NpgsqlConnection? con = DB.GetConnection();

            DateTime beginTime = line.FirstDate is null ? earliestMorningstarDate : (DateTime)line.FirstDate;
            List<ParsedHistoryDetail>? historical = await GetMorningstarHistorical(line.MorningstarID, beginTime);

            if (historical is null)
            {
                Log.AddLine($"Aucun historique pour l'ID morningstar {line.MorningstarID}");
                return;
            }

            using NpgsqlCommand? deleteCmd = new("DELETE FROM fund_data_import", con);
            _ = deleteCmd.ExecuteNonQuery();

            string insertQuery = "INSERT INTO fund_data_import (date,val) " +
                "SELECT * FROM unnest(@d,@v) AS d";
            using NpgsqlCommand? importCmd = new(insertQuery, con);
            _ = importCmd.Parameters.Add(new NpgsqlParameter<DateTime[]>("d",
                historical.Select(e => e.Date).ToArray()));
            _ = importCmd.Parameters.Add(new NpgsqlParameter<double[]>("v",
                historical.Select(e => e.Value).ToArray()));
            _ = importCmd.ExecuteNonQuery();

            string updateQuery = "INSERT INTO fund_data (fund_id,date,val) " +
                "SELECT @id,date,val FROM fund_data_import " +
                "WHERE (@id,date) NOT IN " +
                "  (SELECT fund_id,date FROM fund_data)";
            using NpgsqlCommand? insertCmd = new(updateQuery, con);
            _ = insertCmd.Parameters.AddWithValue("id", line.FundID);
            _ = insertCmd.ExecuteNonQuery();
        }
    }

    public class ParsedHistoryDetail
    {
        private readonly static NumberFormatInfo _numberFormat = new CultureInfo("en-US").NumberFormat;
        public readonly DateTime Date;
        public readonly double Value;

        public ParsedHistoryDetail(HistoryDetail h)
        {
            if (h.EndDate is null || h.EndDate.Length < 10)
            {
                throw new ArgumentException();
            }
            Date = new DateTime(int.Parse(h.EndDate.Substring(0, 4)),
                int.Parse(h.EndDate.Substring(5, 2)),
                int.Parse(h.EndDate.Substring(8, 2)));
            Value = double.Parse(h.Value, _numberFormat);
        }
    }

    public class HistoryDetail
    {
        public string? EndDate { get; set; }
        public string? Value { get; set; }
    }

    public class Security
    {
        public List<HistoryDetail>? HistoryDetail { get; set; }
        public string? Id { get; set; }
    }

    public class TimeSeries
    {
        public List<Security>? Security { get; set; }
    }

    public class MorningStarPayloadRoot
    {
        public TimeSeries? TimeSeries { get; set; }
    }
}
