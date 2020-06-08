﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using System.Reflection;
using Adequation.DNN.LocalizationExtensions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Web.DDRMenu.Localisation
{
	public class Adequation : ILocalisation
	{
		private bool haveChecked;
		private bool found;

		public bool HaveApi()
		{
			if (!haveChecked)
			{
				found = (new DesktopModuleController().GetDesktopModuleByModuleName("Localization Extensions Configuration") != null);
				haveChecked = true;
			}

			return found;
		}

		public TabInfo LocaliseTab(TabInfo tab, int portalId)
		{
			return null;
		}

		public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
		{
			return LE.Instance.ProcessNavigationNodes(nodes);
		}
	}
}
