using System;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public sealed class PackageEditorFactory
    {
        public static IPackageEditor GetPackageEditor(string packageType)
        {
            PackageTypes pkgType;
            Enum.TryParse(packageType, true, out pkgType);

            switch (pkgType)
            {
                case PackageTypes.Module:
                    return new ModulePackageEditor();
                case PackageTypes.AuthSystem:
                    return new AuthSystemPackageEditor();
                case PackageTypes.JavascriptLibrary:
                    return new JsLibraryPackageEditor();
                case PackageTypes.CoreLanguagePack:
                    return new CoreLanguagePackageEditor();
                case PackageTypes.ExtensionLanguagePack:
                    return new ExtensionLanguagePackageEditor();
                case PackageTypes.Container:
                case PackageTypes.Skin:
                    return new SkinPackageEditor();
                case PackageTypes.SkinObject:
                    return new SkinObjectPackageEditor();
            }
            return null;
        }
    }
}