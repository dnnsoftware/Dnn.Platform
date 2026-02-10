// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using global::PetaPoco;

    using Microsoft.Extensions.DependencyInjection;

    [CLSCompliant(false)]
    public class PetaPocoDataContext : IDataContext
    {
        private readonly IHostSettings hostSettings;
        private readonly Database database;
        private readonly IMapper mapper;

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoDataContext()
            : this(ConfigurationManager.ConnectionStrings[0].Name, string.Empty)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public PetaPocoDataContext(IHostSettings hostSettings)
            : this(hostSettings, ConfigurationManager.ConnectionStrings[0].Name, string.Empty)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="connectionStringName">The connection string name.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoDataContext(string connectionStringName)
            : this(connectionStringName, string.Empty, new Dictionary<Type, IMapper>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        public PetaPocoDataContext(IHostSettings hostSettings, string connectionStringName)
            : this(hostSettings, connectionStringName, string.Empty, new Dictionary<Type, IMapper>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tablePrefix">The table prefix.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoDataContext(string connectionStringName, string tablePrefix)
            : this(connectionStringName, tablePrefix, new Dictionary<Type, IMapper>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tablePrefix">The table prefix.</param>
        public PetaPocoDataContext(IHostSettings hostSettings, string connectionStringName, string tablePrefix)
            : this(hostSettings, connectionStringName, tablePrefix, new Dictionary<Type, IMapper>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tablePrefix">The table prefix.</param>
        /// <param name="mappers">A <see cref="Dictionary{TKey,TValue}"/> with <see cref="FluentMapper{TModel}"/> instances for types.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoDataContext(string connectionStringName, string tablePrefix, Dictionary<Type, IMapper> mappers)
            : this(null, connectionStringName, tablePrefix, mappers)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoDataContext"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="connectionStringName">The connection string name.</param>
        /// <param name="tablePrefix">The table prefix.</param>
        /// <param name="mappers">A <see cref="Dictionary{TKey,TValue}"/> with <see cref="FluentMapper{TModel}"/> instances for types.</param>
        public PetaPocoDataContext(IHostSettings hostSettings, string connectionStringName, string tablePrefix, Dictionary<Type, IMapper> mappers)
        {
            Requires.NotNullOrEmpty("connectionStringName", connectionStringName);

            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

            this.database = new Database(connectionStringName);
            this.mapper = new PetaPocoMapper(tablePrefix);
            this.TablePrefix = tablePrefix;
            this.FluentMappers = mappers;
        }

        public Dictionary<Type, IMapper> FluentMappers { get; private set; }

        public string TablePrefix { get; private set; }

        public bool EnableAutoSelect
        {
            get { return this.database.EnableAutoSelect; }
            set { this.database.EnableAutoSelect = value; }
        }

        /// <inheritdoc />
        public void BeginTransaction()
        {
            this.database.BeginTransaction();
        }

        /// <inheritdoc />
        public void Commit()
        {
            this.database.CompleteTransaction();
        }

        /// <inheritdoc />
        public void Execute(CommandType type, string sql, params object[] args)
        {
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            this.database.Execute(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc />
        public IEnumerable<T> ExecuteQuery<T>(CommandType type, string sql, params object[] args)
        {
            PetaPocoMapper.SetMapper<T>(this.mapper);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return this.database.Fetch<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc />
        public T ExecuteScalar<T>(CommandType type, string sql, params object[] args)
        {
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return this.database.ExecuteScalar<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc />
        public T ExecuteSingleOrDefault<T>(CommandType type, string sql, params object[] args)
        {
            PetaPocoMapper.SetMapper<T>(this.mapper);
            if (type == CommandType.StoredProcedure)
            {
                sql = DataUtil.GenerateExecuteStoredProcedureSql(sql, args);
            }

            return this.database.SingleOrDefault<T>(DataUtil.ReplaceTokens(sql), args);
        }

        /// <inheritdoc />
        public IRepository<T> GetRepository<T>()
            where T : class
        {
            PetaPocoRepository<T> rep = null;

            // Determine whether to use a Fluent Mapper
            if (this.FluentMappers.ContainsKey(typeof(T)))
            {
                if (this.FluentMappers[typeof(T)] is FluentMapper<T> fluentMapper)
                {
                    rep = new PetaPocoRepository<T>(this.hostSettings, this.database, fluentMapper);
                    rep.Initialize(fluentMapper.CacheKey, fluentMapper.CacheTimeOut, fluentMapper.CachePriority, fluentMapper.Scope);
                }
            }
            else
            {
                rep = new PetaPocoRepository<T>(this.hostSettings, this.database, this.mapper);
            }

            return rep;
        }

        /// <inheritdoc />
        public void RollbackTransaction()
        {
            this.database.AbortTransaction();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.database.Dispose();
            }
        }
    }
}
