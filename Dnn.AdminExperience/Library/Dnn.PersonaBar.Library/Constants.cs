// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.PersonaBar.Library
{
    public static class Constants
    {
        public const string LocalResourcesFile = DotNetNuke.Prompt.Common.Constants.DefaultPromptResourceFile;
        public const string PersonaBarRelativePath = "~/DesktopModules/admin/Dnn.PersonaBar/";
        public const string PersonaBarModulesPath = PersonaBarRelativePath + "Modules/";

        public const string SharedResources = PersonaBarRelativePath + "/App_LocalResources/SharedResources.resx";

        public const int AvatarWidth = 64;
        public const int AvatarHeight = 64;

        public static readonly TimeSpan ThreeSeconds = TimeSpan.FromSeconds(3);
        public static readonly TimeSpan ThirtySeconds = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan TenMinutes = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan HalfHour = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
        public static readonly TimeSpan FourHours = TimeSpan.FromHours(1);
        public static readonly TimeSpan TwelveHours = TimeSpan.FromHours(12);
        public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
        public static readonly TimeSpan OneWeek = TimeSpan.FromDays(7);
        
        public const string AdminsRoleName = "Administrators";
    }
}
