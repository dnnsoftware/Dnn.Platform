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

// Influenced by http://sameproblemmorecode.blogspot.nl/2013/07/petapoco-as-its-meant-to-be-with.html
// https://github.com/luuksommers/PetaPoco

using System;
using System.Linq.Expressions;
using System.Web.Caching;
using PetaPoco;

namespace DotNetNuke.Data.PetaPoco
{
    public static class FluentMapperExtensions
    {
        public static FluentMapper<TModel> AddFluentMapper<TModel>(this IDataContext dataContext)
        {
            var petaPocoDataContext = dataContext as PetaPocoDataContext;
            FluentMapper<TModel> mapper = null;

            // ReSharper disable once UseNullPropagation
            if (petaPocoDataContext != null)
            {
                mapper = new FluentMapper<TModel>(petaPocoDataContext.TablePrefix);
                petaPocoDataContext.FluentMappers.Add(typeof(TModel), mapper);
            }

            return mapper;
        }

        public static FluentMapper<TModel> Cache<TModel>(this FluentMapper<TModel> mapper, string cacheKey, int timeOut = 20, CacheItemPriority priority = CacheItemPriority.Default)
        {
            mapper.CacheKey = cacheKey;
            mapper.CacheTimeOut = timeOut;
            mapper.CachePriority = priority;
            return mapper;
        }

        public static FluentMapper<TModel> Property<TModel, TProperty>(this FluentMapper<TModel> mapper, Expression<Func<TModel, TProperty>> action, string column, bool primaryKey = false, bool readOnly = false) where TModel : class
        {
            return mapper.Property(action, column, null, primaryKey, readOnly);
        }

        public static FluentMapper<TModel> Property<TModel, TProperty>(this FluentMapper<TModel> mapper, Expression<Func<TModel, TProperty>> action, string column, Func<object, object> fromDbConverter, bool primaryKey = false, bool readOnly = false) where TModel : class
        {
            return mapper.Property(action, column, fromDbConverter, null, primaryKey, readOnly);
        }

        public static FluentMapper<TModel> Property<TModel, TProperty>(this FluentMapper<TModel> mapper, Expression<Func<TModel, TProperty>> action, string column, Func<object, object> fromDbConverter, Func<object, object> toDbConverter, bool primaryKey = false, bool readOnly = false) where TModel : class
        {
            var expression = (MemberExpression)action.Body;
            string name = expression.Member.Name;

            mapper.Mappings.Add(name, new FluentColumnMap(new ColumnInfo() { ColumnName = column, ResultColumn = readOnly }, fromDbConverter, toDbConverter));

            if (primaryKey)
            {
                if (!string.IsNullOrEmpty(mapper.TableInfo.PrimaryKey))
                    mapper.TableInfo.PrimaryKey += ",";
                mapper.TableInfo.PrimaryKey += column;
            }
            return mapper;
        }

        public static FluentMapper<TModel> PrimaryKey<TModel>(this FluentMapper<TModel> mapper, string primaryKey, bool autoIncrement = true)
        {
            mapper.TableInfo.PrimaryKey = primaryKey;
            mapper.TableInfo.AutoIncrement = autoIncrement;

            return mapper;
        }

        public static FluentMapper<TModel> Scope<TModel>(this FluentMapper<TModel> mapper, string scope)
        {
            mapper.Scope = scope;
            return mapper;
        }

        public static FluentMapper<TModel> TableName<TModel>(this FluentMapper<TModel> mapper, string tableName)
        {
            mapper.TableInfo.TableName = mapper.GetTablePrefix() + tableName;

            return mapper;
        }
    }
}
