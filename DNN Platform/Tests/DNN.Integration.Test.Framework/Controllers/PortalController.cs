﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using DNN.Integration.Test.Framework.Helpers;
using DNN.Integration.Test.Framework.Scripts;

namespace DNN.Integration.Test.Framework.Controllers
{
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
