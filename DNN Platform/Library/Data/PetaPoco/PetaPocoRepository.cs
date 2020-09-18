// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using global::PetaPoco;

    public class PetaPocoRepository<T> : RepositoryBase<T>
        where T : class
    {
        private readonly Database _database;
        private readonly IMapper _mapper;

        public PetaPocoRepository(Database database, IMapper mapper)
        {
            Requires.NotNull("database", database);

            this._database = database;
            this._mapper = mapper;

            PetaPocoMapper.SetMapper<T>(mapper);
        }

        public override void Delete(string sqlCondition, params object[] args)
        {
            this._database.Delete<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        public override IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
            return this._database.Fetch<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

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

        public override void Update(string sqlCondition, params object[] args)
        {
            this._database.Update<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        protected override void DeleteInternal(T item)
        {
            this._database.Delete(item);
        }

        protected override IEnumerable<T> GetInternal()
        {
            return this._database.Fetch<T>(string.Empty);
        }

        protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
        {
            return this.Find(pageIndex, pageSize, string.Empty);
        }

        protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
        {
            return this._database.Fetch<T>(this.GetScopeSql(), propertyValue);
        }

        protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
        {
            return this.Find(pageIndex, pageSize, this.GetScopeSql(), propertyValue);
        }

        protected override T GetByIdInternal(object id)
        {
            return this._database.SingleOrDefault<T>(id);
        }

        protected override void InsertInternal(T item)
        {
            this._database.Insert(item);
        }

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
