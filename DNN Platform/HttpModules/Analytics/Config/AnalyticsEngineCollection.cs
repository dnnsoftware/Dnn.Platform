// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Config;

using System;
using System.Collections;

using DotNetNuke.Collections;

/// <summary>A collection of <see cref="AnalyticsEngine"/> instances.</summary>
[Serializable]
public class AnalyticsEngineCollection : GenericCollectionBase<AnalyticsEngine>
{
    /// <inheritdoc cref="ArrayList.Add"/>
    public new void Add(AnalyticsEngine a) => base.Add(a);
}
