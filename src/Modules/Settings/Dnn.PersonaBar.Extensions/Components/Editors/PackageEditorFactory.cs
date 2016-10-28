using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public sealed class PackageEditorFactory
    {
        public static IPackageEditor GetPackageEditor(string packageType)
        {
            switch (packageType.ToLowerInvariant())
            {
                case "module":
                    return new ModulePackageEditor();
                case "authsystem":
                    return new AuthSystemPackageEditor();
                case "javascriptlibrary":
                    return new JsLibraryPackageEditor();
                case "corelanguagepack":
                case "extensionlanguagepack":
                    return new LanguagePackageEditor();
                case "skin":
                    return new SkinPackageEditor();
                case "skinobject":
                    return new SkinObjectPackageEditor();
                default:
                    return null;
            }
        }
    }
}