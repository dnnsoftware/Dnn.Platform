﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
