// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Globalization;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    public enum SkinDefaultType
    {
        /// <summary>Skin.</summary>
        SkinInfo = 0,

        /// <summary>Container.</summary>
        ContainerInfo = 1,
    }

    /// <summary>The skinning defaults config.</summary>
    [Serializable]
    public partial class SkinDefaults
    {
        private string adminDefaultName;
        private string defaultName;
        private string folder;

        private SkinDefaults(SkinDefaultType defaultType)
        {
            string nodename = Enum.GetName(defaultType.GetType(), defaultType).ToLowerInvariant();
            string filePath = Config.GetPathToFile(Config.ConfigFileType.DotNetNuke);
            var dnndoc = new XmlDocument { XmlResolver = null, };
            using (var xmlReader = XmlReader.Create(filePath, new XmlReaderSettings { XmlResolver = null, }))
            {
                dnndoc.Load(xmlReader);
            }

            XmlNode defaultElement = dnndoc.SelectSingleNode("/configuration/skinningdefaults/" + nodename);
            this.folder = defaultElement.Attributes["folder"].Value;
            this.defaultName = defaultElement.Attributes["default"].Value;
            this.adminDefaultName = defaultElement.Attributes["admindefault"].Value;
        }

        public string AdminDefaultName
        {
            get => this.adminDefaultName;
            set => this.adminDefaultName = value;
        }

        public string DefaultName
        {
            get => this.defaultName;
            set => this.defaultName = value;
        }

        public string Folder
        {
            get => this.folder;
            set => this.folder = value;
        }

        /// <summary>Gets the defaults for the <paramref name="defaultType"/>.</summary>
        /// <param name="defaultType">The type.</param>
        /// <returns>A <see cref="SkinDefaults"/> instance.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial SkinDefaults GetSkinDefaults(SkinDefaultType defaultType)
            => GetSkinDefaults(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), defaultType);

        /// <summary>Gets the defaults for the <paramref name="defaultType"/>.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="defaultType">The type.</param>
        /// <returns>A <see cref="SkinDefaults"/> instance.</returns>
        public static SkinDefaults GetSkinDefaults(IHostSettings hostSettings, SkinDefaultType defaultType)
        {
            return
                CBO.GetCachedObject<SkinDefaults>(
                    hostSettings,
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
