// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using DotNetNuke.Common;
using PetaPoco;

namespace DotNetNuke.Data.PetaPoco
{
    [CLSCompliant(false)]
    public class PetaPocoDataContext : IDataContext
    {
        #region Private Members

        private readonly Database _database;
        private readonly IMapper _mapper;

        #endregion

        #region Constructors

        public PetaPocoDataContext()
            : this(ConfigurationManager.ConnectionStrings[0].Name, String.Empty)
        {
        }

        public PetaPocoDataContext(string connectionStringName)
            : this(connectionStringName, String.Empty, new Dictionary<Type, IMapper>())
        {
        }

        public PetaPocoDataContext(string connectionStringName, string tablePrefix)
            : this(connectionStringName, tablePrefix, new Dictionary<Type, IMapper>())
        {
        }

        public PetaPocoDataContext(string connectionStringName, string tablePrefix, Dictionary<Type, IMapper> mappers)
        {
            Requires.NotNullOrEmpty("connectionStringName", connectionStringName);

            _database = new Database(connectionStringName);
            _mapper = new PetaPocoMapper(tablePrefix);
            TablePrefix = tablePrefix;
            FluentMappers = mappers;
        }

        #endregion

        public Dictionary<Type, IMapper> FluentMappers { get; private set; }

        public string TablePrefix { get; private set; }

        #region Implementation of IDataContext

        public void BeginTransaction()
        {
            _database.BeginTransaction();
        }

        public void Commit()
        {
            _database.CompleteTransaction();
        }

        public bool EnableAutoSelect
        {
            get { return _database.EnableAutoSelect; }
            set { _database.EnableAutoSelect = value; }
        }

        public void Execute(CommandType type, string sql, params object[] args)
        {
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            _database.Execute(DataUtil.ReplaceTokens(sql), args);
        }

        public IEnumerable<T> ExecuteQuery<T>(CommandType type, string sql, params object[] args)
        {
            PetaPocoMapper.SetMapper<T>(_mapper);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return _database.Fetch<T>(DataUtil.ReplaceTokens(sql), args);
        }

        public T ExecuteScalar<T>(CommandType type, string sql, params object[] args)
        {
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return _database.ExecuteScalar<T>(DataUtil.ReplaceTokens(sql), args);
        }

        public T ExecuteSingleOrDefault<T>(CommandType type, string sql, params object[] args)
        {
            PetaPocoMapper.SetMapper<T>(_mapper);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return _database.SingleOrDefault<T>(DataUtil.ReplaceTokens(sql), args);
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            PetaPocoRepository<T> rep = null;

            //Determine whether to use a Fluent Mapper
            if (FluentMappers.ContainsKey(typeof (T)))
            {
                var fluentMapper = FluentMappers[typeof(T)] as FluentMapper<T>;
                if (fluentMapper != null)
                {
                    rep = new PetaPocoRepository<T>(_database, fluentMapper);
                    rep.Initialize(fluentMapper.CacheKey, fluentMapper.CacheTimeOut, fluentMapper.CachePriority, fluentMapper.Scope);
                }
            }
            else
            {
                rep = new PetaPocoRepository<T>(_database, _mapper);
            }

            return rep;
        }

        public void RollbackTransaction()
        {
            _database.AbortTransaction();
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            _database.Dispose();
        }

        #endregion
    }
}
