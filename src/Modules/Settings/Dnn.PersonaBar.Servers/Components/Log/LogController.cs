#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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