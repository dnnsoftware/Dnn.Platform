// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Settings;

using System.Reflection;

using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Internal.SourceGenerators;

using Microsoft.Extensions.DependencyInjection;

[DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead")]
public partial class SerializationController
{
    private static ISerializationManager SerializationManager =>
        Globals.DependencyProvider.GetRequiredService<ISerializationManager>();

    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead")]
    public static partial string SerializeProperty<T>(T myObject, PropertyInfo property) =>
        SerializationManager.SerializeProperty(myObject, property);

    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead")]
    public static partial string SerializeProperty<T>(T myObject, PropertyInfo property, string serializer) =>
        SerializationManager.SerializeProperty(myObject, property, serializer);

    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead")]
    public static partial void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue)
        where T : class, new() => SerializationManager.DeserializeProperty(myObject, property, propertyValue);

    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.ISerializationManager' instead")]
    public static partial void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue, string serializer)
        where T : class, new() => SerializationManager.DeserializeProperty(myObject, property, propertyValue, serializer);
}
