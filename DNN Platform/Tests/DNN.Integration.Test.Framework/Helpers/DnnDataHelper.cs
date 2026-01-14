// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;

    public static class DnnDataHelper
    {
        private static int? _portalId;

        public static int PortalId
        {
            get
            {
                if (!_portalId.HasValue)
                {
                    var alias = AppConfigHelper.SiteUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
                    if (alias.EndsWith("/", StringComparison.Ordinal))
                    {
                        alias = alias.Substring(0, alias.Length - 1);
                    }

                    var query = $"SELECT TOP(1) PortalID FROM {{objectQualifier}}PortalAlias WHERE HTTPAlias='{alias}'";
                    var id = DatabaseHelper.ExecuteScalar<int>(query);
                    _portalId = id;
                }

                return _portalId.Value;
            }
        }
    }
}
