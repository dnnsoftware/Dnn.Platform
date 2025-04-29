// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail.OAuth;

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.DependencyInjection.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>SMTP OAuth controller.</summary>
public class SmtpOAuthController : ISmtpOAuthController
{
    private readonly IEnumerable<ISmtpOAuthProvider> smtpOAuthProviders;

    /// <summary>Initializes a new instance of the <see cref="SmtpOAuthController"/> class.</summary>
    /// <param name="smtpOAuthProviders">The SMTP OAuth providers.</param>
    public SmtpOAuthController(IEnumerable<ISmtpOAuthProvider> smtpOAuthProviders)
    {
        this.smtpOAuthProviders = smtpOAuthProviders;
    }

    /// <summary>Registers all of the <see cref="ISmtpOAuthProvider"/> types with the <paramref name="serviceCollection"/>.</summary>
    /// <param name="serviceCollection">The services collection.</param>
    public static void RegisterOAuthProviders(IServiceCollection serviceCollection)
    {
        var providerTypes = TypeExtensions.SafeGetTypes()
            .Types.Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ISmtpOAuthProvider).IsAssignableFrom(t));

        serviceCollection.TryAddEnumerable(providerTypes.Select(t => ServiceDescriptor.Transient(typeof(ISmtpOAuthProvider), t)));
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ISmtpOAuthProvider> GetOAuthProviders()
    {
        return this.smtpOAuthProviders.ToList();
    }

    /// <inheritdoc />
    public ISmtpOAuthProvider GetOAuthProvider(string name)
    {
        return this.smtpOAuthProviders.FirstOrDefault(i => i.Name == name);
    }
}
