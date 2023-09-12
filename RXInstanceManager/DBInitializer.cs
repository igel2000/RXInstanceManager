using System;
using SQLQueryGen;
using SQLQueryGen.Adapter;

namespace RXInstanceManager
{
  public static class DBInitializer
  {
    internal static readonly string DBFileName = "rxman.db";
    internal static readonly string YamlFileName = "rxman.yaml";

    public static string DBFilePath { get; }
    public static string YamlFilePath { get; }
    public static string ConnectionString { get; }
    public static SQLite Database { get; set; }
    public static Generator QueryGenerator { get; set; }

    public static void Initialize()
    {
      Database = new SQLite(ConnectionString, null);
      QueryGenerator = new Generator(Database);

    }

    static DBInitializer()
    {
      DBFilePath = $"{AppContext.BaseDirectory}{DBFileName}";
      YamlFilePath = $"{AppContext.BaseDirectory}{YamlFileName}";
      ConnectionString = $"Data Source={DBFilePath}";
    }
  }
}
