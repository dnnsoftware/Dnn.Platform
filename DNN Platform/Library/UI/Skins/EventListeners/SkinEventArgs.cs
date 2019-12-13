// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.UI.Skins.EventListeners
{
    ///-----------------------------------------------------------------------------
    /// <summary>
    /// SkinEventArgs provides a custom EventARgs class for Skin Events
    /// </summary>
    /// <remarks></remarks>
    ///-----------------------------------------------------------------------------
    public class SkinEventArgs : EventArgs
    {
        private readonly Skin _Skin;

        public SkinEventArgs(Skin skin)
        {
            _Skin = skin;
        }

        public Skin Skin
        {
            get
            {
                return _Skin;
            }
        }
    }
}
