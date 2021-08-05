#nullable enable

using Npgsql;
using Portfolio.Models;
using System.Collections.Generic;

namespace Portfolio.Repositories
{
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
                    lines: Repository.ReadNullableInt(reader,3)));
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

        public static DBState Insert(PortFolio portfolio)
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

        public static DBState Update(PortFolio portfolio)
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
                    return DBState.AlreadyExists;
                }
            }
            return DBState.OK;
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
    }
}
