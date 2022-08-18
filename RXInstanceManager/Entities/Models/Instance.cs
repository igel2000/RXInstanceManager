using System;
using SQLQueryGen;

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

        [Field("port")]
        public int Port { get; set; }

        [Field("url", Size = 100)]
        public string URL { get; set; }

        [Field("dbname", Size = 100)]
        public string DBName { get; set; }

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

        [Field("config")]
        [Navigate(TableName = "config", FieldName = "id", Required = false)]
        public Config Config { get; set; }
    }
}
