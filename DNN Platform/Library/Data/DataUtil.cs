// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    using DotNetNuke.Collections;
    using DotNetNuke.ComponentModel.DataAnnotations;

    internal static class DataUtil
    {
        internal static string GenerateExecuteStoredProcedureSql(string procedureName, params object[] args)
        {
            var sb = new StringBuilder(";Exec ");
            sb.Append(procedureName);
            for (int i = 0; i < args.Length; i++)
            {
                var parameterFormat = " @{0}";
                string parameterName = null;
                var param = args[i] as IDataParameter;
                if (param != null)
                {
                    // intentionally adding an extra @ before parameter name, so PetaPoco won't try to match it to a passed-in arg
                    parameterFormat = " @{1}=@{0}";
                    if (param.Direction == ParameterDirection.Output)
                    { // Add output for output params
                        parameterFormat += " output";
                    }

                    parameterName = param.ParameterName;
                }

                sb.AppendFormat(CultureInfo.InvariantCulture, parameterFormat, i, parameterName);
                if (i < args.Length - 1)
                {
                    sb.Append(",");
                }
            }

            return sb.ToString();
        }

        internal static TAttribute GetAttribute<TAttribute>(Type type)
        {
            TAttribute attribute = default(TAttribute);

            object[] tableNameAttributes = type.GetCustomAttributes(typeof(TAttribute), true);
            if (tableNameAttributes.Length > 0)
            {
                attribute = (TAttribute)tableNameAttributes[0];
            }

            return attribute;
        }

        internal static bool GetAutoIncrement(Type type, bool defaultValue)
        {
            var autoIncrement = defaultValue;

            var primaryKeyAttribute = GetAttribute<PrimaryKeyAttribute>(type);
            if (primaryKeyAttribute != null)
            {
                autoIncrement = primaryKeyAttribute.AutoIncrement;
            }

            return autoIncrement;
        }

        internal static string GetColumnName(Type type, string propertyName)
        {
            return GetColumnName(type.GetProperty(propertyName), propertyName);
        }

        internal static string GetColumnName(PropertyInfo propertyInfo, string defaultName)
        {
            var columnName = defaultName;

            object[] columnNameAttributes = propertyInfo.GetCustomAttributes(typeof(ColumnNameAttribute), true);
            if (columnNameAttributes.Length > 0)
            {
                var columnNameAttribute = (ColumnNameAttribute)columnNameAttributes[0];
                columnName = columnNameAttribute.ColumnName;
            }

            return columnName;
        }

        internal static bool GetIsCacheable(Type type)
        {
            return GetAttribute<CacheableAttribute>(type) != null;
        }

        internal static TProperty GetPrimaryKey<TEntity, TProperty>(TEntity item)
        {
            Type modelType = typeof(TEntity);

            // Get the primary key
            var primaryKeyName = GetPrimaryKeyProperty(modelType, string.Empty);

            return GetPropertyValue<TEntity, TProperty>(item, primaryKeyName);
        }

        internal static string GetPrimaryKeyColumn(Type type, string defaultName)
        {
            var primaryKeyName = defaultName;

            var primaryKeyAttribute = GetAttribute<PrimaryKeyAttribute>(type);
            if (primaryKeyAttribute != null)
            {
                primaryKeyName = primaryKeyAttribute.ColumnName;
            }

            return primaryKeyName;
        }

        internal static string GetPrimaryKeyProperty(Type type, string defaultName)
        {
            var primaryKeyName = defaultName;

            var primaryKeyAttribute = GetAttribute<PrimaryKeyAttribute>(type);
            if (primaryKeyAttribute != null)
            {
                primaryKeyName = primaryKeyAttribute.PropertyName;
            }

            return primaryKeyName;
        }

        internal static TProperty GetPropertyValue<TEntity, TProperty>(TEntity item, string propertyName)
        {
            var modelType = typeof(TEntity);
            var property = modelType.GetProperty(propertyName);

            return (TProperty)property.GetValue(item, null);
        }

        internal static string GetTableName(Type type)
        {
            return GetTableName(type, string.Empty);
        }

        internal static string GetTableName(Type type, string defaultName)
        {
            var tableName = defaultName;

            var tableNameAttribute = GetAttribute<TableNameAttribute>(type);
            if (tableNameAttribute != null)
            {
                tableName = tableNameAttribute.TableName;
            }

            return tableName;
        }

        internal static string ReplaceTokens(string sql)
        {
            var isSqlCe = DataProvider.Instance().Settings.GetValueOrDefault("isSqlCe", false);
            return sql.Replace("{databaseOwner}", isSqlCe ? string.Empty : DataProvider.Instance().DatabaseOwner)
                        .Replace("{objectQualifier}", DataProvider.Instance().ObjectQualifier);
        }
    }
}
