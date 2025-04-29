// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Services.Dto;

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Information about the other settings in the security section of the persona bar to save.
/// </summary>
public class UpdateOtherSettingsRequest
{
    /// <summary>
    /// Gets or sets a value indicating whether to show critical errors on screen.
    /// </summary>
    public bool ShowCriticalErrors { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use debug mode.
    /// </summary>
    public bool DebugMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show the "Remember Me" checkbox for login.
    /// </summary>
    public bool RememberCheckbox { get; set; }

    /// <summary>
    /// Gets or sets the duration in minutes of the automatic account unlock.
    /// </summary>
    public int AutoAccountUnlockDuration { get; set; }

    /// <summary>
    /// Gets or sets the timout in seconds for async postbacks.
    /// </summary>
    public int AsyncTimeout { get; set; }

    /// <summary>
    /// Gets or sets the maximum size of a file upload in MB.
    /// </summary>
    public long MaxUploadSize { get; set; }

    /// <summary>
    /// Gets or sets the allowed extension whitelist.
    /// </summary>
    public string AllowedExtensionWhitelist { get; set; }

    /// <summary>
    /// Gets or sets the default end user extension whitelist.
    /// </summary>
    public string DefaultEndUserExtensionWhitelist { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether visitors can load arbitrary themes by SkinSrc querystring param.
    /// </summary>
    public bool AllowOverrideThemeViaQueryString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether rich text module titles are allowed.
    /// </summary>
    public bool AllowRichTextModuleTitle { get; set; }
}
