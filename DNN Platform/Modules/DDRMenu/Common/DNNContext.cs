﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Web.DDRMenu.DNNCommon
{
	public class DNNContext : IDisposable
	{
		public static DNNContext Current { get { return (DNNContext)HttpContext.Current.Items[DataName]; } private set { HttpContext.Current.Items[DataName] = value; } }

		private readonly DNNContext savedContext;

		public Control HostControl { get; private set; }

		private Page _Page;
		public Page Page { get { return this._Page ?? (this._Page = this.HostControl.Page); } }

		private PortalSettings _PortalSettings;
		public PortalSettings PortalSettings { get { return this._PortalSettings ?? (this._PortalSettings = PortalController.Instance.GetCurrentPortalSettings()); } }

		private TabInfo _ActiveTab;
		public TabInfo ActiveTab { get { return this._ActiveTab ?? (this._ActiveTab = this.PortalSettings.ActiveTab); } }

		private string _SkinPath;
		public string SkinPath { get { return this._SkinPath ?? (this._SkinPath = this.ActiveTab.SkinPath); } }

		private static string _ModuleName;
		public static string ModuleName { get { return _ModuleName ?? (_ModuleName = GetModuleNameFromAssembly()); } }

		private static string _ModuleFolder;
		public static string ModuleFolder
		{
			get
			{
				return _ModuleFolder ??
				       (_ModuleFolder =
				        String.Format(
				        	"~/DesktopModules/{0}/", DesktopModuleController.GetDesktopModuleByModuleName(ModuleName, PortalSettings.Current.PortalId).FolderName));
			}
		}

		private static string _DataName;
		private static string DataName { get { return _DataName ?? (_DataName = "DDRMenu.DNNContext." + ModuleName); } }

		public DNNContext(Control hostControl)
		{
			this.HostControl = hostControl;

			this.savedContext = Current;
			Current = this;
		}

		public string ResolveUrl(string relativeUrl)
		{
			return this.HostControl.ResolveUrl(relativeUrl);
		}

		private static string GetModuleNameFromAssembly()
		{
			var moduleFullName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
// ReSharper disable PossibleNullReferenceException
			return moduleFullName.Substring(moduleFullName.LastIndexOf('.') + 1);
// ReSharper restore PossibleNullReferenceException
		}

		public void Dispose()
		{
			Current = this.savedContext;
		}
	}
}
