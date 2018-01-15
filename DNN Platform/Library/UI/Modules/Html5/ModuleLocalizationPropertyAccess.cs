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

using System;
using System.IO;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;

namespace DotNetNuke.UI.Modules.Html5
{
    public class ModuleLocalizationDto
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("localresourcefile")]
        public string LocalResourceFile { get; set; }
    }

    public class ModuleLocalizationPropertyAccess : JsonPropertyAccess<ModuleLocalizationDto>
    {
        private readonly ModuleInstanceContext _moduleContext;
        private readonly string _html5File;

        public ModuleLocalizationPropertyAccess(ModuleInstanceContext moduleContext, string html5File)
        {
            _html5File = html5File;
            _moduleContext = moduleContext;
        }

        protected override string ProcessToken(ModuleLocalizationDto model, UserInfo accessingUser, Scope accessLevel)
        {
            string returnValue = string.Empty;

            string resourceFile = model.LocalResourceFile;
            if (String.IsNullOrEmpty(resourceFile))
            {
                var fileName = Path.GetFileName(_html5File);
                var path = _html5File.Replace(fileName, "");
                resourceFile = Path.Combine(path, Localization.LocalResourceDirectory + "/", Path.ChangeExtension(fileName, "resx"));
            }
            if (!String.IsNullOrEmpty(model.Key))
            {
                returnValue = Localization.GetString(model.Key, resourceFile);
            }

            return returnValue;
        }
    }
}
