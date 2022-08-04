using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Dapper;
using SQLQueryGen;

namespace RXInstanceManager
{
    public static class Instances
    {
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
                instance.Id = connection.QuerySingle<int>(QueryGenerator.GenerateInsertQuery<Instance>(instance));
            }
        }

        private static void Update(Instance instance)
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                connection.Execute(QueryGenerator.GenerateUpdateQuery<Instance>(instance));
            }
        }

        #endregion

        #region Get.

        public static List<Instance> Get()
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                return connection.Query<Instance, Certificate, Config, Instance>(
                QueryGenerator.GenerateSelectQuery<Instance>(),
                (instance, linkedCertificate, linkedConfig) =>
                {
                    instance.Certificate = linkedCertificate;
                    instance.Config = linkedConfig;
                    return instance;
                },
                splitOn: "Certificate, Config").ToList();
            }
        }

        #endregion

        #region Delete.

        public static void Delete(Instance instance)
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                connection.Execute(QueryGenerator.GenerateDeleteQuery<Instance>(instance));
                instance = null;
            }
        }

        #endregion

        #region Create.

        public static void Create()
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                connection.Execute(QueryGenerator.GenerateCreateQuery<Instance>());
            }
        }

        #endregion
    }
}
