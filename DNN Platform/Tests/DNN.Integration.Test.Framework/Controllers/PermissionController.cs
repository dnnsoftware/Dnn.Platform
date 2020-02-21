// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Linq;
using DNN.Integration.Test.Framework.Helpers;

namespace DNN.Integration.Test.Framework.Controllers
{
    public class PermissionController
    {
        public static dynamic GetPermission(string code, string key)
        {
            const string query =
                @"SELECT * 
                  FROM {{objectQualifier}}Permission
                  WHERE PermissionCode = '{0}' AND PermissionKey = '{1}'";
            return DatabaseHelper.ExecuteDynamicQuery(string.Format(query, code, key)).SingleOrDefault();
        }
    }
}
