// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.Log
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Services.FileSystem;

    public class LogController
    {
        public List<LogFile> GetLogFilesList()
        {
            var logPath = Globals.ApplicationMapPath + @"\portals\_default\logs";
            return GetLogList(logPath, "*.resources");
        }

        public List<LogFile> GetUpgradeLogList()
        {
            var objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration("data");
            var strProviderPath = DataProvider.Instance().GetProviderPath();
            return GetLogList(strProviderPath, "*.log.resources");
        }

        private static List<LogFile> GetLogList(string logPath, string searchPattern)
        {
            var fileSizeFormatProvider = new FileSizeFormatProvider();
            return (
                from file in new DirectoryInfo(logPath).EnumerateFiles(searchPattern)
                select new LogFile
                {
                    Name = file.Name,
                    LastWriteTimeUtc = file.LastWriteTimeUtc,
                    Size = string.Format(fileSizeFormatProvider, "{0:fs}", file.Length),
                })
                .ToList();
        }
    }
}
