#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;

namespace DotNetNuke.Data
{
    /// <summary>
    /// ControllerBase provides some simple CRUD helper methods for implementing the DAL 2 methods
    /// </summary>
    public abstract class ControllerBase
    {
        protected readonly IDataContext DataContext;

        protected ControllerBase() { }

        protected ControllerBase(IDataContext dataContext)
        {
            //Argument Contract
            Requires.NotNull("dataContext", dataContext);

            DataContext = dataContext;
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            //Argument Contract
            Requires.NotNull(entity);

            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                rep.Insert(entity);
            }
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            //Argument Contract
            Requires.NotNull(entity);

            var primaryKey = DataUtil.GetPrimaryKeyProperty(typeof (TEntity), String.Empty);
            Requires.PropertyNotNull(entity, primaryKey);
            Requires.PropertyNotNegative(entity, primaryKey);

            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                rep.Delete(entity);
            }
        }

        public IEnumerable<TEntity> Get<TEntity>() where TEntity : class
        {
            IEnumerable<TEntity> contentTypes;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<TEntity>();

                contentTypes = rep.Get();
            }

            return contentTypes;
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
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
