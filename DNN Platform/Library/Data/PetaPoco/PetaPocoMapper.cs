#region Usings

using System;
using System.Reflection;
using System.Threading;
using DotNetNuke.ComponentModel.DataAnnotations;
using PetaPoco;

#endregion

namespace DotNetNuke.Data.PetaPoco
{
    public class PetaPocoMapper : IMapper
    {
        private static IMapper _defaultMapper;
        private readonly string _tablePrefix;
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public PetaPocoMapper(string tablePrefix)
        {
            _tablePrefix = tablePrefix;
            _defaultMapper = new StandardMapper();
        }

        #region Implementation of IMapper

        public ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
        {
            bool includeColumn = true;

            //Check if the class has the ExplictColumnsAttribute
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

                ci.ResultColumn = (pocoProperty.GetCustomAttributes(typeof(ReadOnlyColumnAttribute), true).Length > 0);
            }

            return ci;
        }

        public TableInfo GetTableInfo(Type pocoType)
        {
            TableInfo ti = TableInfo.FromPoco(pocoType);

            //Table Name
            ti.TableName = DataUtil.GetTableName(pocoType, ti.TableName + "s");

            ti.TableName = _tablePrefix + ti.TableName;

            //Primary Key
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

        #endregion

        public static void SetMapper<T>(IMapper mapper)
        {
            _lock.EnterWriteLock();
            try
            {
                if (Mappers.GetMapper(typeof (T), _defaultMapper) is StandardMapper)
                {
                    Mappers.Register(typeof(T), mapper);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
