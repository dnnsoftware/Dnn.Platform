#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.IO;
using System.Web;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Entities.Modules.Definitions
{
    public enum ModuleDefinitionVersion
    {
        VUnknown = 0,
        V1 = 1,
        V2 = 2,
        V2_Skin = 3,
        V2_Provider = 4,
        V3 = 5
    }

    public class ModuleDefinitionValidator : XmlValidatorBase
    {
        private string GetDnnSchemaPath(Stream xmlStream)
        {
            ModuleDefinitionVersion Version = GetModuleDefinitionVersion(xmlStream);
            string schemaPath = "";
            switch (Version)
            {
                case ModuleDefinitionVersion.V2:
                    schemaPath = "components\\ResourceInstaller\\ModuleDef_V2.xsd";
                    break;
                case ModuleDefinitionVersion.V3:
                    schemaPath = "components\\ResourceInstaller\\ModuleDef_V3.xsd";
                    break;
                case ModuleDefinitionVersion.V2_Skin:
                    schemaPath = "components\\ResourceInstaller\\ModuleDef_V2Skin.xsd";
                    break;
                case ModuleDefinitionVersion.V2_Provider:
                    schemaPath = "components\\ResourceInstaller\\ModuleDef_V2Provider.xsd";
                    break;
                case ModuleDefinitionVersion.VUnknown:
                    throw new Exception(GetLocalizedString("EXCEPTION_LoadFailed"));
            }
            return Path.Combine(Globals.ApplicationMapPath, schemaPath);
        }

        private static string GetLocalizedString(string key)
        {
            var objPortalSettings = (PortalSettings) HttpContext.Current.Items["PortalSettings"];
            if (objPortalSettings == null)
            {
                return key;
            }
            return Localization.GetString(key, objPortalSettings);
        }

        public ModuleDefinitionVersion GetModuleDefinitionVersion(Stream xmlStream)
        {
            ModuleDefinitionVersion retValue;
            xmlStream.Seek(0, SeekOrigin.Begin);
            var xmlReader = new XmlTextReader(xmlStream)
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit
            };
            xmlReader.MoveToContent();

            //This test assumes provides a simple validation 
            switch (xmlReader.LocalName.ToLower())
            {
                case "module":
                    retValue = ModuleDefinitionVersion.V1;
                    break;
                case "dotnetnuke":
                    switch (xmlReader.GetAttribute("type"))
                    {
                        case "Module":
                            switch (xmlReader.GetAttribute("version"))
                            {
                                case "2.0":
                                    retValue = ModuleDefinitionVersion.V2;
                                    break;
                                case "3.0":
                                    retValue = ModuleDefinitionVersion.V3;
                                    break;
                                default:
                                    return ModuleDefinitionVersion.VUnknown;
                            }
                            break;
                        case "SkinObject":
                            retValue = ModuleDefinitionVersion.V2_Skin;
                            break;
                        case "Provider":
                            retValue = ModuleDefinitionVersion.V2_Provider;
                            break;
                        default:
                            retValue = ModuleDefinitionVersion.VUnknown;
                            break;
                    }
                    break;
                default:
                    retValue = ModuleDefinitionVersion.VUnknown;
                    break;
            }
            return retValue;
        }

        public override bool Validate(Stream XmlStream)
        {
            SchemaSet.Add("", GetDnnSchemaPath(XmlStream));
            return base.Validate(XmlStream);
        }
    }
}