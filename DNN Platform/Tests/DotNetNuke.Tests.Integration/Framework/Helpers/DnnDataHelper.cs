﻿// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2015, DNN Corp.
// All Rights Reserved

namespace DotNetNuke.Tests.Integration.Framework.Helpers
{
    public static class DnnDataHelper
    {
        private static int? _portalId;

        public static int PortalId
        {
            get
            {
                if (!_portalId.HasValue)
                {
                    var alias = AppConfigHelper.SiteUrl.Replace("http://", "").Replace("https://", "");
                    if (alias.EndsWith("/"))
                        alias = alias.Substring(0, alias.Length - 1);
                    var query = string.Format(
                        "SELECT TOP(1) PortalID FROM {{objectQualifier}}PortalAlias WHERE HTTPAlias='{0}'", alias);
                    var id = DatabaseHelper.ExecuteScalar<int>(query);
                    _portalId = id;
                }
                return _portalId.Value;
            }
        }
    }
}
