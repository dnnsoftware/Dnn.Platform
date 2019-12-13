// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Web.UI
{
    public interface ILocalizable
    {
        string LocalResourceFile { get; set; }
        bool Localize { get; set; }

        void LocalizeStrings();
    }
}
