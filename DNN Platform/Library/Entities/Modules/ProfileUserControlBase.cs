// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Project  :  DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Modules
    /// Class    :  ProfileUserControlBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfileUserControlBase class defines a custom base class for the profile Control.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProfileUserControlBase : UserModuleBase
    {
        public event EventHandler ProfileUpdated;

        public event EventHandler ProfileUpdateCompleted;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the OnProfileUpdateCompleted Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnProfileUpdateCompleted(EventArgs e)
        {
            if (this.ProfileUpdateCompleted != null)
            {
                this.ProfileUpdateCompleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the ProfileUpdated Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnProfileUpdated(EventArgs e)
        {
            if (this.ProfileUpdated != null)
            {
                this.ProfileUpdated(this, e);
            }
        }
    }
}
