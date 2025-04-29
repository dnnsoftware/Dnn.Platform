// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.DependencyInjection.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotNetNuke.Instrumentation;
using DotNetNuke.Internal.SourceGenerators;

/// <summary><see cref="Type"/> specific extensions to be used in Dependency Injection invocations.</summary>
public static partial class TypeExtensions
{
    /// <summary>Safely Get all Types from the assembly. If there is an error while retrieving the types it will return an empty array of <see cref="Type"/>.</summary>
    /// <param name="assembly">The assembly to retrieve all types from.</param>
    /// <returns>An array of all <see cref="Type"/> in the given <see cref="Assembly"/>.</returns>
    /// <remarks>This is obsolete because logging is not added. Please use the SafeGetTypes with the ILog parameter.</remarks>
    [DnnDeprecated(9, 9, 0, "Please use the SafeGetTypes overload with the ILog parameter")]
    public static partial Type[] SafeGetTypes(this Assembly assembly)
    {
        return assembly.SafeGetTypes(null);
    }

    /// <summary>Safely Get all Types from the assembly. If there is an error while retrieving the types it will return an empty array of <see cref="Type"/>.</summary>
    /// <param name="assembly">The assembly to retrieve all types from.</param>
    /// <param name="logger">A optional <see cref="ILog"/> object. This will log issues loading the types.</param>
    /// <returns>An array of all <see cref="Type"/> in the given <see cref="Assembly"/>.</returns>
    public static Type[] SafeGetTypes(this Assembly assembly, ILog logger)
    {
        var (types, exception) = assembly.GetTypesAndException();
        if (logger is null || exception is null)
        {
            return types.ToArray();
        }

        if (exception is ReflectionTypeLoadException loadException)
        {
            var messageBuilder = BuildLoaderExceptionMessage(
                new StringBuilder($"Unable to get all types for {assembly.FullName}, see exception for details").AppendLine(),
                assembly,
                loadException);

            logger.Warn(messageBuilder.ToString(), loadException);
        }
        else
        {
            logger.Error($"Unable to get any types for {assembly.FullName}, see exception for details", exception);
        }

        return types.ToArray();
    }

    /// <summary>Safely get all types from all assemblies.</summary>
    /// <returns>A sequence of all available <see cref="Type"/> instances, along with errors and warnings.</returns>
    public static TypesResult SafeGetTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .OrderBy(DotNetNukeFirstThenDnnThenOthers)
            .ThenBy(assembly => assembly.FullName)
            .SafeGetTypes();

        static int DotNetNukeFirstThenDnnThenOthers(Assembly assembly)
        {
            if (assembly.FullName.StartsWith("DotNetNuke", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (assembly.FullName.StartsWith("DNN", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            return 2;
        }
    }

    /// <summary>Safely get all types from a set of assemblies.</summary>
    /// <param name="assemblies">The assemblies.</param>
    /// <returns>A sequence of all available <see cref="Type"/> instances, along with errors and warnings.</returns>
    public static TypesResult SafeGetTypes(this IEnumerable<Assembly> assemblies)
    {
        var loadExceptions = new Dictionary<Assembly, ReflectionTypeLoadException>();
        var otherExceptions = new Dictionary<Assembly, Exception>();
        var types = assemblies.SafeGetTypes(loadExceptions, otherExceptions);
        return new TypesResult(types.ToList(), loadExceptions, otherExceptions);
    }

    /// <summary>Logs the exceptions in <see cref="TypesResult.OtherExceptions"/> of <paramref name="result" /> using <paramref name="logger"/>.</summary>
    /// <param name="result">The types result to log.</param>
    /// <param name="logger">The logger.</param>
    public static void LogOtherExceptions(this TypesResult result, ILog logger)
    {
        if (logger is null)
        {
            return;
        }

        foreach (var exceptionPair in result.OtherExceptions)
        {
            var assembly = exceptionPair.Key;
            var exception = exceptionPair.Value;
            logger.Error($"Unable to get any types for {assembly.FullName}, see exception for details", exception);
        }
    }

    /// <summary>Builds a message to load for a collection of <see cref="ReflectionTypeLoadException"/> instances.</summary>
    /// <param name="loadExceptions">A dictionary mapping <see cref="Assembly"/> to <see cref="ReflectionTypeLoadException"/>.</param>
    /// <returns>A <see cref="StringBuilder"/> containing the message.</returns>
    public static StringBuilder BuildLoaderExceptionsMessage(this IReadOnlyDictionary<Assembly, ReflectionTypeLoadException> loadExceptions)
    {
        return
            loadExceptions.Aggregate(
                new StringBuilder("Unable to get all types for some assemblies:").AppendLine(),
                (builder, exceptionPair) =>
                {
                    var assembly = exceptionPair.Key;
                    var loadException = exceptionPair.Value;
                    return BuildLoaderExceptionMessage(builder, assembly, loadException).AppendLine();
                });
    }

    private static StringBuilder BuildLoaderExceptionMessage(StringBuilder builder, Assembly assembly, ReflectionTypeLoadException loadException)
    {
        var foundTypes = loadException.Types.Count(x => x != null);
        var allTypes = loadException.Types.Length;
        var loaderMessages = loadException.LoaderExceptions.Select(e => e.Message).Distinct();

        return loaderMessages.Aggregate(
            builder.AppendLine($"- Found {foundTypes} of {allTypes} types from {assembly.FullName}"),
            (theBuilder, message) => theBuilder.AppendLine($"    - {message}"));
    }

    private static IEnumerable<Type> SafeGetTypes(this IEnumerable<Assembly> assemblies, Dictionary<Assembly, ReflectionTypeLoadException> loadExceptions, Dictionary<Assembly, Exception> otherExceptions)
    {
        return assemblies.SelectMany(assembly =>
        {
            var (types, exception) = assembly.GetTypesAndException();
            if (exception is ReflectionTypeLoadException loadException)
            {
                loadExceptions.Add(assembly, loadException);
            }
            else if (exception is not null)
            {
                otherExceptions.Add(assembly, exception);
            }

            return types;
        });
    }

    private static (IEnumerable<Type> Types, Exception Exception) GetTypesAndException(this Assembly assembly)
    {
        Exception exception = null;
        IEnumerable<Type> types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            exception = ex;

            // Ensure that DNN obtains all types that were loaded, ignoring the failure(s)
            types = ex.Types.Where(x => x != null);
        }
        catch (Exception ex)
        {
            exception = ex;
            types = Enumerable.Empty<Type>();
        }

        return (types.OrderBy(type => type.FullName ?? type.Name), exception);
    }
}
