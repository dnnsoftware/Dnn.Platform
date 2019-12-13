#region Usings

using System.Xml;

using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ContainerPackageWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ContainerPackageWriter : SkinPackageWriter
    {
        public ContainerPackageWriter(PackageInfo package) : base(package)
        {
            BasePath = "Portals\\_default\\Containers\\" + SkinPackage.SkinName;
        }

        public ContainerPackageWriter(SkinPackageInfo skinPackage, PackageInfo package) : base(skinPackage, package)
        {
            BasePath = "Portals\\_default\\Containers\\" + skinPackage.SkinName;
        }

        protected override void WriteFilesToManifest(XmlWriter writer)
        {
            var containerFileWriter = new ContainerComponentWriter(SkinPackage.SkinName, BasePath, Files, Package);
            containerFileWriter.WriteManifest(writer);
        }
    }
}
