#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Generic;
using System.IO;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Modules.Dashboard.Components.Skins
{
    public class SkinsController : IDashboardData
    {
        #region IDashboardData Members

        public void ExportData(XmlWriter writer)
        {
			//Write start of Installed Skins 
            writer.WriteStartElement("installedSkins");
			
			//Iterate through Installed Skins 
            foreach (SkinInfo skin in GetInstalledSkins())
            {
                skin.WriteXml(writer);
            }
			
			//Write end of Installed Skins 
            writer.WriteEndElement();
        }

        #endregion

        private static bool isFallbackSkin(string skinPath)
        {
            SkinDefaults defaultSkin = SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
            string defaultSkinPath = (Globals.HostMapPath + SkinController.RootSkin + defaultSkin.Folder).Replace("/", "\\");
            if (defaultSkinPath.EndsWith("\\"))
            {
                defaultSkinPath = defaultSkinPath.Substring(0, defaultSkinPath.Length - 1);
            }
            return skinPath.IndexOf(defaultSkinPath, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        public static List<SkinInfo> GetInstalledSkins()
        {
            var list = new List<SkinInfo>();
            foreach (string folder in Directory.GetDirectories(Path.Combine(Globals.HostMapPath, "Skins")))
            {
                if (!folder.EndsWith(Globals.glbHostSkinFolder))
                {
                    var skin = new SkinInfo();
                    skin.SkinName = folder.Substring(folder.LastIndexOf("\\") + 1);
                    skin.InUse = isFallbackSkin(folder) || !SkinController.CanDeleteSkin(folder, "");
                    list.Add(skin);
                }
            }
            return list;
        }
    }
}