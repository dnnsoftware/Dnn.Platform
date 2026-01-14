// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components
{
    using System.Diagnostics.CodeAnalysis;

    public enum PackageTypes
    {
        /// <summary>A generic package.</summary>
        Generic = 0,

        /// <summary>An authentication system.</summary>
        AuthSystem = 1,

        /// <summary>An authentication system.</summary>
        AuthenticationSystem = AuthSystem,

        /// <summary>An authentication system.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        Auth_System = AuthSystem,

        /// <summary>A container pack.</summary>
        Container = 2,

        /// <summary>A core language pack.</summary>
        CoreLanguagePack = 3,

        /// <summary>A language pack for an extension.</summary>
        ExtensionLanguagePack = 4,

        /// <summary>A connector.</summary>
        EvoqConnector = 5,

        /// <summary>A JavaScript library.</summary>
        JavascriptLibrary = 6,

        /// <summary>A JavaScript library.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        Javascript_Library = JavascriptLibrary,

        /// <summary>A language pack.</summary>
        Language = 7,

        /// <summary>A library.</summary>
        Library = 8,

        /// <summary>A module.</summary>
        Module = 9,

        /// <summary>A Persona Bar extension/page.</summary>
        PersonaBar = 10,

        /// <summary>A provider.</summary>
        Provider = 11,

        /// <summary>A skin/theme.</summary>
        Skin = 12,

        /// <summary>A skin/theme object.</summary>
        SkinObject = 13,

        /// <summary>A widget.</summary>
        Widget = 14,
    }

    public enum FileType
    {
        /// <summary>A control.</summary>
        Control = 0,

        /// <summary>A template.</summary>
        Template = 1,

        /// <summary>A manifest.</summary>
        Manifest = 2,
    }

    public enum CreateModuleType
    {
        /// <summary>A new module.</summary>
        New = 0,

        /// <summary>A module control.</summary>
        Control = 1,

        /// <summary>A module manifest.</summary>
        Manifest = 2,
    }

    public class Constants
    {
        internal const string DefaultExtensionImage = "icon_extensions_32px.png";
        internal const string DefaultLanguageImage = "icon_languagePack.gif";
        internal const string DefaultAuthenicationImage = "icon_authentication.png";
        internal const string DefaultContainerImage = "icon_container.gif";
        internal const string DefaultSkinImage = "icon_skin.gif";
        internal const string DefaultProviderImage = "icon_provider.gif";
        internal const string DefaultLibraryImage = "icon_library.png";
        internal const string DefaultWidgetImage = "icon_widget.png";
        internal const string DnnAuthTypeName = "DNN";
        internal const string SharedResources = Library.Constants.PersonaBarRelativePath + "/Modules/Dnn.Extensions/App_LocalResources/Extensions.resx";
    }
}
