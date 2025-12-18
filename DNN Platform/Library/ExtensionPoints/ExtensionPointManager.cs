// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.ExtensionPoints.Filters;

    public class ExtensionPointManager
    {
        private static readonly object SyncRoot = new object();
        private static readonly CompositionContainer MefCompositionContainer = InitializeMefCompositionContainer();

#pragma warning disable 649

        [ImportMany]
        private IEnumerable<Lazy<IScriptItemExtensionPoint, IExtensionPointData>> scripts;

        [ImportMany]
        private IEnumerable<Lazy<IEditPageTabExtensionPoint, IExtensionPointData>> editPageTabExtensionPoint;

        [ImportMany]
        private IEnumerable<Lazy<IToolBarButtonExtensionPoint, IExtensionPointData>> toolbarButtonExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IEditPagePanelExtensionPoint, IExtensionPointData>> editPagePanelExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuItemExtensionPoint, IExtensionPointData>> ctxMenuItemExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IUserControlExtensionPoint, IExtensionPointData>> userControlExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IMenuItemExtensionPoint, IExtensionPointData>> menuItems;

        [ImportMany]
        private IEnumerable<Lazy<IGridColumnExtensionPoint, IExtensionPointData>> gridColumns;

#pragma warning restore 649

        /// <summary>Initializes a new instance of the <see cref="ExtensionPointManager"/> class.</summary>
        public ExtensionPointManager()
        {
            ComposeParts(this);
        }

        public static void ComposeParts(params object[] attributeParts)
        {
            lock (SyncRoot)
            {
                MefCompositionContainer.ComposeParts(attributeParts);
            }
        }

        public IEnumerable<IEditPageTabExtensionPoint> GetEditPageTabExtensionPoints(string module)
        {
            return this.GetEditPageTabExtensionPoints(module, null);
        }

        public IEnumerable<IEditPageTabExtensionPoint> GetEditPageTabExtensionPoints(string module, string group)
        {
            return from e in this.editPageTabExtensionPoint
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module)
        {
            return this.GetToolBarButtonExtensionPoints(module, null);
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module, string group)
        {
            return this.GetToolBarButtonExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {
            return from e in this.toolbarButtonExtensionPoints
                   where this.FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IToolBarButtonExtensionPoint GetToolBarButtonExtensionPointFirstByPriority(string module, string name)
        {
            return (from e in this.toolbarButtonExtensionPoints
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault();
        }

        public IEnumerable<IScriptItemExtensionPoint> GetScriptItemExtensionPoints(string module)
        {
            return this.GetScriptItemExtensionPoints(module, null);
        }

        public IEnumerable<IScriptItemExtensionPoint> GetScriptItemExtensionPoints(string module, string group)
        {
            return from e in this.scripts
                   where e.Metadata.Module == module
                       && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IEditPagePanelExtensionPoint> GetEditPagePanelExtensionPoints(string module)
        {
            return this.GetEditPagePanelExtensionPoints(module, null);
        }

        public IEnumerable<IEditPagePanelExtensionPoint> GetEditPagePanelExtensionPoints(string module, string group)
        {
            return from e in this.editPagePanelExtensionPoints
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEditPagePanelExtensionPoint GetEditPagePanelExtensionPointFirstByPriority(string module, string name)
        {
            return (from e in this.editPagePanelExtensionPoints
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault();
        }

        public IEnumerable<IContextMenuItemExtensionPoint> GetContextMenuItemExtensionPoints(string module)
        {
            return this.GetContextMenuItemExtensionPoints(module, null);
        }

        public IEnumerable<IContextMenuItemExtensionPoint> GetContextMenuItemExtensionPoints(string module, string group)
        {
            return from e in this.ctxMenuItemExtensionPoints
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IUserControlExtensionPoint GetUserControlExtensionPointFirstByPriority(string module, string name)
        {
            return (from e in this.userControlExtensionPoints
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault();
        }

        public IEnumerable<IUserControlExtensionPoint> GetUserControlExtensionPoints(string module, string group)
        {
            return from e in this.userControlExtensionPoints
                   where e.Metadata.Module == module && e.Metadata.Group == @group
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module)
        {
            return this.GetMenuItemExtensionPoints(module, null);
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module, string group)
        {
            return this.GetMenuItemExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {
            return from e in this.menuItems
                   where this.FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module)
        {
            return this.GetGridColumnExtensionPoints(module, null);
        }

        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module, string group)
        {
            return this.GetGridColumnExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {
            return from e in this.gridColumns
                   where this.FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;
        }

        private static CompositionContainer InitializeMefCompositionContainer()
        {
            var catalog = new AggregateCatalog();
            var path = Path.Combine(Globals.ApplicationMapPath, "bin");
            catalog.Catalogs.Add(new SafeDirectoryCatalog(path));
            return new CompositionContainer(catalog, true);
        }

        private bool FilterCondition(IExtensionPointData data, string module, string group)
        {
            return data.Module == module && (string.IsNullOrEmpty(@group) || data.Group == @group);
        }
    }
}
