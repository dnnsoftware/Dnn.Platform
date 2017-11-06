#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace Dnn.PersonaBar.Library
{
    public static class Constants
    {
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
