﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;

    using DotNetNuke.Common;
    using global::PetaPoco;

    [CLSCompliant(false)]
    public class PetaPocoDataContext : IDataContext
    {
        private readonly Database _database;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.
        /// </summary>
        public PetaPocoDataContext()
            : this(ConfigurationManager.ConnectionStrings[0].Name, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.
        /// </summary>
        /// <param name="connectionStringName"></param>
        public PetaPocoDataContext(string connectionStringName)
            : this(connectionStringName, string.Empty, new Dictionary<Type, IMapper>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <param name="tablePrefix"></param>
        public PetaPocoDataContext(string connectionStringName, string tablePrefix)
            : this(connectionStringName, tablePrefix, new Dictionary<Type, IMapper>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <param name="tablePrefix"></param>
        /// <param name="mappers"></param>
        public PetaPocoDataContext(string connectionStringName, string tablePrefix, Dictionary<Type, IMapper> mappers)
        {
            Requires.NotNullOrEmpty("connectionStringName", connectionStringName);

            this._database = new Database(connectionStringName);
            this._mapper = new PetaPocoMapper(tablePrefix);
            this.TablePrefix = tablePrefix;
            this.FluentMappers = mappers;
        }

        public Dictionary<Type, IMapper> FluentMappers { get; private set; }

        public string TablePrefix { get; private set; }

        public bool EnableAutoSelect
        {
            get { return this._database.EnableAutoSelect; }
            set { this._database.EnableAutoSelect = value; }
        }

        /// <inheritdoc/>
        public void BeginTransaction()
        {
            this._database.BeginTransaction();
        }

        /// <inheritdoc/>
        public void Commit()
        {
            this._database.CompleteTransaction();
        }

        /// <inheritdoc/>
        public void Execute(CommandType type, string sql, params object[] args)
        {
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            this._database.Execute(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc/>
        public IEnumerable<T> ExecuteQuery<T>(CommandType type, string sql, params object[] args)
        {
            PetaPocoMapper.SetMapper<T>(this._mapper);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return this._database.Fetch<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc/>
        public T ExecuteScalar<T>(CommandType type, string sql, params object[] args)
        {
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return this._database.ExecuteScalar<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc/>
        public T ExecuteSingleOrDefault<T>(CommandType type, string sql, params object[] args)
        {
            PetaPocoMapper.SetMapper<T>(this._mapper);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return this._database.SingleOrDefault<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc/>
        public IRepository<T> GetRepository<T>()
            where T : class
        {
            PetaPocoRepository<T> rep = null;

            // Determine whether to use a Fluent Mapper
            if (this.FluentMappers.ContainsKey(typeof(T)))
            {
                var fluentMapper = this.FluentMappers[typeof(T)] as FluentMapper<T>;
                if (fluentMapper != null)
                {
                    rep = new PetaPocoRepository<T>(this._database, fluentMapper);
                    rep.Initialize(fluentMapper.CacheKey, fluentMapper.CacheTimeOut, fluentMapper.CachePriority, fluentMapper.Scope);
                }
            }
            else
            {
                rep = new PetaPocoRepository<T>(this._database, this._mapper);
            }

            return rep;
        }

        /// <inheritdoc/>
        public void RollbackTransaction()
        {
            this._database.AbortTransaction();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this._database.Dispose();
        }
    }
}
