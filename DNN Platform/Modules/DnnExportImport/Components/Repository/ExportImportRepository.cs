#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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
using System.Linq.Expressions;
using System.Threading;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Interfaces;
using LiteDB;

namespace Dnn.ExportImport.Components.Repository
{
    public class ExportImportRepository : IExportImportRepository
    {
        private LiteDatabase _lightDb;

        public ExportImportRepository(string dbFileName)
        {
            _lightDb = new LiteDatabase(dbFileName);
        }

        ~ExportImportRepository()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            var temp = Interlocked.Exchange(ref _lightDb, null);
            temp?.Dispose();
            if (isDisposing)
                GC.SuppressFinalize(this);
        }

        public T AddSingleItem<T>(T item) where T : class
        {
            var collection = DbCollection<T>();
            collection.Insert(item);
            return item;
        }

        public T GetSingleItem<T>() where T : class
        {
            var collection = DbCollection<T>();
            var first = collection.Min();
            return collection.FindById(first);
        }

        public T CreateItem<T>(T item, int? referenceId) where T : BasicExportImportDto
        {
            if (item == null) return null;
            var collection = DbCollection<T>();
            //collection.EnsureIndex(x => x.Id, true);
            item.ReferenceId = referenceId;
            item.Id = collection.Insert(item);
            if (referenceId.HasValue)
            {
                collection.EnsureIndex(x => x.ReferenceId);
            }
            collection.EnsureIndex(x => x.Id, true);
            return item;
        }

        public IEnumerable<T> CreateItems<T>(IEnumerable<T> items, int? referenceId) where T : BasicExportImportDto
        {
            var allItems = items.ToList();
            var collection = DbCollection<T>();
            //collection.EnsureIndex(x => x.Id, true);
            foreach (var item in allItems)
            {
                item.ReferenceId = referenceId;
                item.Id = collection.Insert(item);
                //collection.EnsureIndex(x => x.Id, true);
            }
            collection.EnsureIndex(x => x.Id, true);
            return allItems;
        }

        public T GetItem<T>(Expression<Func<T, bool>> predicate) where T : BasicExportImportDto
        {
            return InternalGetItems(predicate).FirstOrDefault();
        }

        public IEnumerable<T> GetItems<T>(Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return InternalGetItems(predicate, orderKeySelector, asc, skip, max);
        }
        
        public int GetCount<T>() where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            return collection?.Count() ?? 0;
        }
        public int GetCount<T>(Expression<Func<T, bool>> predicate) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            return collection?.Count(predicate) ?? 0;
        }

        public IEnumerable<T> GetAllItems<T>(
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return InternalGetItems(null, orderKeySelector, asc, skip, max);
        }

        private IEnumerable<T> InternalGetItems<T>(Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            var result = predicate != null ? collection.Find(predicate) : collection.FindAll();

            if (orderKeySelector != null)
                result = asc ? result.OrderBy(orderKeySelector) : result.OrderByDescending(orderKeySelector);
            else
                result = result.OrderBy(x => x.Id);

            if (skip != null)
                result = result.Skip(skip.Value);

            if (max != null)
                result = result.Take(max.Value);

            return result.AsEnumerable();
        }

        public T GetItem<T>(int id) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            return collection.FindById(id);
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<int> idList)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => idList.Contains(p.Id);
            return InternalGetItems(predicate);
        }

        public IEnumerable<T> GetRelatedItems<T>(int referenceId)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => p.ReferenceId == referenceId;
            return InternalGetItems(predicate);
        }

        public void UpdateItem<T>(T item) where T : BasicExportImportDto
        {
            if (item == null) return;
            var collection = DbCollection<T>();
            //collection.EnsureIndex(x => x.Id, true);
            if (collection.FindById(item.Id) == null) throw new KeyNotFoundException();
            collection.Update(item);
            //collection.EnsureIndex(x => x.Id, true);
            if (item.ReferenceId.HasValue)
            {
                collection.EnsureIndex(x => x.ReferenceId);
            }
            collection.EnsureIndex(x => x.Id, true);
        }

        public void UpdateItems<T>(IEnumerable<T> items) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            //collection.EnsureIndex(x => x.Id, true);
            foreach (var item in items)
            {
                if (collection.FindById(item.Id) == null) throw new KeyNotFoundException();
                collection.Update(item);
                //collection.EnsureIndex(x => x.Id, true);
                if (item.ReferenceId.HasValue)
                {
                    collection.EnsureIndex(x => x.ReferenceId);
                }
            }
            collection.EnsureIndex(x => x.Id, true);
        }

        public bool DeleteItem<T>(int id) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            //collection.EnsureIndex(x => x.Id, true);
            var item = collection.FindById(id);
            if (item == null) throw new KeyNotFoundException();
            return collection.Delete(id);
        }

        private LiteCollection<T> DbCollection<T>()
        {
            return _lightDb.GetCollection<T>(typeof(T).Name/*.ToLowerInvariant()*/);
        }
    }
}
