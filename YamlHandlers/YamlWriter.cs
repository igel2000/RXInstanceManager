using System;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlHandlers
{
    public static class YamlWriter
    {
        /// <summary>
        /// Сохранить изменения в yaml.
        /// </summary>
        /// <param name="node">Блок yaml содержащий переменные.</param>
        public static void Save(this YamlVariablesNode node)
        {
            FillFullVariablesNode(node);
            ReplaceVariablesNode(node);

            if (node.Content != node.ContentOriginal)
                node.FullContent = node.FullContent.Replace(node.ContentOriginal, node.Content);

            File.WriteAllText(node.Path, node.FullContent);
        }

        /// <summary>
        /// Получить блок yaml содержащий переменные.
        /// </summary>
        /// <param name="yamlContent">Содержимое yaml.</param>
        /// <returns>Блок yaml содержащий переменные.</returns>
        public static void ClearInstanceConfig(string configYamlPath)
        {
            var yaml = File.ReadAllText(configYamlPath);
            var yamlNode = YamlWriter.GetVariablesNode(yaml);
            yamlNode.Path = configYamlPath;
            yamlNode.Variables.Instance_name = string.Empty;
            yamlNode.Variables.Purpose = string.Empty;
            yamlNode.Variables.Database = string.Empty;
            yamlNode.Variables.Http_port = string.Empty;
            yamlNode.Variables.Home_path = string.Empty;
            yamlNode.Variables.Home_path_src = string.Empty;
            yamlNode.Save();
        }

        /// <summary>
        /// Получить блок yaml содержащий переменные.
        /// </summary>
        /// <param name="yamlContent">Содержимое yaml.</param>
        /// <returns>Блок yaml содержащий переменные.</returns>
        public static YamlVariablesNode GetVariablesNode(string yamlContent)
        {
            var yamlParser = new YamlParser(yamlContent);
            var protocolNode = yamlParser.SelectToken("$.variables.protocol");
            var variablesNode = yamlContent.Substring(0, protocolNode.End.Index);
            var deserializer = new DeserializerBuilder()
              .WithNamingConvention(UnderscoredNamingConvention.Instance)
              .Build();

            var node = deserializer.Deserialize<YamlVariablesNode>(variablesNode);
            node.Content = variablesNode;
            node.ContentOriginal = variablesNode;
            node.FullContent = yamlContent;

            return node;
        }

        /// <summary>
        /// Изменить явно заданные значения настроек на ссылки на переменные.
        /// </summary>
        /// <param name="configPath">Путь до yaml файла.</param>
        /// <param name="yaml">Содержимое yaml.</param>
        public static void RebuildVariablesLinks(string configPath, string yaml)
        {
            var yamlParser = new YamlParser(yaml);

            var instanceNameVariable = "{{ instance_name }}";
            var databaseNameVariable = "{{ database }}";
            var hostVariable = "{{ host_fqdn }}";
            var portVariable = "{{ http_port }}";
            var srcPathVariable = "{{ home_path_src }}";

            #region QUEUE_CONNECTION_STRING

            var queueConfigOriginal = yamlParser.SelectToken("$.common_config.QUEUE_CONNECTION_STRING").ToString();
            var exchangeParam = queueConfigOriginal.Split(';').FirstOrDefault(x => x.ToLower().Contains("exchange"));
            if (exchangeParam != null)
            {
                var exchangeParams = exchangeParam.Split('=');
                var exchangeParamVariable = $"{exchangeParams[0]}={instanceNameVariable}";
                if (exchangeParamVariable != exchangeParam)
                {
                    var queueConfigNew = queueConfigOriginal.Replace(exchangeParam, exchangeParamVariable);
                    yaml = yaml.Replace(queueConfigOriginal, queueConfigNew);
                }
            }

            #endregion

            #region CONNECTION_STRING

            var databaseConfigOriginal = yamlParser.SelectToken("$.common_config.CONNECTION_STRING").ToString();
            var databaseNameParam = databaseConfigOriginal.Split(';').FirstOrDefault(x => x.Contains("initial catalog"));
            if (databaseNameParam != null)
            {
                var databaseNameParams = databaseNameParam.Split('=');
                var databaseParamVariable = $"{databaseNameParams[0]}={databaseNameVariable}";
                if (databaseParamVariable != databaseNameParam)
                {
                    var databaseConfigNew = databaseConfigOriginal.Replace(databaseNameParam, databaseParamVariable);
                    yaml = yaml.Replace(databaseConfigOriginal, databaseConfigNew);
                }
            }

            #endregion

            #region HYPERLINK_SERVER

            var hyperlinkConfigOriginal = yamlParser.SelectToken("$.common_config.HYPERLINK_SERVER").ToString();
            if (!hyperlinkConfigOriginal.Contains(portVariable))
            {
                var hyperlinkConfigNew = hyperlinkConfigOriginal.Replace(hostVariable, $"{hostVariable}:{portVariable}");
                yaml = yaml.Replace(hyperlinkConfigOriginal, hyperlinkConfigNew);
            }

            #endregion

            #region GIT_ROOT_DIRECTORY

            var gitDirectoryConfigOriginal = yamlParser.SelectToken("$.services_config.DevelopmentStudio.GIT_ROOT_DIRECTORY").ToString();
            if (!gitDirectoryConfigOriginal.Contains(srcPathVariable))
                yaml = yaml.Replace(gitDirectoryConfigOriginal, $"{srcPathVariable}");

            #endregion

            File.WriteAllText(configPath, yaml);
        }

        /// <summary>
        /// Сформировать блок yaml содержащий переменные.
        /// </summary>
        /// <param name="node">Блок yaml содержащий переменные.</param>
        private static void FillFullVariablesNode(YamlVariablesNode node)
        {
            if (!node.Content.Contains(node.Variables.Instance_name_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Instance_name_field,
                                                                                                               node.Variables.Instance_name));

            if (!node.Content.Contains(node.Variables.Purpose_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Purpose_field,
                                                                                                               node.Variables.Purpose));

            if (!node.Content.Contains(node.Variables.Database_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Database_field,
                                                                                                               node.Variables.Database));

            if (!node.Content.Contains(node.Variables.Home_path_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Home_path_field,
                                                                                                               node.Variables.Home_path));

            if (!node.Content.Contains(node.Variables.Home_path_src_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Home_path_src_field,
                                                                                                               node.Variables.Home_path_src));

            if (!node.Content.Contains(node.Variables.Host_fqdn_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Host_fqdn_field,
                                                                                                               node.Variables.Host_fqdn));

            if (!node.Content.Contains(node.Variables.Http_port_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: {2}", Environment.NewLine,
                                                                                                             node.Variables.Http_port_field,
                                                                                                             node.Variables.Http_port));

            if (!node.Content.Contains(node.Variables.Https_port_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: {2}", Environment.NewLine,
                                                                                                             node.Variables.Https_port_field,
                                                                                                             node.Variables.Https_port));

            if (!node.Content.Contains(node.Variables.Protocol_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Protocol_field,
                                                                                                               node.Variables.Protocol));
        }

        /// <summary>
        /// Сформировать блок yaml содержащий переменные.
        /// </summary>
        /// <param name="node">Блок yaml содержащий переменные.</param>
        private static void CopyVariablesNode(YamlVariablesNode node)
        {
            if (!node.Content.Contains(node.Variables.Instance_name_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Instance_name_field,
                                                                                                               node.Variables.Instance_name));

            if (!node.Content.Contains(node.Variables.Purpose_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Purpose_field,
                                                                                                               node.Variables.Purpose));

            if (!node.Content.Contains(node.Variables.Database_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Database_field,
                                                                                                               node.Variables.Database));

            if (!node.Content.Contains(node.Variables.Home_path_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Home_path_field,
                                                                                                               node.Variables.Home_path));

            if (!node.Content.Contains(node.Variables.Home_path_src_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: '{2}'", Environment.NewLine,
                                                                                                               node.Variables.Home_path_src_field,
                                                                                                               node.Variables.Home_path_src));

            if (!node.Content.Contains(node.Variables.Http_port_field))
                node.Content = node.Content.Replace("variables:", string.Format("variables:{0}    {1}: {2}", Environment.NewLine,
                                                                                                             node.Variables.Http_port_field,
                                                                                                             node.Variables.Http_port));
        }

        /// <summary>
        /// Заменить значения переменных.
        /// </summary>
        /// <param name="node">Блок yaml содержащий переменные.</param>
        private static void ReplaceVariablesNode(YamlVariablesNode node)
        {
            if (node.Variables.Instance_name != node.Variables.Instance_name_old)
                node.Content = node.Content.Replace(string.Format("{0}: '{1}'", node.Variables.Instance_name_field, node.Variables.Instance_name_old),
                                                    string.Format("{0}: '{1}'", node.Variables.Instance_name_field, node.Variables.Instance_name));

            if (node.Variables.Purpose != node.Variables.Purpose_old)
                node.Content = node.Content.Replace(string.Format("{0}: '{1}'", node.Variables.Purpose_field, node.Variables.Purpose_old),
                                                    string.Format("{0}: '{1}'", node.Variables.Purpose_field, node.Variables.Purpose));

            if (node.Variables.Database != node.Variables.Database_old)
                node.Content = node.Content.Replace(string.Format("{0}: '{1}'", node.Variables.Database_field, node.Variables.Database_old),
                                                    string.Format("{0}: '{1}'", node.Variables.Database_field, node.Variables.Database));

            if (node.Variables.Home_path != node.Variables.Home_path_old)
                node.Content = node.Content.Replace(string.Format("{0}: '{1}'", node.Variables.Home_path_field, node.Variables.Home_path_old),
                                                    string.Format("{0}: '{1}'", node.Variables.Home_path_field, node.Variables.Home_path));

            if (node.Variables.Home_path_src != node.Variables.Home_path_src_old)
                node.Content = node.Content.Replace(string.Format("{0}: '{1}'", node.Variables.Home_path_src_field, node.Variables.Home_path_src_old),
                                                    string.Format("{0}: '{1}'", node.Variables.Home_path_src_field, node.Variables.Home_path_src));

            if (node.Variables.Host_fqdn != node.Variables.Host_fqdn_old)
                node.Content = node.Content.Replace(string.Format("{0}: '{1}'", node.Variables.Host_fqdn_field, node.Variables.Host_fqdn_old),
                                                    string.Format("{0}: '{1}'", node.Variables.Host_fqdn_field, node.Variables.Host_fqdn));

            if (node.Variables.Protocol != node.Variables.Protocol_old)
                node.Content = node.Content.Replace(string.Format("{0}: '{1}'", node.Variables.Protocol_field, node.Variables.Protocol_old),

                                                    string.Format("{0}: '{1}'", node.Variables.Protocol_field, node.Variables.Protocol));

            if (node.Variables.Http_port != node.Variables.Http_port_old)
            {
                var from = string.Format("{0}: {1}", node.Variables.Http_port_field, node.Variables.Http_port_old);
                var to = string.Format("{0}: {1}", node.Variables.Http_port_field, node.Variables.Http_port);
                if (!string.IsNullOrEmpty(node.Variables.Http_port) && !node.Content.Contains(to))
                    node.Content = node.Content.Replace(from, to);
            }

            if (node.Variables.Https_port != node.Variables.Https_port_old)
            {
                var from = string.Format("{0}: {1}", node.Variables.Https_port_field, node.Variables.Https_port_old);
                var to = string.Format("{0}: {1}", node.Variables.Https_port_field, node.Variables.Https_port);
                if (!string.IsNullOrEmpty(node.Variables.Https_port) && !node.Content.Contains(to))
                    node.Content = node.Content.Replace(from, to);
            }
        }
    }
}
