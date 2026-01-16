// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions.TabActions
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// TabActions base class.
    /// </summary>
    internal abstract class TabActionBase
    {
        /// <summary>
        /// Removes the cache for the tab.
        /// </summary>
        /// <param name="stateTransaction">The state transaction.</param>
        internal static void RemoveCache(StateTransaction stateTransaction)
        {
            var tab = GetTab(stateTransaction.ContentItemId);
            if (tab == null)
            {
                return;
            }

            DataCache.RemoveCache($"Tab_Tabs{tab.PortalID}");
        }

        /// <summary>
        /// Gets the content item by content item ID.
        /// </summary>
        /// <param name="contentItemId">The content item ID.</param>
        /// <returns>The content item.</returns>
        internal static ContentItem GetContentItem(int contentItemId)
            => Util.GetContentController().GetContentItem(contentItemId);

        /// <summary>
        /// Gets the tab information by content item ID.
        /// </summary>
        /// <param name="contentItemId">The content item ID.</param>
        /// <returns>The tab information.</returns>
        internal static TabInfo GetTab(int contentItemId)
        {
            const string Sql = "SELECT * FROM dbo.vw_Tabs WHERE ContentItemId = @ContentItemId";

            IDataParameter[] parameters =
            [
                new SqlParameter("@ContentItemId", SqlDbType.Int) { Value = contentItemId },
            ];

            return CBO.FillObject<TabInfo>(DataProvider.Instance().ExecuteSQL(Sql, parameters));
        }

        /// <summary>
        /// Gets the localized string value for the specified key.
        /// </summary>
        /// <param name="key">The key of the string to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the string is not found.</param>
        /// <returns>The localized string value.</returns>
        internal static string GetString(string key, string defaultValue)
        {
            var content = Localization.GetString(key, Localization.GlobalResourceFile);
            return string.IsNullOrEmpty(content) ? defaultValue : content;
        }

        /// <summary>
        /// Gets the formatted localized string value for the specified key for parameters.
        /// </summary>
        /// <param name="key">The key of the string to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the string is not found.</param>
        /// <param name="params">The parameters to format the string.</param>
        /// <returns>The formatted localized string value.</returns>
        internal static string GetString(string key, string defaultValue, params object[] @params)
            => string.Format(CultureInfo.CurrentCulture, GetString(key, defaultValue), @params);
    }
}
