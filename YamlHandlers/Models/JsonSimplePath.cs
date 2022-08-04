using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlHandlers
{
    public class JsonSimplePath
    {
        private readonly string rawPath;

        public IReadOnlyList<string> Parts()
        {
            if (string.IsNullOrEmpty(this.rawPath) || !this.rawPath.StartsWith("$."))
                return new List<string>();

            var sym = new char[1];
            sym[0] = '.';
            return this.rawPath.Split(sym, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();
        }

        public string Parent()
        {
            var idx = this.Parts().Count - 1;
            return $"$.{string.Join(".", this.Parts().Take(idx))}";
        }

        public string Path()
        {
            return $"$.{string.Join(".", this.Parts())}";
        }

        public JsonSimplePath(string rawPath)
        {
            this.rawPath = rawPath;
        }
    }
}
