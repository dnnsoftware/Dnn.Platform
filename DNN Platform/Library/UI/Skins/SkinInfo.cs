// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

#endregion

namespace DotNetNuke.UI.Skins
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : SkinInfo
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Handles the Business Object for Skins
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
                return _SkinId;
            }
            set
            {
                _SkinId = value;
            }
        }

        public int PortalId
        {
            get
            {
                return _PortalId;
            }
            set
            {
                _PortalId = value;
            }
        }

        public string SkinRoot
        {
            get
            {
                return _SkinRoot;
            }
            set
            {
                _SkinRoot = value;
            }
        }

        public SkinType SkinType
        {
            get
            {
                return _SkinType;
            }
            set
            {
                _SkinType = value;
            }
        }

        public string SkinSrc
        {
            get
            {
                return _SkinSrc;
            }
            set
            {
                _SkinSrc = value;
            }
        }
    }
}
