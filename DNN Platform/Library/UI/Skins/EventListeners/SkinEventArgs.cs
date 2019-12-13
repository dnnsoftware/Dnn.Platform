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
