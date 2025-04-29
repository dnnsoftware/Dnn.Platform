// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities;

/// <summary>
/// The CacheItemExpiredCallback delegate defines a callback method that notifies
/// the application when a CacheItem is Expired (when an attempt is made to get the item).
/// </summary>
/// <param name="dataArgs">The args.</param>
/// <returns>The object to cache.</returns>
public delegate object CacheItemExpiredCallback(CacheItemArgs dataArgs);
