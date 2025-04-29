// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Application;

using System;

/// <summary>The Application class contains properties that describe the DotNetNuke Application.</summary>
public interface IApplicationInfo
{
    /// <summary>Gets the company to which the DotNetNuke application is related.</summary>
    /// <value>Fixed result: DotNetNuke Corporation.</value>
    string Company { get; }

    /// <summary>
    /// Gets the version of the currently installed DotNetNuke framework/application
    /// Can be prior to Version, if the application is pending to be upgraded.
    /// </summary>
    /// <value>The version as retreieved from the database version table.</value>
    Version CurrentVersion { get; }

    /// <summary>Gets the description of the application.</summary>
    /// <value>Fixed result: DNN Platform.</value>
    string Description { get; }

    /// <summary>Gets the help URL related to the DotNetNuke application.</summary>
    /// <value>Fixed result: https://docs.dnncommunity.org/. </value>
    string HelpUrl { get; }

    /// <summary>Gets the legal copyright.</summary>
    /// <value>Dynamic: DNN Platform is copyright 2002-todays year by .NET Foundation".</value>
    string LegalCopyright { get; }

    /// <summary>Gets the name of the application.</summary>
    /// <value>Fixed result: DNNCORP.CE.</value>
    string Name { get; }

    /// <summary>Gets the SKU (Stock Keeping Unit).</summary>
    /// <value>Fixed result: DNN.</value>
    string SKU { get; }

    /// <summary>Gets the status of the DotnetNuke application.</summary>
    /// <remarks>
    /// If the value is not be Stable, you will see the exactly status and version in page's title if allow display beta message in host setting.
    /// </remarks>
    /// <value>
    /// The value can be: None, Alpha, Beta, RC, Stable.
    /// </value>
    ReleaseMode Status { get; }

    /// <summary>Gets the title of the application.</summary>
    /// <value>Fixed value: DotNetNuke.</value>
    string Title { get; }

    /// <summary>Gets the trademark.</summary>
    /// <value>Fixed value: DotNetNuke,DNN.</value>
    string Trademark { get; }

    /// <summary>Gets the type of the application.</summary>
    /// <value>Fixed value: Framework.</value>
    string Type { get; }

    /// <summary>Gets the upgrade URL.</summary>
    /// <value>Fixed value: https://dnnplatform.io. </value>
    string UpgradeUrl { get; }

    /// <summary>Gets the URL of the application.</summary>
    /// <value>Fixed value: https://dnncommunity.org.</value>
    string Url { get; }

    /// <summary>Gets the version of the DotNetNuke framework/application.</summary>
    /// <value>The version as retreieved from the Executing assembly.</value>
    Version Version { get; }

    /// <summary>  Determine whether a product specific change is to be applied.</summary>
    /// <param name="productNames">list of product names.</param>
    /// <returns>true if product is within list of names.</returns>
    bool ApplyToProduct(string productNames);
}
