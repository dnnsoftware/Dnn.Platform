// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.DependencyInjection.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Filters;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

/// <summary>Adds property injection extension methods.</summary>
internal static class BuildUpExtensions
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BuildUpExtensions));

    /// <summary>Injects property dependency for properties that are decorated with <see cref="DependencyAttribute"/>.</summary>
    /// <param name="container">The service provider.</param>
    /// <param name="filter">The <see cref="IFilter"/> to inject properties.</param>
    internal static void BuildUp(this IServiceProvider container, IFilter filter)
    {
        if (container == null || filter == null)
        {
            return;
        }

        var properties = GetDependencyProperties(filter.GetType());
        foreach (var property in properties)
        {
            var service = container.GetService(property.PropertyType);
            if (service != null)
            {
                try
                {
                    property.SetValue(filter, service);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
            }
        }

        IEnumerable<PropertyInfo> GetDependencyProperties(Type type)
        {
            return CBO.Instance.GetCachedObject<IEnumerable<PropertyInfo>>(
                new CacheItemArgs($"WebApiDependencyProperties_{type.FullName}"),
                (args) => type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(propertyInfo => propertyInfo.GetSetMethod(true) != null && propertyInfo.GetCustomAttribute<DependencyAttribute>() != null)
                    .ToList(),
                saveInDictionary: false);
        }
    }
}
