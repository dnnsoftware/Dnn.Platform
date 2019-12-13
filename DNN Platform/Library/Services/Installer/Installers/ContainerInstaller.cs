#region Usings

using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ContainerInstaller installs Container Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ContainerInstaller : SkinInstaller
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("containerFiles")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "containerFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("containerFile")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "containerFile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the SkinName Node ("containerName")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string SkinNameNodeName
        {
            get
            {
                return "containerName";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the RootName of the Skin
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string SkinRoot
        {
            get
            {
                return SkinController.RootContainer;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Type of the Skin
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string SkinType
        {
            get
            {
                return "Container";
            }
        }
    }
}
