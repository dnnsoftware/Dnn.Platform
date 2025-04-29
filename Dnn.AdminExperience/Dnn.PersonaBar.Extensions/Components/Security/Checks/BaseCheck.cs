// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks;

using System;

using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

/// <summary>Base class for security checks.</summary>
public abstract class BaseCheck : IAuditCheck
{
    private readonly Lazy<ILog> logger;

    /// <summary>Initializes a new instance of the <see cref="BaseCheck"/> class.</summary>
    public BaseCheck()
    {
        this.logger = new Lazy<ILog>(() => LoggerSource.Instance.GetLogger(this.GetType()));
    }

    /// <inheritdoc cref="IAuditCheck.Id" />
    public virtual string Id => this.GetType().Name;

    /// <inheritdoc cref="IAuditCheck.LazyLoad" />
    public virtual bool LazyLoad => false;

    /// <summary>Gets the path to the resources file (.resx).</summary>
    protected virtual string LocalResourceFile =>
        "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Security/App_LocalResources/Security.resx";

    /// <summary>Gets a typed instance of the <see cref="ILog"/> interface.</summary>
    protected virtual ILog Logger => this.logger.Value;

    /// <inheritdoc cref="IAuditCheck.Execute" />
    public virtual CheckResult Execute()
    {
        try
        {
            return this.ExecuteInternal();
        }
        catch (Exception ex)
        {
            this.Logger.Error($"{this.Id} failed.", ex);
            return this.Unverified("An internal error occurred. See logs for details.");
        }
    }

    /// <summary>Performs the actual security check.</summary>
    /// <returns>A <see cref="CheckResult"/> with the outcome of the security check.</returns>
    protected abstract CheckResult ExecuteInternal();

    /// <inheritdoc cref="Localization.GetString(string, string)" />
    protected virtual string GetLocalizedString(string key)
    {
        return Localization.GetString(this.Id + key, this.LocalResourceFile);
    }

    /// <summary>Returns an "unverified" result.</summary>
    /// <param name="reason">The reason why we are returning an unverified result.
    /// This text will be displayed in the Notes column.</param>
    /// <returns>A <see cref="CheckResult"/> with a <see cref="CheckResult.Severity"/>
    /// of <see cref="SeverityEnum.Unverified"/>.</returns>
    protected virtual CheckResult Unverified(string reason)
    {
        return new CheckResult(SeverityEnum.Unverified, this.Id)
        {
            Notes = { reason },
        };
    }
}
