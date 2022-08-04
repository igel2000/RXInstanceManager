using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace YamlHandlers
{
    public class YamlParser
    {
        private readonly YamlMappingNode rootNode;

        /// <summary>
        /// Получить корневую ноду.
        /// </summary>
        /// <param name="yamlContent">Содержимое yaml конфига.</param>
        public static YamlMappingNode GetRootNode(string yamlContent)
        {
            var yaml = new YamlStream();
            yaml.Load(new StringReader(yamlContent));

            return yaml.Documents.Single().RootNode as YamlMappingNode;
        }

        /// <summary>
        /// Выбрать yaml ноду по заданному https://github.com/json-path/JsonPath.
        /// </summary>
        /// <param name="path">JsonPath в виде строки.</param>
        public YamlNode SelectToken(string path)
        {
            var sections = new Queue<string>(new JsonSimplePath(path).Parts());
            if (!sections.Any())
                return null;

            var mappingNode = this.rootNode;
            while (sections.Count > 1)
            {
                mappingNode = TryGetValue(mappingNode, sections.Dequeue()) as YamlMappingNode;
                if (mappingNode == null)
                    return null;
            }

            return TryGetValue(mappingNode, sections.Dequeue());
        }

        /// <summary>
        /// Получить строковое значение якоря.
        /// </summary>
        /// <param name="node">yaml нода.</param>
        private static string GetAnchorValue(YamlNode node)
        {
            if (node == null)
                return null;

            if (node.Anchor.IsEmpty)
                return null;

            return node.Anchor.Value;
        }

        /// <summary>
        /// Получить дочернее значение yaml ноды по ключу.
        /// </summary>
        /// <param name="node">yaml нода.</param>
        /// <param name="key">ключ.</param>
        private static YamlNode TryGetValue(YamlNode node, string key)
        {
            if (node == null)
                return null;

            try
            {
                return node[key];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Получить merged содержимое ноды. Там, где объявляются алиасы.
        /// </summary>
        /// <param name="node">yaml нода.</param>
        private static YamlNode GetMergedNode(YamlNode node)
        {
            return TryGetValue(node as YamlMappingNode, "<<");
        }

        /// <summary>
        /// Получить список алиасов ноды.
        /// </summary>
        /// <param name="mergedNode">yaml нода.</param>
        private static IList<YamlMappingNode> GetAliasesNodes(YamlNode mergedNode)
        {
            if (mergedNode is YamlMappingNode mergedMappingNode)
                return new[] { mergedMappingNode };

            var result = (mergedNode as YamlSequenceNode)?.Select(n => (YamlMappingNode)n);
            return result?.ToList() ?? new List<YamlMappingNode>();
        }

        /// <summary>
        /// Получить якорь предка. Там, где объявлена нода в случае, если она пришла через merge.
        /// Якорь искомой ноды должен входить в список алиасов.
        /// </summary>
        /// <param name="node">yaml нода.</param>
        /// <param name="aliases">Список алиасов для целевой ноды.</param>
        /// <param name="propertyName">Имя свойства.</param>
        private static string GetAncestorAnchor(YamlMappingNode node, ICollection<string> aliases, string propertyName)
        {
            if (node == null)
                return null;

            foreach (var pair in node)
            {
                var key = pair.Key;
                var value = pair.Value;
                var valueAsMappingNode = value as YamlMappingNode;
                if (valueAsMappingNode == null)
                    continue;

                string anchor = GetAnchorValue(value);
                if (aliases.Contains(anchor) && GetDeclaredNodes(valueAsMappingNode).ContainsKey(propertyName))
                    return anchor;

                string result = GetAncestorAnchor(valueAsMappingNode, aliases, propertyName);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Получить словарь из списка ключей нод, обявленных в yaml конфиге
        /// где ключ - имя свойства ноды, значение - IsOwnKey
        /// значение - IsOwnKey, true - если ключ задан в ноде, false - если пришёл через merge.
        /// </summary>
        /// <param name="node">yaml нода.</param>
        public static IDictionary<string, bool> GetDeclaredNodes(YamlNode node)
        {
            var yamlMappingNode = node as YamlMappingNode;
            if (yamlMappingNode == null)
                return new Dictionary<string, bool>();

            var declaredNodes = new Dictionary<string, bool>();
            foreach (string mergedDeclaredNode in GetDeclaredNodes(GetMergedNode(yamlMappingNode)).Keys)
                declaredNodes[mergedDeclaredNode] = false;

            var explicityDeclaredNodes = yamlMappingNode.Select(n => ((YamlScalarNode)n.Key).Value)
              .Where(v => !string.IsNullOrEmpty(v) && v != "<<").ToList();
            foreach (string explicityDeclaredNode in explicityDeclaredNodes)
                declaredNodes[explicityDeclaredNode] = true;

            return declaredNodes;
        }

        /// <summary>
        ///  Список алиасов у merged ноды.
        /// </summary>
        /// <param name="yamlNode">yaml нода.</param>
        private static IList<string> GetAliasesFromMergedNode(YamlNode yamlNode)
        {
            return GetAliasesNodes(GetMergedNode(yamlNode)).Select(GetAnchorValue).Where(k => !string.IsNullOrEmpty(k)).ToList();
        }

        /// <summary>
        /// Выбрать yaml метаданные на основе https://github.com/json-path/JsonPath.
        /// </summary>
        /// <param name="path">JsonPath в виде строки.</param>
        public YamlMetadata SelectMetadata(string path)
        {
            var yamlNode = this.SelectToken(path);
            string anchor = GetAnchorValue(yamlNode);

            var jsonPath = new JsonSimplePath(path);
            if (!jsonPath.Parts().Any())
                return new YamlMetadata();

            string propertyName = jsonPath.Parts().Last();
            var parentMappingNode = this.SelectToken(jsonPath.Parent()) as YamlMappingNode;
            var isOwnKey = true;
            if (GetDeclaredNodes(parentMappingNode).TryGetValue(propertyName, out bool hasProperty))
                isOwnKey = hasProperty;

            var parentAliases = GetAliasesFromMergedNode(parentMappingNode);
            string ancestorNode = GetAncestorAnchor(this.rootNode, parentAliases, propertyName);

            var aliases = GetAliasesFromMergedNode(yamlNode);
            return new YamlMetadata(anchor, aliases, isOwnKey, ancestorNode);
        }

        /// <summary>
        /// Получить JsonCompatible строку из yaml конфига.
        /// </summary>
        /// <param name="yamlContent">yaml конфиг.</param>
        public static string ToJsonCompatibleString(string yamlContent)
        {
            var mergingParser = new MergingParser(new Parser(new StringReader(yamlContent)));
            var builder = new DeserializerBuilder()
              .WithNodeTypeResolver(new InferTypeFromValue());
            object yamlObject = builder.Build().Deserialize(mergingParser);

            var serializer = new SerializerBuilder()
              .JsonCompatible()
              .Build();
            return serializer.Serialize(yamlObject).Replace("\n", string.Empty);
        }

        public YamlParser(string yamlContent)
        {
            this.rootNode = GetRootNode(yamlContent);
            this.JsonNode = JsonNode.Parse(ToJsonCompatibleString(yamlContent));
        }

        public JsonNode JsonNode { get; }
    }
}
