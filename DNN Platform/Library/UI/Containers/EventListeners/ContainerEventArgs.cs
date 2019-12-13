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
