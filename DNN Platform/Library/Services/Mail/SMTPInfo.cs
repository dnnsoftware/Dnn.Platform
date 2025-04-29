// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Mail;

public class SmtpInfo
{
    /// <summary>Gets or sets SMTP Server.</summary>
    public string Server { get; set; }

    /// <summary>Gets or sets SMTP Authentication.</summary>
    public string Authentication { get; set; }

    /// <summary>Gets or sets SMTP Username.</summary>
    public string Username { get; set; }

    /// <summary>Gets or sets SMTP Password.</summary>
    public string Password { get; set; }

    /// <summary>Gets or sets a value indicating whether SSL should be enabled or disabled.</summary>
    public bool EnableSSL { get; set; }

    /// <summary>
    /// Gets or sets SMTP OAuth provider.
    /// </summary>
    public string AuthProvider { get; set; }
}
