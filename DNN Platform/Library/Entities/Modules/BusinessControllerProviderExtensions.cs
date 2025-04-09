// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules;

using DotNetNuke.Abstractions.Modules;
using DotNetNuke.UI.Modules;

/// <summary>Extension methods for <see cref="IBusinessControllerProvider"/>.</summary>
public static class BusinessControllerProviderExtensions
{
    /// <summary>Gets an instance of the business controller, if it implements <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type the business controller must implement.</typeparam>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="desktopModule">The desktop module.</param>
    /// <returns>An instance of <typeparamref name="T"/>, or <see langword="null"/> if the business controller does not implement <typeparamref name="T"/> or the <see cref="DesktopModuleInfo.BusinessControllerClass"/> is <see langword="null"/> or empty.</returns>
    public static T GetInstance<T>(this IBusinessControllerProvider businessControllerProvider, DesktopModuleInfo desktopModule)
        where T : class
        => businessControllerProvider.GetInstance<T>(desktopModule.BusinessControllerClass);

    /// <summary>Gets an instance of the business controller.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="desktopModule">The desktop module.</param>
    /// <returns>An instance of the business controller class, or <see langword="null"/> if the <see cref="DesktopModuleInfo.BusinessControllerClass"/> is <see langword="null"/> or empty.</returns>
    public static object GetInstance(this IBusinessControllerProvider businessControllerProvider, DesktopModuleInfo desktopModule)
        => businessControllerProvider.GetInstance(desktopModule.BusinessControllerClass);

    /// <summary>Gets an instance of the business controller, if it implements <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type the business controller must implement.</typeparam>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="module">The module.</param>
    /// <returns>An instance of <typeparamref name="T"/>, or <see langword="null"/> if the business controller does not implement <typeparamref name="T"/> or the <see cref="DesktopModuleInfo.BusinessControllerClass"/> is <see langword="null"/> or empty.</returns>
    public static T GetInstance<T>(this IBusinessControllerProvider businessControllerProvider, ModuleInfo module)
        where T : class
        => businessControllerProvider.GetInstance<T>(module.DesktopModule);

    /// <summary>Gets an instance of the business controller.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="module">The module.</param>
    /// <returns>An instance of the business controller class, or <see langword="null"/> if the <see cref="DesktopModuleInfo.BusinessControllerClass"/> is <see langword="null"/> or empty.</returns>
    public static object GetInstance(this IBusinessControllerProvider businessControllerProvider, ModuleInfo module)
        => businessControllerProvider.GetInstance(module.DesktopModule);

    /// <summary>Gets an instance of the business controller, if it implements <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type the business controller must implement.</typeparam>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="moduleContext">The module context.</param>
    /// <returns>An instance of <typeparamref name="T"/>, or <see langword="null"/> if the business controller does not implement <typeparamref name="T"/> or the <see cref="DesktopModuleInfo.BusinessControllerClass"/> is <see langword="null"/> or empty.</returns>
    public static T GetInstance<T>(this IBusinessControllerProvider businessControllerProvider, ModuleInstanceContext moduleContext)
        where T : class
        => businessControllerProvider.GetInstance<T>(moduleContext.Configuration);

    /// <summary>Gets an instance of the business controller.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="moduleContext">The module context.</param>
    /// <returns>An instance of the business controller class, or <see langword="null"/> if the <see cref="DesktopModuleInfo.BusinessControllerClass"/> is <see langword="null"/> or empty.</returns>
    public static object GetInstance(this IBusinessControllerProvider businessControllerProvider, ModuleInstanceContext moduleContext)
        => businessControllerProvider.GetInstance(moduleContext.Configuration);
}
