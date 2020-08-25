// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Caching;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel.DataAnnotations;

    public abstract class RepositoryBase<T> : IRepository<T>
        where T : class
    {
        protected RepositoryBase()
        {
            this.InitializeInternal();
        }

        protected CacheItemArgs CacheArgs { get; private set; }

        protected string Scope { get; private set; }

        protected bool IsCacheable { get; private set; }

        protected bool IsScoped { get; private set; }

        public void Delete(T item)
        {
            this.DeleteInternal(item);
            this.ClearCache(item);
        }

        public abstract void Delete(string sqlCondition, params object[] args);

        public abstract IEnumerable<T> Find(string sqlCondition, params object[] args);

        public abstract IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args);

        public IEnumerable<T> Get()
        {
            return this.IsCacheable && !this.IsScoped
                ? DataCache.GetCachedData<IEnumerable<T>>(this.CacheArgs, c => this.GetInternal())
                : this.GetInternal();
        }

        public IEnumerable<T> Get<TScopeType>(TScopeType scopeValue)
        {
            this.CheckIfScoped();

            if (this.IsCacheable)
            {
                this.CacheArgs.CacheKey = string.Format(this.CacheArgs.CacheKey, scopeValue);
            }

            return this.IsCacheable
                ? DataCache.GetCachedData<IEnumerable<T>>(this.CacheArgs, c => this.GetByScopeInternal(scopeValue))
                : this.GetByScopeInternal(scopeValue);
        }

        public T GetById<TProperty>(TProperty id)
        {
            return this.IsCacheable && !this.IsScoped
                        ? this.Get().SingleOrDefault(t => this.CompareTo(this.GetPrimaryKey<TProperty>(t), id) == 0)
                        : this.GetByIdInternal(id);
        }

        public T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue)
        {
            this.CheckIfScoped();

            return this.Get(scopeValue).SingleOrDefault(t => this.CompareTo(this.GetPrimaryKey<TProperty>(t), id) == 0);
        }

        public IPagedList<T> GetPage(int pageIndex, int pageSize)
        {
            return this.IsCacheable && !this.IsScoped
                ? this.Get().InPagesOf(pageSize).GetPage(pageIndex)
                : this.GetPageInternal(pageIndex, pageSize);
        }

        public IPagedList<T> GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize)
        {
            this.CheckIfScoped();

            return this.IsCacheable
                ? this.Get(scopeValue).InPagesOf(pageSize).GetPage(pageIndex)
                : this.GetPageByScopeInternal(scopeValue, pageIndex, pageSize);
        }

        public void Insert(T item)
        {
            this.InsertInternal(item);
            this.ClearCache(item);
        }

        public void Update(T item)
        {
            this.UpdateInternal(item);
            this.ClearCache(item);
        }

        public abstract void Update(string sqlCondition, params object[] args);

        public void Initialize(string cacheKey, int cacheTimeOut = 20, CacheItemPriority cachePriority = CacheItemPriority.Default, string scope = "")
        {
            this.Scope = scope;
            this.IsScoped = !string.IsNullOrEmpty(this.Scope);
            this.IsCacheable = !string.IsNullOrEmpty(cacheKey);
            if (this.IsCacheable)
            {
                if (this.IsScoped)
                {
                    cacheKey += "_" + this.Scope + "_{0}";
                }

                this.CacheArgs = new CacheItemArgs(cacheKey, cacheTimeOut, cachePriority);
            }
            else
            {
                this.CacheArgs = null;
            }
        }

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
            return DataUtil.GetPropertyValue<T, TProperty>(item, this.Scope);
        }

        protected abstract void DeleteInternal(T item);

        protected abstract IEnumerable<T> GetInternal();

        protected abstract IPagedList<T> GetPageInternal(int pageIndex, int pageSize);

        protected abstract IEnumerable<T> GetByScopeInternal(object propertyValue);

        protected abstract IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize);

        protected abstract T GetByIdInternal(object id);

        protected abstract void InsertInternal(T item);

        protected abstract void UpdateInternal(T item);

        private void CheckIfScoped()
        {
            if (!this.IsScoped)
            {
                throw new NotSupportedException("This method requires the model to be cacheable and have a cache scope defined");
            }
        }

        private void ClearCache(T item)
        {
            if (this.IsCacheable)
            {
                DataCache.RemoveCache(this.IsScoped
                                          ? string.Format(this.CacheArgs.CacheKey, this.GetScopeValue<object>(item))
                                          : this.CacheArgs.CacheKey);
            }
        }

        private void InitializeInternal()
        {
            var type = typeof(T);
            this.Scope = string.Empty;
            this.IsCacheable = false;
            this.IsScoped = false;
            this.CacheArgs = null;

            var scopeAttribute = DataUtil.GetAttribute<ScopeAttribute>(type);
            if (scopeAttribute != null)
            {
                this.Scope = scopeAttribute.Scope;
            }

            this.IsScoped = !string.IsNullOrEmpty(this.Scope);

            var cacheableAttribute = DataUtil.GetAttribute<CacheableAttribute>(type);
            if (cacheableAttribute != null)
            {
                this.IsCacheable = true;
                var cacheKey = !string.IsNullOrEmpty(cacheableAttribute.CacheKey)
                                ? cacheableAttribute.CacheKey
                                : string.Format("OR_{0}", type.Name);
                var cachePriority = cacheableAttribute.CachePriority;
                var cacheTimeOut = cacheableAttribute.CacheTimeOut;

                if (this.IsScoped)
                {
                    cacheKey += "_" + this.Scope + "_{0}";
                }

                this.CacheArgs = new CacheItemArgs(cacheKey, cacheTimeOut, cachePriority);
            }
        }
    }
}
