// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.EventListeners
{
    using System;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SkinEventArgs provides a custom EventARgs class for Skin Events.
    /// </summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public class SkinEventArgs : EventArgs
    {
        private readonly Skin _Skin;

        public SkinEventArgs(Skin skin)
        {
            this._Skin = skin;
        }

        public Skin Skin
        {
            get
            {
                return this._Skin;
            }
        }
    }
}
