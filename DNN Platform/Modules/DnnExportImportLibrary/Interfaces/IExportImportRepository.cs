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
using System.Linq.Expressions;
using Dnn.ExportImport.Dto;

namespace Dnn.ExportImport.Interfaces
{
    public interface IExportImportRepository : IDisposable
    {
        T AddSingleItem<T>(T item) where T : class;
        T GetSingleItem<T>() where T : class;

        T CreateItem<T>(T item, int? referenceId) where T : BasicExportImportDto;

        void CreateItems<T>(IEnumerable<T> items, int? referenceId)
            where T : BasicExportImportDto;

        IEnumerable<T> GetItems<T>(Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto;

        IEnumerable<T> GetAllItems<T>(
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto;

        int GetCount<T>() where T : BasicExportImportDto;

        void RebuildIndex<T>(Expression<Func<T, object>> predicate, bool unique = false) where T : BasicExportImportDto;

        int GetCount<T>(Expression<Func<T, bool>> predicate) where T : BasicExportImportDto;
        T GetItem<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto;

        T GetItem<T>(int id) where T : BasicExportImportDto;

        IEnumerable<T> GetItems<T>(IEnumerable<int> idList)
            where T : BasicExportImportDto;

        IEnumerable<T> GetRelatedItems<T>(int referenceId)
            where T : BasicExportImportDto;

        void UpdateItem<T>(T item) where T : BasicExportImportDto;

        void UpdateItems<T>(IEnumerable<T> items) where T : BasicExportImportDto;

        bool DeleteItem<T>(int id) where T : BasicExportImportDto;
    }
}