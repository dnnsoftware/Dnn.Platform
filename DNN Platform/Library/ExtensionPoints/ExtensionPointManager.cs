#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.ExtensionPoints.Filters;

namespace DotNetNuke.ExtensionPoints
{
    public class ExtensionPointManager
    {
        private static readonly object SyncRoot = new object();

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

        public ExtensionPointManager()
        {        
            ComposeParts(this);
        }

        private static CompositionContainer MefCompositionContainer
        {
            get
            {
                var container = HttpContext.Current.Application["MefCompositionContainer"] as CompositionContainer;                
                if (container == null)
                {
                    var catalog = new AggregateCatalog();
                    var path = Path.Combine(Globals.ApplicationMapPath, "bin");
                    catalog.Catalogs.Add(new SafeDirectoryCatalog(path));
                    container = new CompositionContainer(catalog, true);
                    HttpContext.Current.Application["MefCompositionContainer"] = container;
                }

                return container;
            }
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
            return GetEditPageTabExtensionPoints(module, null);
        }

        public IEnumerable<IEditPageTabExtensionPoint> GetEditPageTabExtensionPoints(string module, string group)
        {
            return from e in _editPageTabExtensionPoint
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module)
        {
            return GetToolBarButtonExtensionPoints(module, null);
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module, string group)
        {
            return GetToolBarButtonExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {            
            return from e in _toolbarButtonExtensionPoints
                   where FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;                
        }

        public IEnumerable<IScriptItemExtensionPoint> GetScriptItemExtensionPoints(string module)
        {
            return GetScriptItemExtensionPoints(module, null);
        }

        public IEnumerable<IScriptItemExtensionPoint> GetScriptItemExtensionPoints(string module, string group)
        {
            return from e in _scripts
                where e.Metadata.Module == module 
                    && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)     
                orderby e.Value.Order
                select e.Value;
        }

        public IEnumerable<IEditPagePanelExtensionPoint> GetEditPagePanelExtensionPoints(string module)
        {
            return GetEditPagePanelExtensionPoints(module, null);
        }

        public IEnumerable<IEditPagePanelExtensionPoint> GetEditPagePanelExtensionPoints(string module, string group)
        {
            return from e in _editPagePanelExtensionPoints
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEditPagePanelExtensionPoint GetEditPagePanelExtensionPointFirstByPriority(string module, string name)
        {
            return (from e in _editPagePanelExtensionPoints 
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault(); 
        }

        public IEnumerable<IContextMenuItemExtensionPoint> GetContextMenuItemExtensionPoints(string module)
        {
            return GetContextMenuItemExtensionPoints(module, null);
        }

        public IEnumerable<IContextMenuItemExtensionPoint> GetContextMenuItemExtensionPoints(string module, string group)
        {            
            return from e in _ctxMenuItemExtensionPoints
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order 
                   select e.Value;
        }

        public IUserControlExtensionPoint GetUserControlExtensionPointFirstByPriority(string module, string name)
        {
            return (from e in _userControlExtensionPoints
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault();            
        }

        public IEnumerable<IUserControlExtensionPoint> GetUserControlExtensionPoints(string module, string group)
        {
            return from e in _userControlExtensionPoints 
                   where e.Metadata.Module == module && e.Metadata.Group == @group
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module)
        {
            return GetMenuItemExtensionPoints(module, null);
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module, string group)
        {
            return GetMenuItemExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {
            return from e in _menuItems
                   where FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order 
                   select e.Value;
        }
        
        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module)
        {
            return GetGridColumnExtensionPoints(module, null);
        }

        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module, string group)
        {
            return GetGridColumnExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {
            return from e in _gridColumns
                   where FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;
        }

        private bool FilterCondition(IExtensionPointData data, string module, string group)
        {
            return data.Module == module && (string.IsNullOrEmpty(@group) || data.Group == @group);
        }
    }
}