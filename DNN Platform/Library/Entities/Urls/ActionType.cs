// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

public enum ActionType
{
    IgnoreRequest = 0,
    Continue = 1,
    Redirect302Now = 2,
    Redirect301 = 3,
    CheckFor301 = 4,
    Redirect302 = 5,
    Output404 = 6,
    Output500 = 7,
}
