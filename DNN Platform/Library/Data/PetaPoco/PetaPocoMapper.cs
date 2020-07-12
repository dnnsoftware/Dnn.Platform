// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Reflection;
    using System.Threading;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using global::PetaPoco;

    public class PetaPocoMapper : IMapper
    {
        private static IMapper _defaultMapper;
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly string _tablePrefix;

        public PetaPocoMapper(string tablePrefix)
        {
            this._tablePrefix = tablePrefix;
            _defaultMapper = new StandardMapper();
        }

        public static void SetMapper<T>(IMapper mapper)
        {
            _lock.EnterWriteLock();
            try
            {
                if (Mappers.GetMapper(typeof(T), _defaultMapper) is StandardMapper)
                {
                    Mappers.Register(typeof(T), mapper);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
        {
            bool includeColumn = true;

            // Check if the class has the ExplictColumnsAttribute
            bool declareColumns = pocoProperty.DeclaringType != null
                            && pocoProperty.DeclaringType.GetCustomAttributes(typeof(DeclareColumnsAttribute), true).Length > 0;

            if (declareColumns)
            {
                if (pocoProperty.GetCustomAttributes(typeof(IncludeColumnAttribute), true).Length == 0)
                {
                    includeColumn = false;
                }
            }
            else
            {
                if (pocoProperty.GetCustomAttributes(typeof(IgnoreColumnAttribute), true).Length > 0)
                {
                    includeColumn = false;
                }
            }

            ColumnInfo ci = null;
            if (includeColumn)
            {
                ci = ColumnInfo.FromProperty(pocoProperty);
                ci.ColumnName = DataUtil.GetColumnName(pocoProperty, ci.ColumnName);

                ci.ResultColumn = pocoProperty.GetCustomAttributes(typeof(ReadOnlyColumnAttribute), true).Length > 0;
            }

            return ci;
        }

        public TableInfo GetTableInfo(Type pocoType)
        {
            TableInfo ti = TableInfo.FromPoco(pocoType);

            // Table Name
            ti.TableName = DataUtil.GetTableName(pocoType, ti.TableName + "s");

            ti.TableName = this._tablePrefix + ti.TableName;

            // Primary Key
            ti.PrimaryKey = DataUtil.GetPrimaryKeyColumn(pocoType, ti.PrimaryKey);

            ti.AutoIncrement = DataUtil.GetAutoIncrement(pocoType, true);

            return ti;
        }

        public Func<object, object> GetFromDbConverter(PropertyInfo pi, Type SourceType)
        {
            return null;
        }

        public Func<object, object> GetToDbConverter(PropertyInfo SourceProperty)
        {
            return null;
        }
    }
}
