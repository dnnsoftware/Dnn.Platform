// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using DotNetNuke.Common;
using DotNetNuke.Framework;

namespace DotNetNuke.Data
{
    public abstract class ControllerBase<TEntity, TContract, TSelf> : ServiceLocator<TContract, TSelf> where TSelf : ServiceLocator<TContract, TSelf>, new() where TEntity : class
    {
        protected readonly IDataContext DataContext;

        protected ControllerBase() { }

        protected ControllerBase(IDataContext dataContext)
        {
            //Argument Contract
            Requires.NotNull("dataContext", dataContext);

            DataContext = dataContext;
        }

        public void Add(TEntity entity)
        {
            //Argument Contract
            Requires.NotNull(entity);

            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                rep.Insert(entity);
            }
        }

        public void Delete(TEntity entity)
        {
            //Argument Contract
            Requires.NotNull(entity);

            var primaryKey = DataUtil.GetPrimaryKeyProperty(typeof(TEntity), String.Empty);
            Requires.PropertyNotNull(entity, primaryKey);
            Requires.PropertyNotNegative(entity, primaryKey);

            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                rep.Delete(entity);
            }
        }

        public IEnumerable<TEntity> Find(string sqlCondition, params object[] args)
        {
            IEnumerable<TEntity> entities;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                entities = rep.Find(sqlCondition, args);
            }

            return entities;

        }

        public IEnumerable<TEntity> Get()
        {
            IEnumerable<TEntity> entities;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                entities = rep.Get();
            }

            return entities;
        }

        public IEnumerable<TEntity> Get<TScope>(TScope scope)
        {
            IEnumerable<TEntity> contentTypes;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                contentTypes = rep.Get(scope);
            }

            return contentTypes;
        }

        public void Update(TEntity entity)
        {
            //Argument Contract
            Requires.NotNull(entity);

            var primaryKey = DataUtil.GetPrimaryKeyProperty(typeof(TEntity), String.Empty);
            Requires.PropertyNotNull(entity, primaryKey);
            Requires.PropertyNotNegative(entity, primaryKey);

            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                rep.Update(entity);
            }
        }
    }
}
