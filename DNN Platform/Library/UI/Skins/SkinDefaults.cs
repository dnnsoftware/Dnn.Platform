// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Globalization;
    using System.Xml;

    using DotNetNuke.Common.Utilities;

    public enum SkinDefaultType
    {
        /// <summary>Skin.</summary>
        SkinInfo = 0,

        /// <summary>Container.</summary>
        ContainerInfo = 1,
    }

    [Serializable]
    public class SkinDefaults
    {
        private string adminDefaultName;
        private string defaultName;
        private string folder;

        private SkinDefaults(SkinDefaultType defaultType)
        {
            string nodename = Enum.GetName(defaultType.GetType(), defaultType).ToLowerInvariant();
            string filePath = Config.GetPathToFile(Config.ConfigFileType.DotNetNuke);
            var dnndoc = new XmlDocument { XmlResolver = null };
            dnndoc.Load(filePath);
            XmlNode defaultElement = dnndoc.SelectSingleNode("/configuration/skinningdefaults/" + nodename);
            this.folder = defaultElement.Attributes["folder"].Value;
            this.defaultName = defaultElement.Attributes["default"].Value;
            this.adminDefaultName = defaultElement.Attributes["admindefault"].Value;
        }

        public string AdminDefaultName
        {
            get
            {
                return this.adminDefaultName;
            }

            set
            {
                this.adminDefaultName = value;
            }
        }

        public string DefaultName
        {
            get
            {
                return this.defaultName;
            }

            set
            {
                this.defaultName = value;
            }
        }

        public string Folder
        {
            get
            {
                return this.folder;
            }

            set
            {
                this.folder = value;
            }
        }

        public static SkinDefaults GetSkinDefaults(SkinDefaultType defaultType)
        {
            return
                CBO.GetCachedObject<SkinDefaults>(
                    new CacheItemArgs(string.Format(CultureInfo.InvariantCulture, DataCache.SkinDefaultsCacheKey, defaultType), DataCache.SkinDefaultsCacheTimeOut, DataCache.SkinDefaultsCachePriority, defaultType),
                    GetSkinDefaultsCallback);
        }

        private static object GetSkinDefaultsCallback(CacheItemArgs cacheItemArgs)
        {
            var defaultType = (SkinDefaultType)cacheItemArgs.ParamList[0];
            return new SkinDefaults(defaultType);
        }
    }
}
