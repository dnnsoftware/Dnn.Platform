// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules.Html5;

using System.IO;

using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Tokens;

public class ModuleLocalizationPropertyAccess : JsonPropertyAccess<ModuleLocalizationDto>
{
    private readonly ModuleInstanceContext moduleContext;
    private readonly string html5File;

    /// <summary>Initializes a new instance of the <see cref="ModuleLocalizationPropertyAccess"/> class.</summary>
    /// <param name="moduleContext"></param>
    /// <param name="html5File"></param>
    public ModuleLocalizationPropertyAccess(ModuleInstanceContext moduleContext, string html5File)
    {
        this.html5File = html5File;
        this.moduleContext = moduleContext;
    }

    /// <inheritdoc/>
    protected override string ProcessToken(ModuleLocalizationDto model, UserInfo accessingUser, Scope accessLevel)
    {
        string returnValue = string.Empty;

        string resourceFile = model.LocalResourceFile;
        if (string.IsNullOrEmpty(resourceFile))
        {
            var fileName = Path.GetFileName(this.html5File);
            var path = this.html5File.Replace(fileName, string.Empty);
            resourceFile = Path.Combine(path, Localization.LocalResourceDirectory + "/", Path.ChangeExtension(fileName, "resx"));
        }

        if (!string.IsNullOrEmpty(model.Key))
        {
            returnValue = Localization.GetString(model.Key, resourceFile);
        }

        return returnValue;
    }
}
