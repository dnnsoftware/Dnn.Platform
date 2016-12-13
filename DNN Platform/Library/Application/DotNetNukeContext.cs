#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System.Collections.Generic;

using DotNetNuke.Collections.Internal;
using DotNetNuke.UI.Containers.EventListeners;
using DotNetNuke.UI.Skins.EventListeners;

#endregion

namespace DotNetNuke.Application
{
    /// <summary>
    /// Defines the context for the environment of the DotNetNuke application
    /// </summary>
    public class DotNetNukeContext
    {
        private static DotNetNukeContext _current;
        private readonly Application _application;
        private readonly IList<ContainerEventListener> _containerEventListeners;
        private readonly IList<SkinEventListener> _skinEventListeners;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetNukeContext" /> class.
        /// </summary>
        protected DotNetNukeContext() : this(new Application())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetNukeContext" /> class using the provided application as base.
        /// </summary>
        /// <param name="application">The application.</param>
        protected DotNetNukeContext(Application application)
        {
            _application = application;
            _containerEventListeners = new NaiveLockingList<ContainerEventListener>();
            _skinEventListeners = new NaiveLockingList<SkinEventListener>();
        }

		/// <summary>
		/// Get the application.
		/// </summary>
        public Application Application
        {
            get
            {
                return _application;
            }
        }

		/// <summary>
		/// Gets the container event listeners. The listeners will be called in each life cycle of load container.
		/// </summary>
		/// <see cref="ContainerEventListener"/>
		/// <seealso cref="DotNetNuke.UI.Containers.Container.OnInit"/>
		/// <seealso cref="DotNetNuke.UI.Containers.Container.OnLoad"/>
		/// <seealso cref="DotNetNuke.UI.Containers.Container.OnPreRender"/>
		/// <seealso cref="DotNetNuke.UI.Containers.Container.OnUnload"/>
        public IList<ContainerEventListener> ContainerEventListeners
        {
            get
            {
                return _containerEventListeners;
            }
        }

		/// <summary>
		/// Gets the skin event listeners. The listeners will be called in each life cycle of load skin.
		/// </summary>
		/// <see cref="SkinEventListener"/>
		/// <seealso cref="DotNetNuke.UI.Skins.Skin.OnInit"/>
		/// <seealso cref="DotNetNuke.UI.Skins.Skin.OnLoad"/>
		/// <seealso cref="DotNetNuke.UI.Skins.Skin.OnPreRender"/>
		/// <seealso cref="DotNetNuke.UI.Skins.Skin.OnUnload"/>
        public IList<SkinEventListener> SkinEventListeners
        {
            get
            {
                return _skinEventListeners;
            }
        }

		/// <summary>
		/// Gets or sets the current app context.
		/// </summary>
        public static DotNetNukeContext Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new DotNetNukeContext();
                }
                return _current;
            }
            set
            {
                _current = value;
            }
        }
    }
}
