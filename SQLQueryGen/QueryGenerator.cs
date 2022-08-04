using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SQLQueryGen
{
    public static class QueryGenerator
    {
        #region GenerateCreate.

        public static string GenerateCreateQuery<T>()
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var keyField = string.Empty;
            var fieldElements = new List<string>();
            var valueElements = new List<string>();

            var properties = type.GetProperties().Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute));
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
                var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

                if (fieldAttribute == null)
                    continue;

                if (fieldAttribute.Key)
                {
                    fieldElements.Add($"{fieldAttribute.Name} INTEGER PRIMARY KEY NOT NULL,");
                    continue;
                }

                if (navigateAttribute != null)
                {

                    if (navigateAttribute.Required)
                        fieldElements.Add($"\"{fieldAttribute.Name}\" INTEGER NOT NULL,");
                    else
                        fieldElements.Add($"\"{fieldAttribute.Name}\" INTEGER NULL,");
                    continue;
                }

                var fieldType = GetPGFieldType(property.PropertyType);
                if (fieldAttribute.Size > 0)
                    fieldType = $"{fieldType}({fieldAttribute.Size})";

                fieldElements.Add($"\"{fieldAttribute.Name}\" {fieldType} NULL,");
            }

            var lastFieldElement = fieldElements.Last();
            fieldElements[fieldElements.Count - 1] = lastFieldElement.Substring(0, lastFieldElement.Length - 1);

            var queryElements = new StringBuilder();
            queryElements.AppendLine($"CREATE TABLE IF NOT EXISTS {mainTable}");
            queryElements.AppendLine("(");
            queryElements.AppendLine(string.Join(Environment.NewLine, fieldElements));
            queryElements.AppendLine(")");

            return queryElements.ToString();
        }

        #endregion

        #region GenerateInsert.

        public static string GenerateInsertQuery<T>(T entity)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var keyField = string.Empty;
            var fieldElements = new List<string>();
            var valueElements = new List<string>();

            var properties = type.GetProperties().Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute));
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
                var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

                if (fieldAttribute == null)
                    continue;

                if (fieldAttribute.Key)
                {
                    keyField = fieldAttribute.Name;
                    continue;
                }

                var value = property.GetValue(entity);
                if (value == null)
                    continue;

                if (navigateAttribute != null)
                {
                    var navigateProperty = value.GetType().GetProperties()
                      .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute && (xx as FieldAttribute).Name == navigateAttribute.FieldName))
                      .FirstOrDefault();
                    var navigatePropertyValue = navigateProperty.GetValue(value);
                    if (navigatePropertyValue == null)
                        continue;

                    value = navigatePropertyValue;
                }

                fieldElements.Add($"\"{fieldAttribute.Name}\",");
                valueElements.Add(GetFieldValue(property.PropertyType, value) + ",");
            }

            var lastFieldElement = fieldElements.Last();
            fieldElements[fieldElements.Count - 1] = lastFieldElement.Substring(0, lastFieldElement.Length - 1);

            var lastValueElement = valueElements.Last();
            valueElements[valueElements.Count - 1] = lastValueElement.Substring(0, lastValueElement.Length - 1);

            var queryElements = new StringBuilder();
            queryElements.AppendLine($"INSERT INTO {mainTable}");
            queryElements.AppendLine("(");
            queryElements.AppendLine(string.Join(Environment.NewLine, fieldElements));
            queryElements.AppendLine(")");
            queryElements.AppendLine("VALUES");
            queryElements.AppendLine("(");
            queryElements.AppendLine(string.Join(Environment.NewLine, valueElements));
            queryElements.AppendLine(")");

            if (!string.IsNullOrEmpty(keyField))
                queryElements.AppendLine($"RETURNING {keyField}");

            return queryElements.ToString();
        }

        #endregion

        #region GenerateUpdate.

        public static string GenerateUpdateQuery<T>(T entity)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var keyField = string.Empty;
            var keyValue = string.Empty;
            var updateElements = new List<string>();

            var properties = type.GetProperties().Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute));
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
                var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

                if (fieldAttribute == null)
                    continue;

                if (fieldAttribute.Key)
                {
                    keyField = fieldAttribute.Name;
                    var propValue = property.GetValue(entity);
                    var isKeyNumeric = property.PropertyType == typeof(int) || property.PropertyType == typeof(decimal) || property.PropertyType == typeof(double);
                    keyValue = string.Format(isKeyNumeric ? "{0}" : "'{0}'", isKeyNumeric ? propValue.ToString().Replace(",", ".") : propValue.ToString());
                    continue;
                }

                var value = property.GetValue(entity);
                if (value == null)
                    continue;

                if (navigateAttribute != null)
                {
                    var navigateProperty = value.GetType().GetProperties()
                      .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute && (xx as FieldAttribute).Name == navigateAttribute.FieldName))
                      .FirstOrDefault();
                    var navigatePropertyValue = navigateProperty.GetValue(value);
                    if (navigatePropertyValue == null)
                        continue;

                    value = navigatePropertyValue;
                }

                var updateValue = GetFieldValue(property.PropertyType, value);
                updateElements.Add($"{fieldAttribute.Name} = {updateValue},");
            }

            var lastUpdateElement = updateElements.Last();
            updateElements[updateElements.Count - 1] = lastUpdateElement.Substring(0, lastUpdateElement.Length - 1);

            var queryElements = new StringBuilder();
            queryElements.AppendLine($"UPDATE {mainTable}");
            queryElements.AppendLine("SET");
            queryElements.AppendLine(string.Join(Environment.NewLine, updateElements));
            queryElements.AppendLine("WHERE");
            queryElements.AppendLine($"{keyField} = {keyValue}");

            return queryElements.ToString();
        }

        #endregion

        #region GenerateDelete.

        public static string GenerateDeleteQuery<T>()
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            return $"DELETE FROM {mainTable} {mainAlias}";
        }

        public static string GenerateDeleteQuery<T>(AddWhere<T> addWhere)
        {
            var query = GenerateDeleteQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            whereElements.Add(addWhere.Result);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateDeleteQuery<T>(List<AddWhere<T>> addWhereList, string addWhereCondition)
        {
            var query = GenerateDeleteQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            foreach (var addWhere in addWhereList)
            {
                whereElements.Add(addWhere.Result);
                whereElements.Add(addWhereCondition);
            }
            whereElements.RemoveAt(whereElements.Count - 1);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateDeleteQuery<T>(T entity)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;

            var property = type.GetProperties()
              .Where(x => x.GetCustomAttributes()
                .Where(xx => xx is FieldAttribute)
                .Cast<FieldAttribute>()
                .Any(xxx => xxx.Key)).FirstOrDefault();

            if (property == null)
                throw new Exception("Удаление возможно только при наличии ключевого свойства.");

            var value = property.GetValue(entity);
            if (value == null)
                throw new Exception("Удаление возможно только при наличии значения ключевого свойства.");

            var keyField = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault().Name;
            var keyValue = GetFieldValue(property.PropertyType, value);

            var queryElements = new StringBuilder();
            queryElements.AppendLine($"DELETE FROM {mainTable}");
            queryElements.AppendLine("WHERE");
            queryElements.AppendLine($"{keyField} = {keyValue}");

            return queryElements.ToString();
        }

        #endregion

        #region GenerateSelect.

        public static string GenerateSelectQuery<T>(AddWhere<T> addWhere, AddOrder<T> addOrder, string direction)
        {
            var query = GenerateSelectQuery<T>(addWhere);

            var whereElements = new List<string>();
            whereElements.Add("ORDER BY");
            whereElements.Add(addOrder.Result);
            whereElements.Add(direction);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateSelectQuery<T>(AddOrder<T> addOrder, string direction)
        {
            var query = GenerateSelectQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("ORDER BY");
            whereElements.Add(addOrder.Result);
            whereElements.Add(direction);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateSelectQuery<T>(AddWhere<T> addWhere)
        {
            var query = GenerateSelectQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            whereElements.Add(addWhere.Result);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateSelectQuery<T>(List<AddWhere<T>> addWhereList, string addWhereCondition)
        {
            var query = GenerateSelectQuery<T>();

            var whereElements = new List<string>();
            whereElements.Add("WHERE");
            foreach (var addWhere in addWhereList)
            {
                whereElements.Add(addWhere.Result);
                whereElements.Add(addWhereCondition);
            }
            whereElements.RemoveAt(whereElements.Count - 1);

            return query + Environment.NewLine + string.Join(Environment.NewLine, whereElements);
        }

        public static string GenerateSelectQuery<T>()
        {
            var selectElementTemplate = "{0}.{1} as {2},";
            var fromElementTemplate = "FROM {0} {1}";
            var joinElementTemplate = @"{0} {1} {2} ON {2}.{3} = {4}.{5}";

            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();
            var fromElement = string.Format(fromElementTemplate, mainTable, mainAlias);

            var selectElements = GetSelectElements(type);
            var joinElements = new List<string>();

            var properties = type.GetProperties()
              .Where(x => x.GetCustomAttributes().Any(xx => xx is NavigateAttribute));
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();
                var navigateAttribute = attributes.Where(x => x is NavigateAttribute).Cast<NavigateAttribute>().FirstOrDefault();

                if (fieldAttribute == null)
                    continue;

                var selectElement = string.Format(selectElementTemplate, mainAlias, fieldAttribute.Name, property.Name);
                selectElements.Add(selectElement);

                if (navigateAttribute != null)
                {
                    var joinType = "JOIN";
                    if (!navigateAttribute.Required)
                        joinType = "LEFT JOIN";

                    var joinAlias = navigateAttribute.TableName.Substring(0, 3).ToLower();
                    var joinElement = string.Format(joinElementTemplate, joinType, navigateAttribute.TableName, joinAlias, navigateAttribute.FieldName, mainAlias, fieldAttribute.Name);
                    joinElements.Add(joinElement);

                    var propertyType = property.PropertyType;
                    selectElements.AddRange(GetSelectElements(propertyType));
                }
            }

            var lastElement = selectElements.Last();
            selectElements[selectElements.FindIndex(x => x.Equals(lastElement))] = lastElement.Substring(0, lastElement.Length - 1);

            var queryElements = new List<string>();
            queryElements.Add("SELECT");
            queryElements.Add(string.Join(Environment.NewLine, selectElements));
            queryElements.Add(fromElement);
            if (joinElements.Any())
                queryElements.Add(string.Join(Environment.NewLine, joinElements));

            return string.Join(Environment.NewLine, queryElements);
        }

        private static List<string> GetSelectElements(Type type)
        {
            var selectElementTemplate = "{0}.{1} as {2},";

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            var selectElements = new List<string>();

            var onlyFieldProperies = type.GetProperties()
              .Where(x => x.GetCustomAttributes().Any(xx => xx is FieldAttribute) &&
                          !x.GetCustomAttributes().Any(xx => xx is NavigateAttribute));

            foreach (var property in onlyFieldProperies)
            {
                var attributes = property.GetCustomAttributes();
                var fieldAttribute = attributes.Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

                var selectElement = string.Format(selectElementTemplate, mainAlias, fieldAttribute.Name, property.Name);
                selectElements.Add(selectElement);
            }

            return selectElements;
        }

        #endregion

        #region GetFieldValue.

        public static string GetFieldValue(Type type, object value)
        {
            string fieldValue;

            if (type == typeof(int) || type == typeof(decimal) || type == typeof(double))
                fieldValue = value.ToString().Replace(",", ".");
            else if (type == typeof(DateTime))
                fieldValue = string.Format("'{0}'", (value as DateTime?).Value.ToString("yyyy-MM-dd HH:mm:ss"));
            //else if (type == typeof(bool))
            //  fieldValue = (value as bool?).Value ? "1" : "0";
            else
                fieldValue = $"'{value}'";

            return fieldValue;
        }

        #endregion

        #region GetPGFieldType.

        public static string GetPGFieldType(Type type)
        {
            var fieldType = string.Empty;

            if (type == typeof(short))
                fieldType = "int2";
            if (type == typeof(int))
                fieldType = "int4";
            if (type == typeof(long))
                fieldType = "int6";
            if (type == typeof(double))
                fieldType = "numeric";
            if (type == typeof(decimal))
                fieldType = "numeric";
            if (type == typeof(string))
                fieldType = "varchar";
            if (type == typeof(bool))
                fieldType = "bool";
            if (type == typeof(DateTime))
                fieldType = "timestamp";

            return fieldType;
        }

        #endregion
    }

    #region AddWhere.

    public static class AddWhereCondition
    {
        public const string OR = "OR";
        public const string AND = "AND";
    }

    public class AddWhere<T>
    {
        public string Result { get; set; }

        public AddWhere(string propertyName, string expression, object value)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            var property = type.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

            var fieldName = $"{mainAlias}.{fieldAttribute.Name}";
            var fieldValue = QueryGenerator.GetFieldValue(property.PropertyType, value);

            if (expression == QueryConstants.Expression.Like)
            {
                fieldName = $"lower({fieldName})";
                fieldValue = $"'%{fieldValue.Replace("'", "")}%'";
            }

            Result = $"{mainAlias}.{fieldAttribute.Name} {expression} {fieldValue}";
        }
    }

    #endregion

    #region AddOrder.

    public static class AddOrderDirection
    {
        public const string ASC = "ASC";
        public const string DESC = "DESC";
    }

    public class AddOrder<T>
    {
        public string Result { get; set; }

        public AddOrder(string propertyName)
        {
            var type = typeof(T);

            var mainTable = type.GetCustomAttribute<TableAttribute>().Name;
            var mainAlias = mainTable.Substring(0, 3).ToLower();

            var property = type.GetProperties().Where(x => x.Name == propertyName).FirstOrDefault();
            var fieldAttribute = property.GetCustomAttributes().Where(x => x is FieldAttribute).Cast<FieldAttribute>().FirstOrDefault();

            Result = $"{mainAlias}.{fieldAttribute.Name}";
        }
    }

    #endregion
}
