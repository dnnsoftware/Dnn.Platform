// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Urls
{
    public enum ActionType
    {
        IgnoreRequest,
        Continue,
        Redirect302Now,
        Redirect301,
        CheckFor301,
        Redirect302,
        Output404,
        Output500
    }
}
