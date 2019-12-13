// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

using DotNetNuke.Collections;
using DotNetNuke.Common;

using PetaPoco;

#endregion

namespace DotNetNuke.Data.PetaPoco
{
    public class PetaPocoRepository<T> : RepositoryBase<T> where T : class
    {
        private readonly Database _database;
        private readonly IMapper _mapper;

        #region Constructors

        public PetaPocoRepository(Database database, IMapper mapper)
        {
            Requires.NotNull("database", database);

            _database = database;
            _mapper = mapper;

            PetaPocoMapper.SetMapper<T>(mapper);
        }

        #endregion

        #region IRepository<T> Implementation

        public override void Delete(string sqlCondition, params object[] args)
        {
            _database.Delete<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        public override IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
            return _database.Fetch<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        public override IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            //Make sure that the sql Condition contains an ORDER BY Clause
            if(!sqlCondition.ToUpperInvariant().Contains("ORDER BY"))
            {
                sqlCondition = String.Format("{0} ORDER BY {1}", sqlCondition, _mapper.GetTableInfo(typeof(T)).PrimaryKey);
            }
            Page<T> petaPocoPage = _database.Page<T>(pageIndex + 1, pageSize, DataUtil.ReplaceTokens(sqlCondition), args);

            return new PagedList<T>(petaPocoPage.Items, (int)petaPocoPage.TotalItems, pageIndex, pageSize);
        }

        public override void Update(string sqlCondition, params object[] args)
        {
            _database.Update<T>(DataUtil.ReplaceTokens(sqlCondition), args);
        }

        #endregion

        protected override void DeleteInternal(T item)
        {
            _database.Delete(item);
        }

        protected override IEnumerable<T> GetInternal()
        {
            return _database.Fetch<T>(String.Empty);
        }

        protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
        {
            return Find(pageIndex, pageSize, String.Empty);
        }

        protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
        {
            return _database.Fetch<T>(GetScopeSql(), propertyValue);
        }

        protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
        {
            return Find(pageIndex, pageSize, GetScopeSql(), propertyValue);
        }

        protected override T GetByIdInternal(object id)
        {
            return _database.SingleOrDefault<T>(id);
        }

        protected override void InsertInternal(T item)
        {
            _database.Insert(item);
        }

        protected override void UpdateInternal(T item)
        {
            _database.Update(item);
        }

        private string GetScopeSql()
        {
            return String.Format("WHERE {0} = @0", DataUtil.GetColumnName(typeof (T), Scope));
        }
    }
}
