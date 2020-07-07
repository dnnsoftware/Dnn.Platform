// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.AppEvents
{
    using DotNetNuke.UI.Skins.EventListeners;

    /// <summary>
    /// This interface defines methods that need to be called in skins lifecycle.
    /// </summary>
    public interface ISkinEvents
    {
        /// <summary>
        /// call durgin skin skin event.
        /// </summary>
        /// <param name="e"></param>
        void Init(SkinEventArgs e);

        /// <summary>
        /// call durgin skin load event.
        /// </summary>
        /// <param name="e"></param>
        void Load(SkinEventArgs e);

        /// <summary>
        /// call durgin skin pre render event.
        /// </summary>
        /// <param name="e"></param>
        void PreRender(SkinEventArgs e);

        /// <summary>
        /// call durgin skin unload event.
        /// </summary>
        /// <param name="e"></param>
        void UnLoad(SkinEventArgs e);
    }
}
