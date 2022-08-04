using System;
using System.Linq;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlHandlers
{
    public class InferTypeFromValue : INodeTypeResolver
    {
        private static readonly string[] yamlBool = { "true", "false", };

        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            var scalar = nodeEvent as Scalar;
            if (scalar == null)
                return false;

            if (scalar.IsQuotedImplicit)
                return false;

            if (yamlBool.Contains(scalar.Value, StringComparer.OrdinalIgnoreCase))
            {
                currentType = typeof(bool);
                return true;
            }

            if (int.TryParse(scalar.Value, out int _))
            {
                currentType = typeof(int);
                return true;
            }

            return false;
        }
    }
}
