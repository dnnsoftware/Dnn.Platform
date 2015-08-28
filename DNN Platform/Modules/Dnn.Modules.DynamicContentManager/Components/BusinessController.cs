// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Upgrade;

#pragma warning disable 1591

namespace Dnn.Modules.DynamicContentManager.Components
{
    public class BusinessController : IUpgradeable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BusinessController));

        public string UpgradeModule(string Version)
        {
            try
            {
                switch (Version)
                {
                    case "08.00.00":
                        var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("Dnn.DynamicContentManager", Null.NullInteger);

                        if (desktopModule != null)
                        {
                            desktopModule.Category = "Admin";
                            DesktopModuleController.SaveDesktopModule(desktopModule, false, false);

                            ModuleDefinitionInfo moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByDefinitionName("Dnn.DynamicContentManager", desktopModule.DesktopModuleID);

                            if (moduleDefinition != null)
                            {
                                //Add Module to Admin Page for all Portals
                                Upgrade.AddAdminPages("Dynamic Content Type Manager",
                                                      "Manage the Dynamic Content Types for your Site",
                                                      "~/images/icon_tag_16px.gif",
                                                      "~/images/icon_tag_32px.gif",
                                                      true,
                                                      moduleDefinition.ModuleDefID,
                                                      String.Empty,
                                                      "~/images/icon_tag_32px.gif",
                                                      true);
                            }
                        }

                        var gettingStartedFile = FileManager.Instance.GetFile(-1, "Content Templates/GettingStarted.cshtml");

                        //Ensure Getting Started is registered
                        FolderManager.Instance.Synchronize(-1);
                        
                        var htmlContentType = DynamicContentTypeManager.Instance.GetContentTypes(-1, false).SingleOrDefault(t => t.Name == "HTML" && t.IsDynamic);
                        if (htmlContentType != null && gettingStartedFile != null)
                        {
                            var template = new ContentTemplate(-1)
                                                {
                                                    Name = "Getting Started",
                                                    TemplateFileId = gettingStartedFile.FileId,
                                                    ContentTypeId = htmlContentType.ContentTypeId
                                                };
                            ContentTemplateManager.Instance.UpdateContentTemplate(template);
                        }

                        break;
                }
                return "Success";
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                return "Failed";
            }
        }
    }
}