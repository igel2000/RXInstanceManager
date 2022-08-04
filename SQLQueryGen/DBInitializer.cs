using System;
using System.Data.SQLite;
using Dapper;

namespace SQLQueryGen
{
    public static class DBInitializer
    {
        internal static readonly string DBFileName = "rxman.db";

        public static string DBFilePath { get; }
        public static string ConnectionString { get; }

        public static void Initialize()
        {
            
        }

        static DBInitializer()
        {
            DBFilePath = $"{AppContext.BaseDirectory}{DBFileName}";
            ConnectionString = $"Data Source={DBFilePath}";
        }
    }
}
