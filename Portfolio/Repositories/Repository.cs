#nullable enable

using Npgsql;
using System;

namespace Portfolio.Repositories
{
    public static class Repository
    {
        public static object ConvertNullableStringParam(string? param)
        {
            return param is null ? DBNull.Value : param;
        }

        public static string? ReadNullableString(NpgsqlDataReader reader, int pos)
        {
            return reader.IsDBNull(pos) ? null : reader.GetString(pos);
        }

        public static int? ReadNullableInt(NpgsqlDataReader reader, int pos)
        {
            return reader.IsDBNull(pos) ? null : reader.GetInt32(pos);
        }
    }
}
