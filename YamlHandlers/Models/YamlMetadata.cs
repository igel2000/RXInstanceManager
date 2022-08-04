using System;
using System.Collections.Generic;

namespace YamlHandlers
{
    public class YamlMetadata
    {
        // Собственный ключ. (Значение явно задано в текущем словаре).
        public bool IsOwnKey { get; }

        // Откуда пришла настройка.
        public string AncestorAnchor { get; }

        // Имена используемых якорей. Обявляются через *. Например: <<: [*a1, *b1].
        public IList<string> Aliases { get; }

        // Имя якоря. Объявляется через &.Например: a1: &a1.
        public string Anchor { get; }

        public YamlMetadata(string anchor, IList<string> aliases, bool isOwnKey, string ancestorAnchor)
        {
            this.Anchor = anchor;
            this.Aliases = aliases ?? new List<string>();
            this.IsOwnKey = isOwnKey;
            this.AncestorAnchor = ancestorAnchor;
        }
        public YamlMetadata() :
          this(null, null, true, null)
        {
        }
    }
}
