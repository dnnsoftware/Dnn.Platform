// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Data;
using DotNetNuke.Data;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    public class DataService
    {
        private static readonly DataProvider Provider = DataProvider.Instance();
        private static string moduleQualifier = "PersonaBar_";

        private static string GetFullyQualifiedName(string name)
        {
            return String.Concat(moduleQualifier, name);
        }

        public static IDataReader GetDbInfo()
        {
            return Provider.ExecuteReader(GetFullyQualifiedName("GetDbInfo"));
        }

        public static IDataReader GetDbFileInfo()
        {
            return Provider.ExecuteReader(GetFullyQualifiedName("GetDbFileInfo"));
        }

        public static IDataReader GetDbBackups()
        {
            return Provider.ExecuteReader(GetFullyQualifiedName("GetDbBackups"));
        }
    }
}
