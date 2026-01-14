// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Modules
{
    using System;

    /// <summary>A contract specifying the ability to create instances of a module's business controller class.</summary>
    public interface IBusinessControllerProvider
    {
        /// <summary>Gets an instance of the business controller, if it implements <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">The type the business controller must implement.</typeparam>
        /// <param name="businessControllerTypeName">The name of the business controller type.</param>
        /// <returns>An instance of <typeparamref name="T"/>, or <see langword="null"/> if the business controller does not implement <typeparamref name="T"/> or the <paramref name="businessControllerTypeName"/> is <see langword="null"/> or empty.</returns>
        T GetInstance<T>(string businessControllerTypeName)
            where T : class;

        /// <summary>Gets an instance of the business controller, if it implements <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">The type the business controller must implement.</typeparam>
        /// <param name="businessControllerType">The type of the business controller.</param>
        /// <returns>An instance of <typeparamref name="T"/>, or <see langword="null"/> if the business controller does not implement <typeparamref name="T"/>.</returns>
        T GetInstance<T>(Type businessControllerType)
            where T : class;

        /// <summary>Gets an instance of the business controller.</summary>
        /// <param name="businessControllerTypeName">The name of the business controller type.</param>
        /// <returns>An instance of the business controller, or <see langword="null"/> if the <paramref name="businessControllerTypeName"/> is <see langword="null"/> or empty.</returns>
        object GetInstance(string businessControllerTypeName);

        /// <summary>Gets an instance of the business controller.</summary>
        /// <param name="businessControllerType">The type of the business controller.</param>
        /// <returns>An instance of the business controller.</returns>
        object GetInstance(Type businessControllerType);
    }
}
