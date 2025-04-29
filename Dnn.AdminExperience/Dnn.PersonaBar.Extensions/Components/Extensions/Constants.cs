// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components;

using System;

public enum PackageTypes
{
    Generic = 0,
    AuthSystem = 1,
    AuthenticationSystem = AuthSystem,
    Auth_System = AuthSystem,
    Container = 2,
    CoreLanguagePack = 3,
    ExtensionLanguagePack = 4,
    EvoqConnector = 5,
    JavascriptLibrary = 6,
    Javascript_Library = JavascriptLibrary,
    Language = 7,
    Library = 8,
    Module = 9,
    PersonaBar = 10,
    Provider = 11,
    Skin = 12,
    SkinObject = 13,
    Widget = 14,
}

public enum FileType
{
    Control = 0,
    Template = 1,
    Manifest = 2,
}

public enum CreateModuleType
{
    New = 0,
    Control = 1,
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
