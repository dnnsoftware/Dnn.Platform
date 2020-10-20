// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data.PetaPoco
{
    using System.Collections.Generic;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using global::PetaPoco;

    public class PetaPocoRepository<T> : RepositoryBase<T>
        where T : class
    {
        private readonly Database _database;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PetaPocoRepository{T}"/> class.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="mapper"></param>
        public PetaPocoRepository(Database database, IMapper mapper)
        {
            Requires.NotNull("database", database);

            this._database = database;
            this._mapper = mapper;

            PetaPocoMapper.SetMapper<T>(mapper);
        }

        /// <inheritdoc/>
        public override void Delete(string sqlCondition, params object[] args)
        {
            this._database.Delete<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        /// <inheritdoc/>
        public override IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
            return this._database.Fetch<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        /// <inheritdoc/>
        public override IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            // Make sure that the sql Condition contains an ORDER BY Clause
            if (!sqlCondition.ToUpperInvariant().Contains("ORDER BY"))
            {
                sqlCondition = string.Format("{0} ORDER BY {1}", sqlCondition, this._mapper.GetTableInfo(typeof(T)).PrimaryKey);
            }

            Page<T> petaPocoPage = this._database.Page<T>(pageIndex + 1, pageSize, DataUtil.ReplaceTokens(sqlCondition), args);

            return new PagedList<T>(petaPocoPage.Items, (int)petaPocoPage.TotalItems, pageIndex, pageSize);
        }

        /// <inheritdoc/>
        public override void Update(string sqlCondition, params object[] args)
        {
            this._database.Update<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        /// <inheritdoc/>
        protected override void DeleteInternal(T item)
        {
            this._database.Delete(item);
        }

        /// <inheritdoc/>
        protected override IEnumerable<T> GetInternal()
        {
            return this._database.Fetch<T>(string.Empty);
        }

        /// <inheritdoc/>
        protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
        {
            return this.Find(pageIndex, pageSize, string.Empty);
        }

        /// <inheritdoc/>
        protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
        {
            return this._database.Fetch<T>(this.GetScopeSql(), propertyValue);
        }

        /// <inheritdoc/>
        protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
        {
            return this.Find(pageIndex, pageSize, this.GetScopeSql(), propertyValue);
        }

        /// <inheritdoc/>
        protected override T GetByIdInternal(object id)
        {
            return this._database.SingleOrDefault<T>(id);
        }

        /// <inheritdoc/>
        protected override void InsertInternal(T item)
        {
            this._database.Insert(item);
        }

        /// <inheritdoc/>
        protected override void UpdateInternal(T item)
        {
            this._database.Update(item);
        }

        private string GetScopeSql()
        {
            return string.Format("WHERE {0} = @0", DataUtil.GetColumnName(typeof(T), this.Scope));
        }
    }
}
