// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.UI.Containers.EventListeners
{
    ///-----------------------------------------------------------------------------
    /// <summary>
    /// ContainerEventArgs provides a custom EventARgs class for Container Events
    /// </summary>
    /// <remarks></remarks>
    ///-----------------------------------------------------------------------------
    public class ContainerEventArgs : EventArgs
    {
        private readonly Container _Container;

        public ContainerEventArgs(Container container)
        {
            _Container = container;
        }

        public Container Container
        {
            get
            {
                return _Container;
            }
        }
    }
}
