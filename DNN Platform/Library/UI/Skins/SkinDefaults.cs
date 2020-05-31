// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            string nodename = Enum.GetName(DefaultType.GetType(), DefaultType).ToLowerInvariant();
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
