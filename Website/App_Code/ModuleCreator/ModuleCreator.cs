#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2012
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

#region Using Statements

using System.IO;
using System.Collections.Generic;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Modules.Admin.Modules
{

    public class ModuleCreatorController : IUpgradeable
    {

           public string UpgradeModule(string Version) 
           { 
               //Fix icon and add "Development" to the DesktopModule taxonomy and associate it with this module
               if (Version == "01.00.00")
               {
                   var vocabularyId = -1;
                   var termId = -1;
                   var objTermController = DotNetNuke.Entities.Content.Common.Util.GetTermController();
	           var objTerms = objTermController.GetTermsByVocabulary("Module_Categories");
                   foreach(Term term in objTerms) 
                   {
                       vocabularyId = term.VocabularyId;
                       if (term.Name == "Development")
                       {
                           termId = term.TermId;
                       }
                   }
                   if (termId == -1)
                   {
                       termId = objTermController.AddTerm(new Term(vocabularyId) { Name = "Development" });
                   }
                   var objTerm = objTermController.GetTerm(termId);

                   var portalID = -1;
                   Dictionary<string, string> HostSettings = HostController.Instance.GetSettingsDictionary();
                   if (HostSettings.ContainsKey("HostPortalId"))
                   {
                       portalID = int.Parse(HostSettings["HostPortalId"]);
                   }
                   if (portalID != -1)
                   {
                       var objDesktopModule = DesktopModuleController.GetDesktopModuleByModuleName("DNNCorp.ModuleCreator", portalID);
                       var objPackage = PackageController.GetPackage(objDesktopModule.PackageID);
                       objPackage.IconFile = "~/DesktopModules/ModuleCreator/icon.png";
                       PackageController.SavePackage(objPackage);

                       var objContentController = DotNetNuke.Entities.Content.Common.Util.GetContentController();
                       var objContent = objContentController.GetContentItem(objDesktopModule.ContentItemId);
                       objTermController.AddTermToContent(objTerm, objContent);
                   }
               }

               return Version; 
           }
    }
}

