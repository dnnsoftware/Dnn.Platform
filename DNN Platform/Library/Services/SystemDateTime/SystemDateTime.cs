// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.SystemDateTime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>
    /// The SystemDateTime provides static method to obtain System's Time.
    /// </summary>
    /// <remarks>
    /// DateTime information is collected from Database. The methods are created to find one unified timestamp from database
    /// as opposed to depending on web server's timestamp. This method becomes more relevant in a web farm configuration.
    /// </remarks>
    public class SystemDateTime
    {
        private static readonly DataProvider Provider = DataProvider.Instance();

        [Obsolete("Deprecated in DNN 7.1.2.  Replaced by DateUtils.GetDatabaseUtcTime, which includes caching. Scheduled removal in v10.0.0.")]
        public static DateTime GetCurrentTimeUtc()
        {
            return Provider.GetDatabaseTimeUtc();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCurrentTime get current time from database.
        /// </summary>
        /// <returns>DateTime.</returns>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in DNN 9.1.0.  Replaced by DateUtils.GetDatabaseLocalTime, which includes caching. Scheduled removal in v11.0.0.")]
        public static DateTime GetCurrentTime()
        {
            return Provider.GetDatabaseTime();
        }
    }
}
