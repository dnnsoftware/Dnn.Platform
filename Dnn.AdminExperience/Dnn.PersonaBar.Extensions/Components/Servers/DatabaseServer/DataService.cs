// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    using System;
    using System.Data;

    using DotNetNuke.Data;
    using DotNetNuke.Services.Localization;

    public class DataService
    {
        private static readonly DataProvider Provider = DataProvider.Instance();
        private static string moduleQualifier = "PersonaBar_";

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

        private static string GetFullyQualifiedName(string name)
        {
            return String.Concat(moduleQualifier, name);
        }
    }
}
