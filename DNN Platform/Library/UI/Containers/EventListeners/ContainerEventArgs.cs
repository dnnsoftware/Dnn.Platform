// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

#endregion

namespace DotNetNuke.UI.Containers.EventListeners
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ContainerEventArgs provides a custom EventARgs class for Container Events
    /// </summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public class ContainerEventArgs : EventArgs
    {
        private readonly Container _Container;

        public ContainerEventArgs(Container container)
        {
            this._Container = container;
        }

        public Container Container
        {
            get
            {
                return this._Container;
            }
        }
    }
}
