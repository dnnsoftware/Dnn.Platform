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
using System.Collections.Generic;
using System.IO;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using Dnn.Modules.Dashboard.Data;

#endregion

namespace Dnn.Modules.Dashboard.Components
{
    public class DashboardController
    {
        public static int AddDashboardControl(DashboardControl dashboardControl)
        {
            return DataService.AddDashboardControl(dashboardControl.PackageID,
                                                   dashboardControl.DashboardControlKey,
                                                   dashboardControl.IsEnabled,
                                                   dashboardControl.DashboardControlSrc,
                                                   dashboardControl.DashboardControlLocalResources,
                                                   dashboardControl.ControllerClass,
                                                   dashboardControl.ViewOrder);
        }

        public static void DeleteControl(DashboardControl dashboardControl)
        {
            DataService.DeleteDashboardControl(dashboardControl.DashboardControlID);
        }

        public static void Export(string filename)
        {
            string fullName = Path.Combine(Globals.HostMapPath, filename);
            var settings = new XmlWriterSettings();
            using (XmlWriter writer = XmlWriter.Create(fullName, settings))
            {
                //Write start of Dashboard 
                writer.WriteStartElement("dashboard");
                foreach (DashboardControl dashboard in GetDashboardControls(true))
                {
                    var controller = Activator.CreateInstance(Reflection.CreateType(dashboard.ControllerClass)) as IDashboardData;
                    if (controller != null)
                    {
                        controller.ExportData(writer);
                    }
                }
				
                //Write end of Host 
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        public static DashboardControl GetDashboardControlByKey(string dashboardControlKey)
        {
            return CBO.FillObject<DashboardControl>(DataService.GetDashboardControlByKey(dashboardControlKey));
        }

        public static DashboardControl GetDashboardControlByPackageId(int packageId)
        {
            return CBO.FillObject<DashboardControl>(DataService.GetDashboardControlByPackageId(packageId));
        }

        public static List<DashboardControl> GetDashboardControls(bool isEnabled)
        {
            return CBO.FillCollection<DashboardControl>(DataService.GetDashboardControls(isEnabled));
        }

        public static void UpdateDashboardControl(DashboardControl dashboardControl)
        {
            DataService.UpdateDashboardControl(dashboardControl.DashboardControlID,
                                               dashboardControl.DashboardControlKey,
                                               dashboardControl.IsEnabled,
                                               dashboardControl.DashboardControlSrc,
                                               dashboardControl.DashboardControlLocalResources,
                                               dashboardControl.ControllerClass,
                                               dashboardControl.ViewOrder);
        }
    }
}