// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DNN.Integration.Test.Framework.Helpers
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
