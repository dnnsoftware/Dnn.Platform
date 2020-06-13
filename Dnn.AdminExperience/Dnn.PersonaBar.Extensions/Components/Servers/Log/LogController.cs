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

    public class LogController
    {
        public List<string> GetLogFilesList()
        {
            var files = Directory.GetFiles(Globals.ApplicationMapPath + @"\portals\_default\logs", "*.resources");
            var fileList = (from file in files select Path.GetFileName(file)).ToList();
            return fileList;
        }

        public ArrayList GetUpgradeLogList()
        {
            var objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration("data");
            var strProviderPath = DataProvider.Instance().GetProviderPath();
            var arrScriptFiles = new ArrayList();
            var arrFiles = Directory.GetFiles(strProviderPath, "*." + objProviderConfiguration.DefaultProvider);
            foreach (var strFile in arrFiles)
            {
                arrScriptFiles.Add(Path.GetFileNameWithoutExtension(strFile));
            }
            arrScriptFiles.Sort();
            return arrScriptFiles;
        }
    }
}
