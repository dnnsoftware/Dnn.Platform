// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data
{
    using System.Collections.Generic;

    using DotNetNuke.Collections;

    public interface IRepository<T>
        where T : class
    {
        /// <summary>
        /// Delete an Item from the repository.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        void Delete(T item);

        /// <summary>
        /// Delete items from the repository based on a sql Condition.
        /// </summary>
        /// <param name="sqlCondition">The sql condition e.g. "WHERE ArticleId = {0}".</param>
        /// <param name="args">A collection of arguments to be mapped to the tokens in the sqlCondition.</param>
        void Delete(string sqlCondition, params object[] args);

        /// <summary>
        /// Find items from the repository based on a sql condition.
        /// </summary>
        /// <remarks>Find supports both full SQL statements such as "SELECT * FROM table WHERE ..."
        /// as well as a SQL condition like "WHERE ...".</remarks>
        /// <param name="sqlCondition">The sql condition e.g. "WHERE ArticleId = @0".</param>
        /// <param name="args">A collection of arguments to be mapped to the tokens in the sqlCondition.</param>
        /// <example>Find("where ArticleId = @0 and UserId = @1", articleId, userId).</example>
        /// <returns>A list of items.</returns>
        IEnumerable<T> Find(string sqlCondition, params object[] args);

        /// <summary>
        /// Find a GetPage of items from the repository based on a sql condition.
        /// </summary>
        /// <remarks>Find supports both full SQL statements such as "SELECT * FROM table WHERE ..."
        /// as well as a SQL condition like "WHERE ...".</remarks>
        /// <param name="pageIndex">The page Index to fetch.</param>
        /// <param name="pageSize">The size of the page to fetch.</param>
        /// <param name="sqlCondition">The sql condition e.g. "WHERE ArticleId = @0".</param>
        /// <param name="args">A collection of arguments to be mapped to the tokens in the sqlCondition.</param>
        /// <returns>A list of items.</returns>
        IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args);

        /// <summary>
        /// Returns all the items in the repository as an enumerable list.
        /// </summary>
        /// <returns>The list of items.</returns>
        IEnumerable<T> Get();

        /// <summary>
        /// Returns an enumerable list of items filtered by scope.
        /// </summary>
        /// <remarks>
        /// This overload should be used to get a list of items for a specific module
        /// instance or for a specific portal dependening on how the items in the repository
        /// are scoped.
        /// </remarks>
        /// <typeparam name="TScopeType">The type of the scope field.</typeparam>
        /// <param name="scopeValue">The value of the scope to filter by.</param>
        /// <returns>The list of items.</returns>
        IEnumerable<T> Get<TScopeType>(TScopeType scopeValue);

        /// <summary>
        /// Get an individual item based on the items Id field.
        /// </summary>
        /// <typeparam name="TProperty">The type of the Id field.</typeparam>
        /// <param name="id">The value of the Id field.</param>
        /// <returns>An item.</returns>
        T GetById<TProperty>(TProperty id);

        /// <summary>
        /// Get an individual item based on the items Id field.
        /// </summary>
        /// <remarks>
        /// This overload should be used to get an item for a specific module
        /// instance or for a specific portal dependening on how the items in the repository
        /// are scoped. This will allow the relevant cache to be searched first.
        /// </remarks>
        /// <typeparam name="TProperty">The type of the Id field.</typeparam>
        /// <param name="id">The value of the Id field.</param>
        /// <typeparam name="TScopeType">The type of the scope field.</typeparam>
        /// <param name="scopeValue">The value of the scope to filter by.</param>
        /// <returns>An item.</returns>
        T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue);

        /// <summary>
        /// Returns a page of items in the repository as a paged list.
        /// </summary>
        /// <param name="pageIndex">The page Index to fetch.</param>
        /// <param name="pageSize">The size of the page to fetch.</param>
        /// <returns>The list of items.</returns>
        IPagedList<T> GetPage(int pageIndex, int pageSize);

        /// <summary>
        /// Returns a page of items in the repository as a paged list filtered by scope.
        /// </summary>
        /// <remarks>
        /// This overload should be used to get a list of items for a specific module
        /// instance or for a specific portal dependening on how the items in the repository
        /// are scoped.
        /// </remarks>
        /// <typeparam name="TScopeType">The type of the scope field.</typeparam>
        /// <param name="scopeValue">The value of the scope to filter by.</param>
        /// <param name="pageIndex">The page Index to fetch.</param>
        /// <param name="pageSize">The size of the page to fetch.</param>
        /// <returns>The list of items.</returns>
        IPagedList<T> GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize);

        /// <summary>
        /// Inserts an Item into the repository.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        void Insert(T item);

        /// <summary>
        /// Updates an Item in the repository.
        /// </summary>
        /// <param name="item">The item to be updated.</param>
        void Update(T item);

        /// <summary>
        /// Update items in the repository based on a sql Condition.
        /// </summary>
        /// <param name="sqlCondition">The sql condition e.g. "SET ArticelName = @1 WHERE ArticleId = @0".</param>
        /// <param name="args">A collection of arguments to be mapped to the tokens in the sqlCondition.</param>
        /// <example>Update("SET Age=@1, Name=@2 WHERE ID=@0", 1, 21, "scooby").</example>
        void Update(string sqlCondition, params object[] args);
    }
}
