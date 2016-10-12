using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public sealed class PackageEditorFactory
    {
        public static IPackageEditor GetPackageEditor(PackageTypes packageType)
        {
            switch (packageType)
            {
                case PackageTypes.Module:
                    return new ModulePackageEditor();
                default:
                    return null;
            }
        }
    }
}