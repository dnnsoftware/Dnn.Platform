// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.FileSystem
{
    internal interface IIconController
    {
        string IconURL(string key);

        string IconURL(string key, string size);
    }
}
