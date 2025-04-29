// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Application;

using System;
using System.Diagnostics;
using System.Reflection;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

using NewReleaseMode = DotNetNuke.Abstractions.Application.ReleaseMode;

/// <inheritdoc />
public class Application : IApplicationInfo
{
    private static NewReleaseMode status = NewReleaseMode.None;

    /// <summary>Initializes a new instance of the <see cref="Application"/> class.</summary>
    public Application()
    {
    }

    /// <inheritdoc />
    public string Company
    {
        get
        {
            return "DNN Corporation";
        }
    }

    /// <inheritdoc />
    public virtual Version CurrentVersion
    {
        get
        {
            return DataProvider.Instance().GetVersion();
        }
    }

    /// <inheritdoc />
    public virtual string Description
    {
        get
        {
            return "DNN Platform";
        }
    }

    /// <inheritdoc />
    public string HelpUrl
    {
        get
        {
            return "https://docs.dnncommunity.org/";
        }
    }

    /// <inheritdoc />
    public string LegalCopyright
    {
        get
        {
            return string.Concat("DNN Platform is copyright 2002-", DateTime.Today.ToString("yyyy"), " by .NET Foundation");
        }
    }

    /// <inheritdoc />
    public virtual string Name
    {
        get
        {
            return "DNNCORP.CE";
        }
    }

    /// <inheritdoc />
    public virtual string SKU
    {
        get
        {
            return "DNN";
        }
    }

    /// <summary>Gets the status of the DotnetNuke application.</summary>
    /// <remarks>If the value is not be Stable, you will see the exactly status and version in page's title if allow display beta message in host setting.</remarks>
    /// <value>The value can be: None, Alpha, Beta, RC, Stable.</value>
    [Obsolete("Deprecated in DotNetNuke 9.7.0. Use 'DotNetNuke.Abstractions.Application.IApplicationInfo' with Dependency Injection instead. Scheduled removal in v11.0.0.")]
    public ReleaseMode Status { get => (ReleaseMode)(this as IApplicationInfo).Status; }

    /// <inheritdoc />
    NewReleaseMode IApplicationInfo.Status
    {
        get
        {
            if (status == NewReleaseMode.None)
            {
                Assembly assy = Assembly.GetExecutingAssembly();
                if (Attribute.IsDefined(assy, typeof(AssemblyStatusAttribute)))
                {
                    Attribute attr = Attribute.GetCustomAttribute(assy, typeof(AssemblyStatusAttribute));
                    if (attr != null)
                    {
                        status = (NewReleaseMode)((AssemblyStatusAttribute)attr).Status;
                    }
                }
            }

            return status;
        }
    }

    /// <inheritdoc />
    public string Title
    {
        get
        {
            return "DotNetNuke";
        }
    }

    /// <inheritdoc />
    public string Trademark
    {
        get
        {
            return "DotNetNuke,DNN";
        }
    }

    /// <inheritdoc />
    public string Type
    {
        get
        {
            return "Framework";
        }
    }

    /// <inheritdoc />
    public string UpgradeUrl
    {
        get
        {
            var url = Config.GetSetting("UpdateServiceUrl");
            if (string.IsNullOrEmpty(url))
            {
                return "https://dnnplatform.io";
            }

            return url;
        }
    }

    /// <inheritdoc />
    public string Url
    {
        get
        {
            return "https://dnncommunity.org";
        }
    }

    /// <inheritdoc />
    public virtual Version Version
    {
        get
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var fileVersion = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            return new Version(fileVersion);
        }
    }

    /// <inheritdoc />
    public virtual bool ApplyToProduct(string productNames)
    {
        return productNames.Contains(this.Name);
    }
}
