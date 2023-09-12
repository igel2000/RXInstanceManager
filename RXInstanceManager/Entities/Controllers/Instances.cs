using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Dapper;
using SQLQueryGen;
using System.IO;

namespace RXInstanceManager
{
  public static class Instances
  {

    public static void UpdateInstanceYaml()
    {
      List<string> instancesFolders = new List<string>();
      var instances = Get();
      foreach (var inst in instances)
        instancesFolders.Add(inst.InstancePath);
      var serializer = new YamlDotNet.Serialization.SerializerBuilder()
          .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
          .Build();
      var yaml = serializer.Serialize(instancesFolders);
      File.WriteAllText(DBInitializer.YamlFilePath, yaml);
    }
    #region Save.

    public static void Save(this Instance instance)
    {
      if (instance.Id > 0)
        Update(instance);
      else
        Insert(instance);
    }

    private static void Insert(Instance instance)
    {
      using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
      {
        instance.Id = connection.QuerySingle<int>(DBInitializer.QueryGenerator.GenerateInsertQuery<Instance>(instance));
      }
      //UpdateInstanceYaml();
    }

    private static void Update(Instance instance)
    {
      using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
      {
        connection.Execute(DBInitializer.QueryGenerator.GenerateUpdateQuery<Instance>(instance));
      }
    }

    #endregion

    #region Get.

    public static List<Instance> Get()
    {
      using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
      {
        return connection.Query<Instance>(
        DBInitializer.QueryGenerator.GenerateSelectQuery<Instance>()).ToList();
      }
    }

    #endregion

    #region Delete.

    public static void Delete(Instance instance)
    {
      using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
      {
        connection.Execute(DBInitializer.QueryGenerator.GenerateDeleteQuery<Instance>(instance));
        instance = null;
        UpdateInstanceYaml();
      }
    }

    #endregion

    #region Create.

    public static void Create()
    {
      using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
      {
        connection.Execute(DBInitializer.QueryGenerator.GenerateDropTableQuery<Instance>());
        connection.Execute(DBInitializer.QueryGenerator.GenerateCreateQuery<Instance>());
      }
    }

    #endregion
  }
}
