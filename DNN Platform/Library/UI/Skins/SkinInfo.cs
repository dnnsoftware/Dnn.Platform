// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : SkinInfo
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Handles the Business Object for Skins.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class SkinInfo
    {
        private int _PortalId;
        private int _SkinId;
        private string _SkinRoot;
        private string _SkinSrc;
        private SkinType _SkinType;

        public int SkinId
        {
            get
            {
                return this._SkinId;
            }

            set
            {
                this._SkinId = value;
            }
        }

        public int PortalId
        {
            get
            {
                return this._PortalId;
            }

            set
            {
                this._PortalId = value;
            }
        }

        public string SkinRoot
        {
            get
            {
                return this._SkinRoot;
            }

            set
            {
                this._SkinRoot = value;
            }
        }

        public SkinType SkinType
        {
            get
            {
                return this._SkinType;
            }

            set
            {
                this._SkinType = value;
            }
        }

        public string SkinSrc
        {
            get
            {
                return this._SkinSrc;
            }

            set
            {
                this._SkinSrc = value;
            }
        }
    }
}
