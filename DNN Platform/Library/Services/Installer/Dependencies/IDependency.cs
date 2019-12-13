#region Usings

using System.Xml.XPath;

#endregion

namespace DotNetNuke.Services.Installer.Dependencies
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The IDependency Interface defines the contract for a Package Dependency
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public interface IDependency
    {
        string ErrorMessage { get; }
        bool IsValid { get; }

        void ReadManifest(XPathNavigator dependencyNav);
    }
}
