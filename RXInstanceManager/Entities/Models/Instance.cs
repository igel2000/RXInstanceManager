using System;
using System.Text;
using SQLQueryGen;
using System.IO;

namespace RXInstanceManager
{

  [Table("instance")]
  public class Instance
  {
    [Field("id", Key = true)]
    public int Id { get; set; }

    [Field("code", Size = 10)]
    public string Code { get; set; }

    [Field("platform", Size = 20)]
    public string PlatformVersion { get; set; }

    [Field("sungero", Size = 20)]
    public string SolutionVersion { get; set; }

    [Field("name", Size = 100)]
    public string Name { get; set; }

    [Field("projectconfigpath", Size = 250)]
    public string ProjectConfigPath { get; set; }

    [Field("port")]
    public int Port { get; set; }

    [Field("url", Size = 100)]
    public string URL { get; set; }

    [Field("dbname", Size = 100)]
    public string DBName { get; set; }

    [Field("dbengine", Size = 20)]
    public string DBEngine { get; set; }

    [Field("serverdb", Size = 250)]
    public string ServerDB { get; set; }

    [Field("service", Size = 50)]
    public string ServiceName { get; set; }

    [Field("instance", Size = 100)]
    public string InstancePath { get; set; }

    [Field("storage", Size = 150)]
    public string StoragePath { get; set; }

    [Field("sources", Size = 150)]
    public string SourcesPath { get; set; }

    [Field("status", Size = 12)]
    public string Status { get; set; }

    [Field("configchanged")]
    public DateTime ConfigChanged { get; set; }

    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.AppendLine("Код:                " + Code ?? string.Empty);
      builder.AppendLine("URL:                " + URL ?? string.Empty);
      builder.AppendLine("Версия платформы:   " + PlatformVersion ?? string.Empty);
      builder.AppendLine("Версия решения:     " + SolutionVersion ?? string.Empty);
      builder.AppendLine("Путь до инстанса:   " + InstancePath ?? string.Empty);
      builder.AppendLine("Имя службы:         " + ServiceName ?? string.Empty);
      builder.AppendLine("SQL-движок:         " + DBEngine ?? string.Empty);
      builder.AppendLine("Сервер БД:          " + ServerDB ?? string.Empty);
      builder.AppendLine("");
      builder.AppendLine("");
      builder.AppendLine("Конфиг проекта: " + ProjectConfigPath ?? string.Empty);
      builder.AppendLine("");

      if (File.Exists(ProjectConfigPath))
      {
        var readText = File.ReadAllLines(ProjectConfigPath);
        foreach (var s in readText)
        {
          builder.AppendLine(s);
        }
      }
      return builder.ToString();
    }
  }
}
