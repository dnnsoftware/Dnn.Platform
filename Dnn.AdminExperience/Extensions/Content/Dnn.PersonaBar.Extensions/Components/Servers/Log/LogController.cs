// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Framework.Providers;

namespace Dnn.PersonaBar.Servers.Components.Log
{
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
