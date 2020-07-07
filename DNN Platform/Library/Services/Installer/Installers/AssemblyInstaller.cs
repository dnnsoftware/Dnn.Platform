// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Text.RegularExpressions;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AssemblyInstaller installs Assembly Components to a DotNetNuke site.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class AssemblyInstaller : FileInstaller
    {
        private static readonly Regex PublicKeyTokenRegex = new Regex(@"PublicKeyToken=(\w+)", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly string OldVersion = "0.0.0.0-" + new Version(short.MaxValue, short.MaxValue, short.MaxValue, short.MaxValue);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "dll,pdb";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("assemblies").
        /// </summary>
        /// <value>A String.</value>
        protected override string CollectionNodeName
        {
            get
            {
                return "assemblies";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the default Path for the file - if not present in the manifest.
        /// </summary>
        /// <value>A String.</value>
        protected override string DefaultPath
        {
            get
            {
                return "bin\\";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("assembly").
        /// </summary>
        /// <value>A String.</value>
        protected override string ItemNodeName
        {
            get
            {
                return "assembly";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the PhysicalBasePath for the assemblies.
        /// </summary>
        /// <value>A String.</value>
        protected override string PhysicalBasePath
        {
            get
            {
                return this.PhysicalSitePath + "\\";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteFile method deletes a single assembly.
        /// </summary>
        /// <param name="file">The InstallFile to delete.</param>
        protected override void DeleteFile(InstallFile file)
        {
            // Attempt to unregister assembly this will return False if the assembly is used by another package and
            // cannot be delete andtrue if it is not being used and can be deleted
            if (DataProvider.Instance().UnRegisterAssembly(this.Package.PackageID, file.Name))
            {
                this.Log.AddInfo(Util.ASSEMBLY_UnRegistered + " - " + file.FullName);

                this.RemoveBindingRedirect(file);

                // Call base class version to deleteFile file from \bin
                base.DeleteFile(file);
            }
            else
            {
                this.Log.AddInfo(Util.ASSEMBLY_InUse + " - " + file.FullName);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that determines what type of file this installer supports.
        /// </summary>
        /// <param name="type">The type of file being processed.</param>
        /// <returns></returns>
        protected override bool IsCorrectType(InstallFileType type)
        {
            return type == InstallFileType.Assembly;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The InstallFile method installs a single assembly.
        /// </summary>
        /// <param name="file">The InstallFile to install.</param>
        /// <returns></returns>
        protected override bool InstallFile(InstallFile file)
        {
            bool bSuccess = true;
            if (file.Action == "UnRegister")
            {
                this.DeleteFile(file);
            }
            else
            {
                // Attempt to register assembly this will return False if the assembly exists and true if it does not or is older
                int returnCode = DataProvider.Instance().RegisterAssembly(this.Package.PackageID, file.Name, file.Version.ToString(3));
                switch (returnCode)
                {
                    case 0:
                        // Assembly Does Not Exist
                        this.Log.AddInfo(Util.ASSEMBLY_Added + " - " + file.FullName);
                        break;
                    case 1:
                        // Older version of Assembly Exists
                        this.Log.AddInfo(Util.ASSEMBLY_Updated + " - " + file.FullName);
                        break;
                    case 2:
                    case 3:
                        // Assembly already Registered
                        this.Log.AddInfo(Util.ASSEMBLY_Registered + " - " + file.FullName);
                        break;
                }

                // If assembly not registered, is newer (or is the same version and we are in repair mode)
                if (returnCode < 2 || (returnCode == 2 && file.InstallerInfo.RepairInstall))
                {
                    // Call base class version to copy file to \bin
                    bSuccess = base.InstallFile(file);
                    this.AddOrUpdateBindingRedirect(file);
                }
            }

            return bSuccess;
        }

        /// <summary>Reads the file's <see cref="AssemblyName"/>.</summary>
        /// <param name="assemblyFile">The path for the assembly whose <see cref="AssemblyName"/> is to be returned.</param>
        /// <returns>An <see cref="AssemblyName"/> or <c>null</c>.</returns>
        private static AssemblyName ReadAssemblyName(string assemblyFile)
        {
            try
            {
                return AssemblyName.GetAssemblyName(assemblyFile);
            }
            catch (BadImageFormatException)
            {
                // assemblyFile is not a valid assembly.
                return null;
            }
            catch (ArgumentException)
            {
                // assemblyFile is invalid, such as an assembly with an invalid culture.
                return null;
            }
            catch (SecurityException)
            {
                // The caller does not have path discovery permission.
                return null;
            }
            catch (FileLoadException)
            {
                // An assembly or module was loaded twice with two different sets of evidence.
                return null;
            }
        }

        private static string ReadPublicKey(AssemblyName assemblyName)
        {
            if (assemblyName == null || !assemblyName.Flags.HasFlag(AssemblyNameFlags.PublicKey))
            {
                return null;
            }

            return PublicKeyTokenRegex.Match(assemblyName.FullName).Groups[1].Value;
        }

        /// <summary>Gets the XML merge document to create the binding redirect.</summary>
        /// <param name="xmlMergePath">The path to the template binding redirect XML Merge document.</param>
        /// <param name="name">The assembly name.</param>
        /// <param name="publicKeyToken">The assembly's public key token.</param>
        /// <param name="oldVersion">The old version range.</param>
        /// <param name="newVersion">The new version.</param>
        /// <returns>An <see cref="XmlDocument"/> instance.</returns>
        private static XmlDocument GetXmlMergeDoc(string xmlMergePath, string name, string publicKeyToken, string oldVersion, string newVersion)
        {
            var xmlMergeDoc = new XmlDocument { XmlResolver = null };
            xmlMergeDoc.Load(xmlMergePath);

            var namespaceManager = new XmlNamespaceManager(xmlMergeDoc.NameTable);
            namespaceManager.AddNamespace("ab", "urn:schemas-microsoft-com:asm.v1");

            var node = xmlMergeDoc.SelectSingleNode("/configuration/nodes/node", namespaceManager);
            ReplaceInAttributeValue(node, namespaceManager, "@path", "$$name$$", name);
            ReplaceInAttributeValue(node, namespaceManager, "@path", "$$publicKeyToken$$", publicKeyToken);

            var dependentAssembly = node.SelectSingleNode("ab:dependentAssembly", namespaceManager);
            if (dependentAssembly == null)
            {
                return xmlMergeDoc;
            }

            ReplaceInAttributeValue(node, namespaceManager, "@targetpath", "$$name$$", name);
            ReplaceInAttributeValue(node, namespaceManager, "@targetpath", "$$publicKeyToken$$", publicKeyToken);

            var assemblyIdentity = dependentAssembly.SelectSingleNode("ab:assemblyIdentity", namespaceManager);
            ReplaceInAttributeValue(assemblyIdentity, namespaceManager, "@name", "$$name$$", name);
            ReplaceInAttributeValue(assemblyIdentity, namespaceManager, "@publicKeyToken", "$$publicKeyToken$$", publicKeyToken);

            var bindingRedirect = dependentAssembly.SelectSingleNode("ab:bindingRedirect", namespaceManager);
            ReplaceInAttributeValue(bindingRedirect, namespaceManager, "@oldVersion", "$$oldVersion$$", oldVersion);
            ReplaceInAttributeValue(bindingRedirect, namespaceManager, "@newVersion", "$$newVersion$$", newVersion);

            return xmlMergeDoc;
        }

        /// <summary>Replaces the given text in the value of the attribute matched by <paramref name="xpath"/>.</summary>
        /// <param name="parentNode">The parent node in which to search via the <paramref name="xpath"/> expression.</param>
        /// <param name="namespaceManager">The namespace manager.</param>
        /// <param name="xpath">The xpath expression to get the attribute.</param>
        /// <param name="oldValue">The placeholder value to replace.</param>
        /// <param name="newValue">The real value with which to replace <paramref name="oldValue"/>.</param>
        private static void ReplaceInAttributeValue(XmlNode parentNode, XmlNamespaceManager namespaceManager, string xpath, string oldValue, string newValue)
        {
            var attribute = parentNode.SelectSingleNode(xpath, namespaceManager);
            attribute.Value = attribute.Value.Replace(oldValue, newValue);
        }

        /// <summary>Adds or updates the binding redirect for the assembly file, if the assembly file it strong-named.</summary>
        /// <param name="file">The assembly file.</param>
        private void AddOrUpdateBindingRedirect(InstallFile file)
        {
            if (this.ApplyXmlMerge(file, "BindingRedirect.config"))
            {
                this.Log.AddInfo(Util.ASSEMBLY_AddedBindingRedirect + " - " + file.FullName);
            }
        }

        /// <summary>Removes the binding redirect for the assembly file, if the assembly is strong-named.</summary>
        /// <param name="file">The assembly file.</param>
        private void RemoveBindingRedirect(InstallFile file)
        {
            if (this.ApplyXmlMerge(file, "RemoveBindingRedirect.config"))
            {
                this.Log.AddInfo(Util.ASSEMBLY_RemovedBindingRedirect + " - " + file.FullName);
            }
        }

        /// <summary>If the <paramref name="file"/> is a strong-named assembly, applies the XML merge.</summary>
        /// <param name="file">The assembly file.</param>
        /// <param name="xmlMergeFile">The XML merge file name.</param>
        /// <returns><c>true</c> if the XML Merge was applied successfully, <c>false</c> if the file was not a strong-named assembly or could not be read.</returns>
        private bool ApplyXmlMerge(InstallFile file, string xmlMergeFile)
        {
            var assemblyName = ReadAssemblyName(Path.Combine(this.PhysicalBasePath, file.FullName));
            var publicKeyToken = ReadPublicKey(assemblyName);
            if (string.IsNullOrEmpty(publicKeyToken))
            {
                return false;
            }

            var name = assemblyName.Name;
            var assemblyVersion = assemblyName.Version;
            var newVersion = assemblyVersion.ToString();

            var xmlMergePath = Path.Combine(Globals.InstallMapPath, "Config", xmlMergeFile);
            var xmlMergeDoc = GetXmlMergeDoc(xmlMergePath, name, publicKeyToken, OldVersion, newVersion);
            var xmlMerge = new XmlMerge(xmlMergeDoc, file.Version.ToString(), this.Package.Name);
            xmlMerge.UpdateConfigs();

            return true;
        }
    }
}
