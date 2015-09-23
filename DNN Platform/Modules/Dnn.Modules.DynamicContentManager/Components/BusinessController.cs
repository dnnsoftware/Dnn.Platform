﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Upgrade;
using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

#pragma warning disable 1591

namespace Dnn.Modules.DynamicContentManager.Components
{
    public class BusinessController : IUpgradeable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BusinessController));

        private static void AddTemplate(string fileName, int portalId, string contentTypeName, string templateName)
        {
            var folder = FolderManager.Instance.GetFolder(-1, "Content Templates/");
            var file = new FileInfo
                            {
                                PortalId = portalId,
                                FileName = fileName,
                                Extension = "cshtml",
                                FolderId = folder.FolderID,
                                Folder = folder.FolderPath,
                                StartDate = DateTime.Now,
                                EndDate = Null.NullDate,
                                EnablePublishPeriod = false,
                                ContentItemID = Null.NullInteger
                            };

            //Save new File
            try
            {
                //Initially, install files are on local system, then we need the Standard folder provider to read the content regardless the target folderprovider					
                using (var fileContent = FolderProvider.Instance("StandardFolderProvider").GetFileStream(file))
                {
                    var contentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName));
                    var userId = UserController.Instance.GetCurrentUserInfo().UserID;
                    file.FileId = FileManager.Instance.AddFile(folder, fileName, fileContent, false, false, true, contentType, userId).FileId;
                }

                var dynamicContentType = DynamicContentTypeManager.Instance.GetContentTypes(portalId, false)
                                        .SingleOrDefault(t => t.Name == contentTypeName && t.IsDynamic);
                if (dynamicContentType != null)
                {
                    var template = new ContentTemplate(portalId)
                                        {
                                            Name = templateName,
                                            TemplateFileId = file.FileId,
                                            ContentTypeId = dynamicContentType.ContentTypeId
                                        };
                    ContentTemplateManager.Instance.AddContentTemplate(template);
                }
            }
            catch (InvalidFileExtensionException ex) //when the file is not allowed, we should not break parse process, but just log the error.
            {
                Logger.Error(ex.Message);
            }
        }

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

                        //Ensure Getting Started is registered
                        AddTemplate("GettingStarted.cshtml", -1, "HTML", "Getting Started");
                        AddTemplate("ViewHTML.cshtml", -1, "HTML", "View HTML");
                        AddTemplate("ViewMarkdown.cshtml", -1, "Markdown", "View Markdown");
                        AddTemplate("EditHTML.cshtml", -1, "HTML", "Edit HTML");
                        AddTemplate("EditMarkdown.cshtml", -1, "Markdown", "Edit Markdown");

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