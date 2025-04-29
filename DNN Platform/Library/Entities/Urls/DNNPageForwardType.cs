// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

public enum DNNPageForwardType
{
    NoForward = 0,
    Redirect302 = 1,
    Redirect301 = 2,
}
