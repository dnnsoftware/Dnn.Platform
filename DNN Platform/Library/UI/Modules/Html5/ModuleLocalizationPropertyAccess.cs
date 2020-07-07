// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules.Html5
{
    using System;
    using System.IO;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;
    using Newtonsoft.Json;

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
            this._html5File = html5File;
            this._moduleContext = moduleContext;
        }

        protected override string ProcessToken(ModuleLocalizationDto model, UserInfo accessingUser, Scope accessLevel)
        {
            string returnValue = string.Empty;

            string resourceFile = model.LocalResourceFile;
            if (string.IsNullOrEmpty(resourceFile))
            {
                var fileName = Path.GetFileName(this._html5File);
                var path = this._html5File.Replace(fileName, string.Empty);
                resourceFile = Path.Combine(path, Localization.LocalResourceDirectory + "/", Path.ChangeExtension(fileName, "resx"));
            }

            if (!string.IsNullOrEmpty(model.Key))
            {
                returnValue = Localization.GetString(model.Key, resourceFile);
            }

            return returnValue;
        }
    }
}
