#region Copyright
// DNN® and DotNetNuke® - http://www.DNNSoftware.com
// Copyright ©2002-2019
// by DNN Corp
// All Rights Reserved
#endregion

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
