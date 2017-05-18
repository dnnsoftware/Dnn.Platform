// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

using System;
using DotNetNuke.Tests.Integration.Framework.Helpers;
using DotNetNuke.Tests.Integration.Framework.Scripts;

namespace DotNetNuke.Tests.Integration.Framework.Controllers
{
    public static class SolutionController
    {
        private const string TableNameMarker = @"$[table_name]";

        private static bool? _isSocial;
        private static bool? _isEnterprise;
        private static bool? _isContent;
        /// <summary>
        /// Is the Solution package Social
        /// </summary>
        public static bool IsSocial()
        {
            if (! _isSocial.HasValue)
            {
                var script = SqlScripts.TableExist.Replace(TableNameMarker, "Ideas_Idea")
                    .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier);

                var result = DatabaseHelper.ExecuteQuery(script);

                _isSocial = Convert.ToBoolean(result[0][""]);
            }
            return  (bool) _isSocial;
        }

        /// <summary>
        /// Is the Solution package Enterprise
        /// </summary>
        /// <returns></returns>
        public static bool IsEnterprise()
        {
            if (!_isEnterprise.HasValue)
            {
                var script = SqlScripts.TableExist.Replace(TableNameMarker, "SPConnector_Connections")
                    .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier);

                var result = DatabaseHelper.ExecuteQuery(script);

                _isEnterprise = Convert.ToBoolean(result[0][""]) && !IsSocial();
            }
            return (bool)_isEnterprise;
        }

        /// <summary>
        /// Is the Solution package Content
        /// </summary>
        /// <returns></returns>
        public static bool IsContent()
        {
            if (!_isContent.HasValue)
            {
                var script = SqlScripts.TableExist.Replace(TableNameMarker, "ContentLayouts")
                    .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier);

                var result = DatabaseHelper.ExecuteQuery(script);

                _isContent = Convert.ToBoolean(result[0][""]) && !IsSocial() && !IsEnterprise();
            }
            return (bool)_isContent;
        }
    }
}
