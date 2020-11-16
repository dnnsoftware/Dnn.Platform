// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DNN.Integration.Test.Framework.Helpers;
    using DNN.Integration.Test.Framework.Scripts;

    public static class PortalController
    {
        private const string PortalIdMarker = @"'$[portal_id]'";

        public static TimeZoneInfo GetTimeZone(int portalId)
        {
            const string fileContent = SqlScripts.PortalGetTimeZone;
            var script = new StringBuilder(fileContent)
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var timeZoneString = DatabaseHelper.ExecuteQuery(script).First()["SettingValue"].ToString();

            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneString);
        }
    }
}
