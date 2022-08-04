using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Dapper;
using SQLQueryGen;

namespace RXInstanceManager
{
    public static class Certificates
    {
        #region Save.

        public static void Save(this Certificate certificate)
        {
            if (certificate.Id > 0)
                Update(certificate);
            else
                Insert(certificate);
        }

        private static void Insert(Certificate certificate)
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                certificate.Id = connection.QuerySingle<int>(QueryGenerator.GenerateInsertQuery<Certificate>(certificate));
            }
        }

        private static void Update(Certificate certificate)
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                connection.Execute(QueryGenerator.GenerateUpdateQuery<Certificate>(certificate));
            }
        }

        #endregion

        #region Get.

        public static List<Certificate> Get()
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                return connection.Query<Certificate>(QueryGenerator.GenerateSelectQuery<Certificate>()).ToList();
            }
        }

        #endregion

        #region Delete.

        public static void Delete(Certificate certificate)
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                connection.Execute(QueryGenerator.GenerateDeleteQuery<Certificate>(certificate));
                certificate = null;
            }
        }

        #endregion

        #region Create.

        public static void Create()
        {
            using (var connection = new SQLiteConnection(DBInitializer.ConnectionString))
            {
                connection.Execute(QueryGenerator.GenerateCreateQuery<Certificate>());
            }
        }

        #endregion
    }
}
