#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Data;

using DotNetNuke.Data;
using DotNetNuke.Services.Localization;

#endregion

namespace Dnn.Modules.Dashboard.Data
{
    public class DataService
    {
        private static readonly DataProvider provider = DataProvider.Instance();
        private static string moduleQualifier = "Dashboard_";

        private static string GetFullyQualifiedName(string name)
        {
            return String.Concat(moduleQualifier, name);
        }

        public static int AddDashboardControl(int packageId, string dashboardControlKey, bool isEnabled, string dashboardControlSrc, string dashboardControlLocalResources, string controllerClass,
                                              int viewOrder)
        {
            return provider.ExecuteScalar<int>(GetFullyQualifiedName("AddControl"),
                                               packageId,
                                               dashboardControlKey,
                                               isEnabled,
                                               dashboardControlSrc,
                                               dashboardControlLocalResources,
                                               controllerClass,
                                               viewOrder);
        }

        public static void DeleteDashboardControl(int dashboardControlId)
        {
            provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteControl"), dashboardControlId);
        }

        public static IDataReader GetDashboardControlByKey(string dashboardControlKey)
        {
            return provider.ExecuteReader(GetFullyQualifiedName("GetDashboardControlByKey"), dashboardControlKey);
        }

        public static IDataReader GetDashboardControlByPackageId(int packageId)
        {
            return provider.ExecuteReader(GetFullyQualifiedName("GetDashboardControlByPackageId"), packageId);
        }

        public static IDataReader GetDashboardControls(bool isEnabled)
        {
            return provider.ExecuteReader(GetFullyQualifiedName("GetControls"), isEnabled);
        }

        public static IDataReader GetDbInfo()
        {
            return provider.ExecuteReader(GetFullyQualifiedName("GetDbInfo"));
        }

        public static IDataReader GetDbFileInfo()
        {
            return provider.ExecuteReader(GetFullyQualifiedName("GetDbFileInfo"));
        }

        public static IDataReader GetDbBackups()
        {
            return provider.ExecuteReader(GetFullyQualifiedName("GetDbBackups"));
        }

        public static IDataReader GetPortals()
        {
            string cultureCode = Localization.SystemLocale;
            return provider.GetPortals(cultureCode);
        }

        public static IDataReader GetServerErrors()
        {
            return provider.ExecuteReader(GetFullyQualifiedName("GetServerErrors"));
        }

        public static void UpdateDashboardControl(int dashboardControlId, string dashboardControlKey, bool isEnabled, string dashboardControlSrc, string dashboardControlLocalResources,
                                                  string controllerClass, int viewOrder)
        {
            provider.ExecuteNonQuery(GetFullyQualifiedName("UpdateControl"),
                                     dashboardControlId,
                                     dashboardControlKey,
                                     isEnabled,
                                     dashboardControlSrc,
                                     dashboardControlLocalResources,
                                     controllerClass,
                                     viewOrder);
        }
    }
}