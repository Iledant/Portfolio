#nullable enable

using Npgsql;
using Portfolio.Models;
using System;
using System.Collections.Generic;

namespace Portfolio.Repositories
{
    public static class CompanyRepository
    {
        public static List<Company> Get(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"WITH fund_count AS (SELECT count(id),company_id FROM fund GROUP BY 2) " +
                $"SELECT c.id,c.name,c.comment,fund_count.count " +
                $"FROM company c " +
                $"LEFT JOIN fund_count ON fund_count.company_id = c.id " +
                $"WHERE c.name ILIKE $$%{pattern}%$$ " +
                $"ORDER BY 2,1";
            using NpgsqlCommand? cmd = new(query, con);
            List<Company> companies = new();
            using NpgsqlDataReader? reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                companies.Add(new(
                    id: reader.GetInt32(0),
                    name: reader.GetString(1),
                    comment: Repository.ReadNullableString(reader, 2),
                    fundCount: Repository.ReadNullableInt(reader, 3)));
            }
            return companies;
        }

        public static void Insert(Company company)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"INSERT INTO company (name,comment) VALUES(@name,@comment);";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", company.Name);
            _ = cmd.Parameters.AddWithValue("comment",
                Repository.ConvertNullableStringParam(company.Comment));
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
            }
            catch (Exception)
            {
                DB.State = DBState.Error;
            }
        }

        public static DBState Update(Company company)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"UPDATE company SET name=@name,comment=@comment WHERE id=@id;";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.Parameters.AddWithValue("name", company.Name);
            _ = cmd.Parameters.AddWithValue("comment",
                Repository.ConvertNullableStringParam(company.Comment));
            _ = cmd.Parameters.AddWithValue("id", company.ID);
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

        public static void Delete(Company company)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"DELETE FROM company WHERE id={company.ID};";
            using NpgsqlCommand? cmd = new(query, con);
            _ = cmd.ExecuteNonQuery();
        }

        public static List<Company> GetSuggestions(string pattern)
        {
            NpgsqlConnection? con = DB.GetConnection();
            string query = $"SELECT id,name FROM company WHERE name ILIKE $$%{pattern}%$$ " +
                $"ORDER BY 2 LIMIT 8";
            using NpgsqlCommand? cmd = new(query, con);
            List<Company> companies = new();
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
