#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Instrumentation;
﻿using DotNetNuke.Services.Installer.Packages;
using System.Reflection;

namespace DotNetNuke.Services.Connections
{
    public sealed class ConnectionsManager : ServiceLocator<IConnectionsManager, ConnectionsManager>, IConnectionsManager
    {
        #region Service Locator Implements

        protected override Func<IConnectionsManager> GetFactory()
        {
            return  () => new ConnectionsManager();
        }

        #endregion

        #region Properties

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConnectionsManager));
        private static readonly object LockerObject = new object();
        private static IDictionary<string, IConnector> _processors;

        #endregion

        #region Public Methods

        public void RegisterConnections()
        {
            if (_processors == null)
            {
                lock (LockerObject)
                {
                    if (_processors == null)
                    {
                        LoadProcessors();
                    }
                }
            }
        }

        public IList<IConnector> GetConnectors()
        {
            return _processors.Values.Where(x => IsPackageInstalled(x.GetType().Assembly)).ToList();
        }

        #endregion

        #region Private Methods

        private void LoadProcessors()
        {
            _processors = new Dictionary<string, IConnector>();

            var typeLocator = new TypeLocator();
            var types = typeLocator.GetAllMatchingTypes(IsValidFilter);

            foreach (var type in types)
            {
                try
                {
                    var processor = Activator.CreateInstance(type) as IConnector;
                    if (processor != null 
                            && !string.IsNullOrEmpty(processor.Name) 
                            && !_processors.ContainsKey(processor.Name))
                    {
                        _processors.Add(processor.Name, processor);
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Unable to create {0} while registering connections.{1}", type.FullName, e.Message);
                }
            }
        }

        private bool IsValidFilter(Type t)
        {
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof(IConnector).IsAssignableFrom(t);
        }

        private bool IsPackageInstalled(Assembly assembly)
        {
            return PackageController.Instance.GetExtensionPackages(-1,
                info => info.Name == assembly.GetName().Name && info.IsValid).Count > 0;
        }

        #endregion
    }
}
