// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

using DotNetNuke.Collections;
using DotNetNuke.Data;

namespace DotNetNuke.Tests.Data.Fakes
{
    public class FakeRepository<T> : RepositoryBase<T> where T : class
    {
        public override void Delete(string sqlCondition, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public override IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public override void Update(string sqlCondition, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
        {
            throw new System.NotImplementedException();
        }

        protected override T GetByIdInternal(object id)
        {
            throw new System.NotImplementedException();
        }

        protected override void InsertInternal(T item)
        {
            throw new System.NotImplementedException();
        }

        protected override void DeleteInternal(T item)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<T> GetInternal()
        {
            throw new System.NotImplementedException();
        }

        protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
        {
            throw new System.NotImplementedException();
        }

        protected override void UpdateInternal(T item)
        {
            throw new System.NotImplementedException();
        }
    }
}
