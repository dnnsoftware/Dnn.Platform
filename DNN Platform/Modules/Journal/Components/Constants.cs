// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using DotNetNuke.Services.Search.Internals;

    public class Constants
    {
        internal const string SharedResourcesPath = "~/DesktopModules/Journal/App_LocalResources/SharedResources.resx";
        internal const string DefaultPageSize = "Journal_PageSize";
        internal const string AllowFiles = "Journal_AllowFiles";
        internal const string AllowedFileTypes = "Journal_AllowedFileTypes";
        internal const string AllowPhotos = "Journal_AllowPhotos";
        internal const string AllowResizePhotos = "Journal_AllowResizePhotos";
        internal const string DefaultSecurity = "Journal_DefaultSecurity";
        internal const string MaxCharacters = "Journal_MaxCharacters";
        internal const string JournalFilters = "Journal_Filters";
        internal const string JournalFilterMode = "Journal_Mode";
        internal const string JournalEditorEnabled = "Journal_EditorEnabled";

        internal const int SearchBatchSize = 500;
    }
}
