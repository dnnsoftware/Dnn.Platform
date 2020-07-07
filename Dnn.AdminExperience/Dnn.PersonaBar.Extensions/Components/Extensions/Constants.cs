// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components
{
    public enum PackageTypes
    {
        Generic = 0,
        AuthSystem,
        AuthenticationSystem = AuthSystem,
        Auth_System = AuthSystem,
        Container,
        CoreLanguagePack,
        DashboardControl,
        ExtensionLanguagePack,
        EvoqConnector,
        JavascriptLibrary,
        Javascript_Library = JavascriptLibrary,
        Language,
        Library,
        Module,
        PersonaBar,
        Provider,
        Skin,
        SkinObject,
        Widget,
    }

    public enum FileType
    {
        Control,
        Template,
        Manifest
    }

    public enum CreateModuleType
    {
        New,
        Control,
        Manifest
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
        internal const string DefaultDashboardImage = "icon_dashboard.png";

        internal const string DnnAuthTypeName = "DNN";
        internal const string SharedResources = Library.Constants.PersonaBarRelativePath + "/Modules/Dnn.Extensions/App_LocalResources/Extensions.resx";
    }
}
