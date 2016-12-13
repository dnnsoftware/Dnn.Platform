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

using System;
using System.Xml.XPath;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Installers;

#endregion

namespace Dnn.Modules.Dashboard.Components.Installers
{
    public class DashboardInstaller : ComponentInstallerBase
    {
		#region "Private Properties"

        private string ControllerClass;
        private bool IsEnabled;
        private string Key;
        private string LocalResources;
        private string Src;
        private DashboardControl TempDashboardControl;
        private int ViewOrder;
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html";
            }
        }
		
		#endregion

		#region "Private Methods"

        private void DeleteDashboard()
        {
            try
            {
				//Attempt to get the Dashboard
                DashboardControl dashboardControl = DashboardController.GetDashboardControlByPackageId(Package.PackageID);
                if (dashboardControl != null)
                {
                    DashboardController.DeleteControl(dashboardControl);
                }
                Log.AddInfo(dashboardControl.DashboardControlKey + " " + Util.AUTHENTICATION_UnRegistered);
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        public override void Commit()
        {
        }

        public override void Install()
        {
            bool bAdd = Null.NullBoolean;
            try
            {
                //Attempt to get the Dashboard
                TempDashboardControl = DashboardController.GetDashboardControlByKey(Key);
                var dashboardControl = new DashboardControl();

                if (TempDashboardControl == null)
                {
                    dashboardControl.IsEnabled = true;
                    bAdd = true;
                }
                else
                {
                    dashboardControl.DashboardControlID = TempDashboardControl.DashboardControlID;
                    dashboardControl.IsEnabled = TempDashboardControl.IsEnabled;
                }
                dashboardControl.DashboardControlKey = Key;
                dashboardControl.PackageID = Package.PackageID;
                dashboardControl.DashboardControlSrc = Src;
                dashboardControl.DashboardControlLocalResources = LocalResources;
                dashboardControl.ControllerClass = ControllerClass;
                dashboardControl.ViewOrder = ViewOrder;
                if (bAdd)
                {
                    //Add new Dashboard
                    DashboardController.AddDashboardControl(dashboardControl);
                }
                else
                {
					//Update Dashboard
                    DashboardController.UpdateDashboardControl(dashboardControl);
                }
                Completed = true;
                Log.AddInfo(dashboardControl.DashboardControlKey + " " + Util.DASHBOARD_Registered);
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the Authentication compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			//Get the Key
            Key = Util.ReadElement(manifestNav, "dashboardControl/key", Log, Util.DASHBOARD_KeyMissing);

            //Get the Src
            Src = Util.ReadElement(manifestNav, "dashboardControl/src", Log, Util.DASHBOARD_SrcMissing);

            //Get the LocalResources
            LocalResources = Util.ReadElement(manifestNav, "dashboardControl/localResources", Log, Util.DASHBOARD_LocalResourcesMissing);

            //Get the ControllerClass
            ControllerClass = Util.ReadElement(manifestNav, "dashboardControl/controllerClass");

            //Get the IsEnabled Flag
            IsEnabled = bool.Parse(Util.ReadElement(manifestNav, "dashboardControl/isEnabled", "true"));

            //Get the ViewOrder
            ViewOrder = int.Parse(Util.ReadElement(manifestNav, "dashboardControl/viewOrder", "-1"));

            if (Log.Valid)
            {
                Log.AddInfo(Util.DASHBOARD_ReadSuccess);
            }
        }

        public override void Rollback()
        {
			//If Temp Dashboard exists then we need to update the DataStore with this 
            if (TempDashboardControl == null)
            {
				//No Temp Dashboard - Delete newly added system
                DeleteDashboard();
            }
            else
            {
				//Temp Dashboard - Rollback to Temp
                DashboardController.UpdateDashboardControl(TempDashboardControl);
            }
        }

        public override void UnInstall()
        {
            try
            {
				//Attempt to get the DashboardControl
                DashboardControl dashboardControl = DashboardController.GetDashboardControlByPackageId(Package.PackageID);
                if (dashboardControl != null)
                {
                    DashboardController.DeleteControl(dashboardControl);
                }
                Log.AddInfo(dashboardControl.DashboardControlKey + " " + Util.DASHBOARD_UnRegistered);
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }
		
		#endregion
    }
}
