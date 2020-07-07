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
            return from e in this._editPageTabExtensionPoint
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
            return from e in this._toolbarButtonExtensionPoints
                   where this.FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IToolBarButtonExtensionPoint GetToolBarButtonExtensionPointFirstByPriority(string module, string name)
        {
            return (from e in this._toolbarButtonExtensionPoints
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
            return from e in this._scripts
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
            return from e in this._editPagePanelExtensionPoints
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEditPagePanelExtensionPoint GetEditPagePanelExtensionPointFirstByPriority(string module, string name)
        {
            return (from e in this._editPagePanelExtensionPoints
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
            return from e in this._ctxMenuItemExtensionPoints
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IUserControlExtensionPoint GetUserControlExtensionPointFirstByPriority(string module, string name)
        {
            return (from e in this._userControlExtensionPoints
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault();
        }

        public IEnumerable<IUserControlExtensionPoint> GetUserControlExtensionPoints(string module, string group)
        {
            return from e in this._userControlExtensionPoints
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
            return from e in this._menuItems
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
            return from e in this._gridColumns
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

#pragma warning disable 649

        [ImportMany]
        private IEnumerable<Lazy<IScriptItemExtensionPoint, IExtensionPointData>> _scripts;

        [ImportMany]
        private IEnumerable<Lazy<IEditPageTabExtensionPoint, IExtensionPointData>> _editPageTabExtensionPoint;

        [ImportMany]
        private IEnumerable<Lazy<IToolBarButtonExtensionPoint, IExtensionPointData>> _toolbarButtonExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IEditPagePanelExtensionPoint, IExtensionPointData>> _editPagePanelExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuItemExtensionPoint, IExtensionPointData>> _ctxMenuItemExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IUserControlExtensionPoint, IExtensionPointData>> _userControlExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IMenuItemExtensionPoint, IExtensionPointData>> _menuItems;

        [ImportMany]
        private IEnumerable<Lazy<IGridColumnExtensionPoint, IExtensionPointData>> _gridColumns;

#pragma warning restore 649
    }
}
