#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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