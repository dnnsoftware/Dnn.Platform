// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Threading;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using global::PetaPoco;

    public class PetaPocoMapper : IMapper
    {
        private static IMapper defaultMapper;
        private static ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
        private readonly string tablePrefix;

        /// <summary>Initializes a new instance of the <see cref="PetaPocoMapper"/> class.</summary>
        /// <param name="tablePrefix">The table prefix.</param>
        public PetaPocoMapper(string tablePrefix)
        {
            this.tablePrefix = tablePrefix;
            defaultMapper = new StandardMapper();
        }

        public static void SetMapper<T>(IMapper mapper)
        {
            @lock.EnterWriteLock();
            try
            {
                if (Mappers.GetMapper(typeof(T), defaultMapper) is StandardMapper)
                {
                    Mappers.Register(typeof(T), mapper);
                }
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
        {
            bool includeColumn = true;

            // Check if the class has the DeclareColumnsAttribute
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

        /// <inheritdoc/>
        public TableInfo GetTableInfo(Type pocoType)
        {
            TableInfo ti = TableInfo.FromPoco(pocoType);

            // Table Name
            ti.TableName = DataUtil.GetTableName(pocoType, ti.TableName + "s");

            ti.TableName = this.tablePrefix + ti.TableName;

            // Primary Key
            ti.PrimaryKey = DataUtil.GetPrimaryKeyColumn(pocoType, ti.PrimaryKey);

            ti.AutoIncrement = DataUtil.GetAutoIncrement(pocoType, true);

            return ti;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public Func<object, object> GetFromDbConverter(PropertyInfo pi, Type sourceType)
        {
            return null;
        }

        /// <inheritdoc/>
        public Func<object, object> GetToDbConverter(PropertyInfo sourceProperty)
        {
            return null;
        }
    }
}
