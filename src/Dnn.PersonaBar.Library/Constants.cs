#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

namespace Dnn.PersonaBar.Library
{
    public static class Constants
    {
        public const string PersonaBarRelativePath = "~/admin/Dnn.PersonaBar/";

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
        
        public const string DisallowedCharacters = "%?*&;:'\\";

        public const string AdminsRoleName = "Administrators";
        public const string CommunityManagerRoleName = "Community Manager";
        public const string ContentEditorRoleName = "Content Editors";
        public const string ContentManagerRoleName = "Content Managers";
        public const string ModeratorsRoleName = "Moderators";
        public const string ManagerRoles = "Content Managers,Community Manager, Administrators";
        public const string AllMajorRoles = "Content Managers,Community Manager,Content Editors, Administrators";

        public const string PlatformBasicSku = "DNN";
        public const string EvoqContentBasicSku = "DNNPRO";
        public const string EvoqContentSku = "DNNENT";
        public const string EvoqEngageSku = "DNNSOCIAL";

        // Js resources

        public static readonly string[] DefaultDocumentExtensions = { "doc", "docx", "pdf", "xls", "xlsx", "ppt", "pptx", "txt" };

        public const string PageTagsVocabulary = "PageTags";

        public const string TemplatesTabName = "ContentPageTemplates";
    }
}
