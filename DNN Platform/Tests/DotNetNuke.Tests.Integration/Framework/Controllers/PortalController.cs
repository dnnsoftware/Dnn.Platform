// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using DotNetNuke.Tests.Integration.Framework.Helpers;
using DotNetNuke.Tests.Integration.Framework.Scripts;

namespace DotNetNuke.Tests.Integration.Framework.Controllers
{
    public static class PortalController
    {
        private const string PortalIdMarker = @"'$[portal_id]'";

        public static TimeZoneInfo GetTimeZone(int portalId)
        {
            var fileContent = SqlScripts.PortalGetTimeZone;
            var script = new StringBuilder(fileContent)
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var timeZoneString = DatabaseHelper.ExecuteQuery(script).First()["SettingValue"].ToString();
           
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneString);
        }
    }
}
