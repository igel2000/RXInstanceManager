using System;
using System.Collections.Generic;
using System.Text;

namespace SQLQueryGen
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NavigateAttribute : System.Attribute
    {
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public bool Required { get; set; }

        public NavigateAttribute()
        { }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : System.Attribute
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public bool Key { get; set; }

        public FieldAttribute()
        { }

        public FieldAttribute(string fieldName)
        {
            Name = fieldName;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : System.Attribute
    {
        public string Name { get; set; }

        public TableAttribute()
        { }

        public TableAttribute(string tableName)
        {
            Name = tableName;
        }
    }
}
