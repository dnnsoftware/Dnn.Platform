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