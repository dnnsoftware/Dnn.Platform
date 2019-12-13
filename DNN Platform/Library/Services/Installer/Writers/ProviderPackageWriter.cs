#region Usings

using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProviderPackageWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProviderPackageWriter : PackageWriterBase
    {
        public ProviderPackageWriter(PackageInfo package) : base(package)
        {
            XmlDocument configDoc = Config.Load();
            XPathNavigator providerNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*/providers/add[@name='" + package.Name + "']");
            string providerPath = Null.NullString;
            if (providerNavigator != null)
            {
                providerPath = Util.ReadAttribute(providerNavigator, "providerPath");
            }
            if (!string.IsNullOrEmpty(providerPath))
            {
                BasePath = providerPath.Replace("~/", "").Replace("/", "\\");
            }
        }

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
            base.GetFiles(includeSource, false);
        }
    }
}
