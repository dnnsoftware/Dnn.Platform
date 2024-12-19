// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;

    using DotNetNuke.Abstractions.Skins;

    /// Project  : DotNetNuke
    /// Class    : SkinInfo
    ///
    /// <summary>    Handles the Business Object for Skins.</summary>
    [Serializable]
    public class SkinInfo : ISkinInfo
    {
        private int portalId;
        private int skinId;
        private int skinPackageId;
        private string skinRoot;
        private string skinSrc;
        private SkinType skinType;

        public int SkinId
        {
            get
            {
                return this.skinId;
            }

            set
            {
                this.skinId = value;
            }
        }

        public int SkinPackageId
        {
            get
            {
                return this.skinPackageId;
            }

            set
            {
                this.skinPackageId = value;
            }
        }

        public int PortalId
        {
            get
            {
                return this.portalId;
            }

            set
            {
                this.portalId = value;
            }
        }

        public string SkinRoot
        {
            get
            {
                return this.skinRoot;
            }

            set
            {
                this.skinRoot = value;
            }
        }

        [Obsolete("Deprecated in DotNetNuke 10.0.0. No replacement. Scheduled removal in v12.0.0.")]
        public SkinType SkinType
        {
            get
            {
                return this.skinType;
            }

            set
            {
                this.skinType = value;
            }
        }

        public string SkinSrc
        {
            get
            {
                return this.skinSrc;
            }

            set
            {
                this.skinSrc = value;
            }
        }
    }
}
