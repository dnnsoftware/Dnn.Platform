// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules;

using System;

using DotNetNuke.Abstractions.Modules;
using DotNetNuke.Common;
using DotNetNuke.Framework;

/// <summary>The <see cref="IBusinessControllerProvider"/> implementation.</summary>
public class BusinessControllerProvider : IBusinessControllerProvider
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>Initializes a new instance of the <see cref="BusinessControllerProvider"/> class.</summary>
    /// <param name="serviceProvider">The service provider.</param>
    public BusinessControllerProvider(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public T GetInstance<T>(string businessControllerTypeName)
        where T : class
    {
        if (string.IsNullOrEmpty(businessControllerTypeName))
        {
            return null;
        }

        return this.GetInstance<T>(Reflection.CreateType(businessControllerTypeName));
    }

    /// <inheritdoc/>
    public T GetInstance<T>(Type businessControllerType)
        where T : class
    {
        Requires.NotNull(nameof(businessControllerType), businessControllerType);
        if (typeof(T).IsAssignableFrom(businessControllerType))
        {
            return (T)this.GetInstance(businessControllerType);
        }

        return null;
    }

    /// <inheritdoc/>
    public object GetInstance(string businessControllerTypeName)
    {
        if (string.IsNullOrEmpty(businessControllerTypeName))
        {
            return null;
        }

        return this.GetInstance(Reflection.CreateType(businessControllerTypeName));
    }

    /// <inheritdoc/>
    public object GetInstance(Type businessControllerType)
    {
        return Reflection.CreateObject(this.serviceProvider, businessControllerType);
    }
}
