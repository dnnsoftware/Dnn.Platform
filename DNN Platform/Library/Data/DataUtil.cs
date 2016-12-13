#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using DotNetNuke.Collections;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Data
{
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
            return (GetAttribute<CacheableAttribute>(type) != null);
        }

        internal static TProperty GetPrimaryKey<TEntity, TProperty>(TEntity item)
        {
            Type modelType = typeof(TEntity);

            //Get the primary key
            var primaryKeyName = GetPrimaryKeyProperty(modelType, String.Empty);

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
            return GetTableName(type, String.Empty);
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
            return sql.Replace("{databaseOwner}", (isSqlCe) ? String.Empty : DataProvider.Instance().DatabaseOwner)
                        .Replace("{objectQualifier}", DataProvider.Instance().ObjectQualifier);
        }
    }
}