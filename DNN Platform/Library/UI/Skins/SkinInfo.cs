#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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

        [Obsolete("Replaced in DNN 5.0 by SkinController.RootSkin")]
        public static string RootSkin
        {
            get
            {
                return "Skins";
            }
        }

        [Obsolete("Replaced in DNN 5.0 by SkinController.RootContainer")]
        public static string RootContainer
        {
            get
            {
                return "Containers";
            }
        }
    }
}
