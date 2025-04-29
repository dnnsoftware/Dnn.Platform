// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.GeneratedImage;

using System.Web;

internal interface IImageStore
{
    void Add(string id, byte[] data);

    bool TryTransmitIfContains(string id, HttpResponseBase response);

    void ForcePurgeFromServerCache(string cacheId);
}
