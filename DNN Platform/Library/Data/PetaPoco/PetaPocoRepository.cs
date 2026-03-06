// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using global::PetaPoco;

    using Microsoft.Extensions.DependencyInjection;

    public class PetaPocoRepository<T> : RepositoryBase<T>
        where T : class
    {
        private readonly Database database;
        private readonly IMapper mapper;

        /// <summary>Initializes a new instance of the <see cref="PetaPocoRepository{T}"/> class.</summary>
        /// <param name="database">The database.</param>
        /// <param name="mapper">The mapper.</param>
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PetaPocoRepository(Database database, IMapper mapper)
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), database, mapper)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PetaPocoRepository{T}"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="database">The database.</param>
        /// <param name="mapper">The mapper.</param>
        public PetaPocoRepository(IHostSettings hostSettings, Database database, IMapper mapper)
            : base(hostSettings)
#pragma warning restore CS3001
        {
            Requires.NotNull("database", database);

            this.database = database;
            this.mapper = mapper;

            PetaPocoMapper.SetMapper<T>(mapper);
        }

        /// <inheritdoc />
        public override void Delete(string sqlCondition, params object[] args)
        {
            this.database.Delete<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        /// <inheritdoc />
        public override IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
            return this.database.Fetch<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        /// <inheritdoc />
        public override IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            // Make sure that the sql Condition contains an ORDER BY Clause
            if (!sqlCondition.ToUpperInvariant().Contains("ORDER BY"))
            {
                sqlCondition = $"{sqlCondition} ORDER BY {this.mapper.GetTableInfo(typeof(T)).PrimaryKey}";
            }

            Page<T> petaPocoPage = this.database.Page<T>(pageIndex + 1, pageSize, DataUtil.ReplaceTokens(sqlCondition), args);

            return new PagedList<T>(petaPocoPage.Items, (int)petaPocoPage.TotalItems, pageIndex, pageSize);
        }

        /// <inheritdoc />
        public override void Update(string sqlCondition, params object[] args)
        {
            this.database.Update<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        /// <inheritdoc />
        protected override void DeleteInternal(T item)
        {
            this.database.Delete(item);
        }

        /// <inheritdoc />
        protected override IEnumerable<T> GetInternal()
        {
            return this.database.Fetch<T>(string.Empty);
        }

        /// <inheritdoc />
        protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
        {
            return this.Find(pageIndex, pageSize, string.Empty);
        }

        /// <inheritdoc />
        protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
        {
            return this.database.Fetch<T>(this.GetScopeSql(), propertyValue);
        }

        /// <inheritdoc />
        protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
        {
            return this.Find(pageIndex, pageSize, this.GetScopeSql(), propertyValue);
        }

        /// <inheritdoc />
        protected override T GetByIdInternal(object id)
        {
            return this.database.SingleOrDefault<T>(id);
        }

        /// <inheritdoc />
        protected override void InsertInternal(T item)
        {
            this.database.Insert(item);
        }

        /// <inheritdoc />
        protected override void UpdateInternal(T item)
        {
            this.database.Update(item);
        }

        private string GetScopeSql()
        {
            return $"WHERE {DataUtil.GetColumnName(typeof(T), this.Scope)} = @0";
        }
    }
}
