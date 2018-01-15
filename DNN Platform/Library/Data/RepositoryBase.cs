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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Data
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : class
    {
        #region Constructors

        protected RepositoryBase()
        {
            InitializeInternal();
        }

        #endregion

        #region IRepository<T> Implementation

        public void Delete(T item)
        {
            DeleteInternal(item);
            ClearCache(item);
        }

        public abstract void Delete(string sqlCondition, params object[] args);

        public abstract IEnumerable<T> Find(string sqlCondition, params object[] args);

        public abstract IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args);

        public IEnumerable<T> Get()
        {
            return IsCacheable && !IsScoped
                ? DataCache.GetCachedData<IEnumerable<T>>(CacheArgs, c => GetInternal())
                : GetInternal();
        }

        public IEnumerable<T> Get<TScopeType>(TScopeType scopeValue)
        {
            CheckIfScoped();

            if(IsCacheable)
            {
                CacheArgs.CacheKey = String.Format(CacheArgs.CacheKey, scopeValue);
            }

            return IsCacheable
                ? DataCache.GetCachedData<IEnumerable<T>>(CacheArgs, c => GetByScopeInternal(scopeValue))
                : GetByScopeInternal(scopeValue);
        }

        public T GetById<TProperty>(TProperty id)
        {
            return IsCacheable && !IsScoped
                        ? Get().SingleOrDefault(t => CompareTo(GetPrimaryKey<TProperty>(t), id) == 0)
                        : GetByIdInternal(id);
        }

        public T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue)
        {
            CheckIfScoped();

            return Get(scopeValue).SingleOrDefault(t => CompareTo(GetPrimaryKey<TProperty>(t), id) == 0);
        }

        public IPagedList<T> GetPage(int pageIndex, int pageSize)
        {
            return IsCacheable && !IsScoped
                ? Get().InPagesOf(pageSize).GetPage(pageIndex)
                : GetPageInternal(pageIndex, pageSize);
        }

        public IPagedList<T> GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize)
        {
            CheckIfScoped();

            return IsCacheable
                ? Get(scopeValue).InPagesOf(pageSize).GetPage(pageIndex)
                : GetPageByScopeInternal(scopeValue, pageIndex, pageSize);
        }

        public void Insert(T item)
        {
            InsertInternal(item);
            ClearCache(item);
        }

        public void Update(T item)
        {
            UpdateInternal(item);
            ClearCache(item);
        }

        public abstract void Update(string sqlCondition, params object[] args);

        #endregion

        #region Private Methods

        private void CheckIfScoped()
        {
            if (!IsScoped)
            {
                throw new NotSupportedException("This method requires the model to be cacheable and have a cache scope defined");
            }
        }

        private void ClearCache(T item)
        {
            if (IsCacheable)
            {
                DataCache.RemoveCache(IsScoped
                                          ? String.Format(CacheArgs.CacheKey, GetScopeValue<object>(item))
                                          : CacheArgs.CacheKey);
            }
        }

        private void InitializeInternal()
        {
            var type = typeof (T);
            Scope = String.Empty;
            IsCacheable = false;
            IsScoped = false;
            CacheArgs = null;

            var scopeAttribute = DataUtil.GetAttribute<ScopeAttribute>(type);
            if (scopeAttribute != null)
            {
                Scope = scopeAttribute.Scope;
            }

            IsScoped = (!String.IsNullOrEmpty(Scope));

            var cacheableAttribute = DataUtil.GetAttribute<CacheableAttribute>(type);
            if (cacheableAttribute != null)
            {
                IsCacheable = true;
                var cacheKey = !String.IsNullOrEmpty(cacheableAttribute.CacheKey)
                                ? cacheableAttribute.CacheKey
                                : String.Format("OR_{0}", type.Name);
                var cachePriority = cacheableAttribute.CachePriority;
                var cacheTimeOut = cacheableAttribute.CacheTimeOut;

                if (IsScoped)
                {
                    cacheKey += "_" + Scope + "_{0}";
                }

                CacheArgs = new CacheItemArgs(cacheKey, cacheTimeOut, cachePriority);
            }
        }

        #endregion

        #region Protected Properties

        protected CacheItemArgs CacheArgs { get; private set; }

        protected string Scope { get; private set; }

        protected bool IsCacheable { get; private set; }

        protected bool IsScoped { get; private set; }

        #endregion

        protected int CompareTo<TProperty>(TProperty first, TProperty second)
        {
            Requires.IsTypeOf<IComparable>("first", first);
            Requires.IsTypeOf<IComparable>("second", second);

            var firstComparable = first as IComparable;
            var secondComparable = second as IComparable;

// ReSharper disable PossibleNullReferenceException
            return firstComparable.CompareTo(secondComparable);
// ReSharper restore PossibleNullReferenceException
        }

        protected TProperty GetPropertyValue<TProperty>(T item, string propertyName)
        {
            return DataUtil.GetPropertyValue<T, TProperty>(item, propertyName);
        }

        protected TProperty GetPrimaryKey<TProperty>(T item)
        {
            return DataUtil.GetPrimaryKey<T, TProperty>(item);
        }

        protected TProperty GetScopeValue<TProperty>(T item)
        {
            return DataUtil.GetPropertyValue<T, TProperty>(item, Scope);
        }

        #region Abstract Methods

        protected abstract void DeleteInternal(T item);

        protected abstract IEnumerable<T> GetInternal();

        protected abstract IPagedList<T> GetPageInternal(int pageIndex, int pageSize);

        protected abstract IEnumerable<T> GetByScopeInternal(object propertyValue);

        protected abstract IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize);

        protected abstract T GetByIdInternal(object id);

        protected abstract void InsertInternal(T item);

        protected abstract void UpdateInternal(T item);

        #endregion

        public void Initialize(string cacheKey, int cacheTimeOut = 20, CacheItemPriority cachePriority = CacheItemPriority.Default, string scope = "")
        {
            Scope = scope;
            IsScoped = (!String.IsNullOrEmpty(Scope));
            IsCacheable = (!String.IsNullOrEmpty(cacheKey));
            if (IsCacheable)
            {
                if (IsScoped)
                {
                    cacheKey += "_" + Scope + "_{0}";
                }
                CacheArgs = new CacheItemArgs(cacheKey, cacheTimeOut, cachePriority);
            }
            else
            {
                CacheArgs = null;
            }
        }
    }
}
