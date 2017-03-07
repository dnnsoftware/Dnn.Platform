using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dnn.ExportImport.Components.Dto;

namespace Dnn.ExportImport.Components.Interfaces
{
    public interface IExportImportRepository : IDisposable
    {
        T AddSingleItem<T>(T item) where T : class;
        T GetSingleItem<T>() where T : class;

        T CreateItem<T>(T item, int? referenceId) where T : BasicExportImportDto;

        IEnumerable<T> CreateItems<T>(IEnumerable<T> items, int? referenceId)
            where T : BasicExportImportDto;

        IEnumerable<T> GetItems<T>(Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto;

        IEnumerable<T> GetAllItems<T>(
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto;

        int GetCount<T>() where T : BasicExportImportDto;

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