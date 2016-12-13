#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Linq;
using DotNetNuke.Tests.Integration.Framework.Helpers;

namespace DotNetNuke.Tests.Integration.Framework.Controllers
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
