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
#region Usings

using System;
using System.Xml;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.Skins
{
    public enum SkinDefaultType
    {
        SkinInfo,
        ContainerInfo
    }

    [Serializable]
    public class SkinDefaults
    {
        private string _adminDefaultName;
        private string _defaultName;
        private string _folder;

        private SkinDefaults(SkinDefaultType DefaultType)
        {
            string nodename = Enum.GetName(DefaultType.GetType(), DefaultType).ToLower();
            string filePath = Config.GetPathToFile(Config.ConfigFileType.DotNetNuke);
            var dnndoc = new XmlDocument { XmlResolver = null };
            dnndoc.Load(filePath);
            XmlNode defaultElement = dnndoc.SelectSingleNode("/configuration/skinningdefaults/" + nodename);
            _folder = defaultElement.Attributes["folder"].Value;
            _defaultName = defaultElement.Attributes["default"].Value;
            _adminDefaultName = defaultElement.Attributes["admindefault"].Value;
        }

        public string AdminDefaultName
        {
            get
            {
                return _adminDefaultName;
            }
            set
            {
                _adminDefaultName = value;
            }
        }

        public string DefaultName
        {
            get
            {
                return _defaultName;
            }
            set
            {
                _defaultName = value;
            }
        }

        public string Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                _folder = value;
            }
        }

        private static object GetSkinDefaultsCallback(CacheItemArgs cacheItemArgs)
        {
            var defaultType = (SkinDefaultType) cacheItemArgs.ParamList[0];
            return new SkinDefaults(defaultType);
        }

        public static SkinDefaults GetSkinDefaults(SkinDefaultType DefaultType)
        {
            return
                CBO.GetCachedObject<SkinDefaults>(
                    new CacheItemArgs(string.Format(DataCache.SkinDefaultsCacheKey, DefaultType), DataCache.SkinDefaultsCacheTimeOut, DataCache.SkinDefaultsCachePriority, DefaultType),
                    GetSkinDefaultsCallback);
        }
    }
}