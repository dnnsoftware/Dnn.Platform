// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;

    public sealed class ConnectionsManager : ServiceLocator<IConnectionsManager, ConnectionsManager>, IConnectionsManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConnectionsManager));
        private static readonly object LockerObject = new object();
        private static IDictionary<string, IConnector> _processors;

        public void RegisterConnections()
        {
            if (_processors == null)
            {
                lock (LockerObject)
                {
                    if (_processors == null)
                    {
                        this.LoadProcessors();
                    }
                }
            }
        }

        public IList<IConnector> GetConnectors()
        {
            return _processors.Values.Where(x => this.IsPackageInstalled(x.GetType().Assembly)).ToList();
        }

        protected override Func<IConnectionsManager> GetFactory()
        {
            return () => new ConnectionsManager();
        }

        private void LoadProcessors()
        {
            _processors = new Dictionary<string, IConnector>();

            var typeLocator = new TypeLocator();
            var types = typeLocator.GetAllMatchingTypes(this.IsValidFilter);

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
            return PackageController.Instance.GetExtensionPackages(
                -1,
                info => info.Name == assembly.GetName().Name && info.IsValid).Count > 0;
        }
    }
}
