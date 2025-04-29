// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers;

using System;
using System.Collections;
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.UI.Skins;

/// <summary>The SkinInstaller installs Skin Components to a DotNetNuke site.</summary>
public class SkinInstaller : FileInstaller
{
    private readonly ArrayList skinFiles = new ArrayList();

    private SkinPackageInfo skinPackage;
    private SkinPackageInfo tempSkinPackage;
    private string skinName = Null.NullString;

    /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
    /// <value>A String.</value>
    public override string AllowableFiles
    {
        get
        {
            return "ascx, html, htm, css, xml, js, resx, jpg, jpeg, gif, png";
        }
    }

    /// <summary>Gets the name of the Collection Node ("skinFiles").</summary>
    /// <value>A String.</value>
    protected override string CollectionNodeName
    {
        get
        {
            return "skinFiles";
        }
    }

    /// <summary>Gets the name of the Item Node ("skinFile").</summary>
    /// <value>A String.</value>
    protected override string ItemNodeName
    {
        get
        {
            return "skinFile";
        }
    }

    /// <summary>Gets the PhysicalBasePath for the skin files.</summary>
    /// <value>A String.</value>
    protected override string PhysicalBasePath
    {
        get
        {
            string physicalBasePath = this.RootPath + this.SkinRoot + "\\" + this.skinPackage.SkinName;
            if (!physicalBasePath.EndsWith("\\"))
            {
                physicalBasePath += "\\";
            }

            return physicalBasePath.Replace("/", "\\");
        }
    }

    /// <summary>Gets the root folder for the Skin.</summary>
    /// <value>A String.</value>
    protected string RootPath
    {
        get
        {
            string rootPath = Null.NullString;
            if (this.Package.InstallerInfo.PortalID == Null.NullInteger && this.Package.PortalID == Null.NullInteger)
            {
                rootPath = Globals.HostMapPath;
            }
            else
            {
                rootPath = PortalController.Instance.GetCurrentPortalSettings().HomeSystemDirectoryMapPath;
            }

            return rootPath;
        }
    }

    /// <summary>Gets the collection of Skin Files.</summary>
    /// <value>A List(Of InstallFile).</value>
    protected ArrayList SkinFiles
    {
        get
        {
            return this.skinFiles;
        }
    }

    /// <summary>Gets the name of the SkinName Node ("skinName").</summary>
    /// <value>A String.</value>
    protected virtual string SkinNameNodeName
    {
        get
        {
            return "skinName";
        }
    }

    /// <summary>Gets the RootName of the Skin.</summary>
    /// <value>A String.</value>
    protected virtual string SkinRoot
    {
        get
        {
            return SkinController.RootSkin;
        }
    }

    /// <summary>Gets the Type of the Skin.</summary>
    /// <value>A String.</value>
    protected virtual string SkinType
    {
        get
        {
            return "Skin";
        }
    }

    /// <summary>The Install method installs the skin component.</summary>
    public override void Install()
    {
        bool bAdd = Null.NullBoolean;
        try
        {
            // Attempt to get the Skin Package
            this.tempSkinPackage = SkinController.GetSkinPackage(this.skinPackage.PortalID, this.skinPackage.SkinName, this.SkinType);
            if (this.tempSkinPackage == null)
            {
                bAdd = true;
                this.skinPackage.PackageID = this.Package.PackageID;
            }
            else
            {
                this.skinPackage.SkinPackageID = this.tempSkinPackage.SkinPackageID;
                if (this.tempSkinPackage.PackageID != this.Package.PackageID)
                {
                    this.Completed = false;
                    this.Log.AddFailure(Util.SKIN_Installed);
                    return;
                }
                else
                {
                    this.skinPackage.PackageID = this.tempSkinPackage.PackageID;
                }
            }

            this.skinPackage.SkinType = this.SkinType;
            if (bAdd)
            {
                // Add new skin package
                this.skinPackage.SkinPackageID = SkinController.AddSkinPackage(this.skinPackage);
            }
            else
            {
                // Update skin package
                SkinController.UpdateSkinPackage(this.skinPackage);
            }

            this.Log.AddInfo(string.Format(Util.SKIN_Registered, this.skinPackage.SkinName));

            // install (copy the files) by calling the base class
            base.Install();

            // process the list of skin files
            if (this.SkinFiles.Count > 0)
            {
                this.Log.StartJob(Util.SKIN_BeginProcessing);
                string strMessage = Null.NullString;
                var newSkin = new SkinFileProcessor(this.RootPath, this.SkinRoot, this.skinPackage.SkinName);
                foreach (string skinFile in this.SkinFiles)
                {
                    strMessage += newSkin.ProcessFile(skinFile, SkinParser.Portable);
                    skinFile.Replace(Globals.HostMapPath + "\\", "[G]");
                    switch (Path.GetExtension(skinFile))
                    {
                        case ".htm":
                            SkinController.AddSkin(this.skinPackage.SkinPackageID, skinFile.Replace("htm", "ascx"));
                            break;
                        case ".html":
                            SkinController.AddSkin(this.skinPackage.SkinPackageID, skinFile.Replace("html", "ascx"));
                            break;
                        case ".ascx":
                            SkinController.AddSkin(this.skinPackage.SkinPackageID, skinFile);
                            break;
                    }
                }

                Array arrMessage = strMessage.Split(new[] { "<br />" }, StringSplitOptions.None);
                foreach (string strRow in arrMessage)
                {
                    this.Log.AddInfo(HtmlUtils.StripTags(strRow, true));
                }

                this.Log.EndJob(Util.SKIN_EndProcessing);
            }

            this.Completed = true;
        }
        catch (Exception ex)
        {
            this.Log.AddFailure(ex);
        }
    }

    /// <summary>
    /// The Rollback method undoes the installation of the component in the event
    /// that one of the other components fails.
    /// </summary>
    public override void Rollback()
    {
        // If Temp Skin exists then we need to update the DataStore with this
        if (this.tempSkinPackage == null)
        {
            // No Temp Skin - Delete newly added Skin
            this.DeleteSkinPackage();
        }
        else
        {
            // Temp Skin - Rollback to Temp
            SkinController.UpdateSkinPackage(this.tempSkinPackage);
        }

        // Call base class to prcoess files
        base.Rollback();
    }

    /// <summary>The UnInstall method uninstalls the skin component.</summary>
    public override void UnInstall()
    {
        this.DeleteSkinPackage();

        // Call base class to prcoess files
        base.UnInstall();
    }

    /// <summary>The ProcessFile method determines what to do with parsed "file" node.</summary>
    /// <param name="file">The file represented by the node.</param>
    /// <param name="nav">The XPathNavigator representing the node.</param>
    protected override void ProcessFile(InstallFile file, XPathNavigator nav)
    {
        switch (file.Extension)
        {
            case "htm":
            case "html":
            case "ascx":
            case "css":
                if (file.Path.IndexOf(Globals.glbAboutPage, StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    this.SkinFiles.Add(this.PhysicalBasePath + file.FullName);
                }

                break;
        }

        // Call base method to set up for file processing
        base.ProcessFile(file, nav);
    }

    /// <summary>The ReadCustomManifest method reads the custom manifest items.</summary>
    /// <param name="nav">The XPathNavigator representing the node.</param>
    protected override void ReadCustomManifest(XPathNavigator nav)
    {
        this.skinPackage = new SkinPackageInfo();
        this.skinPackage.PortalID = this.Package.PortalID;

        // Get the Skin name
        this.skinPackage.SkinName = Util.ReadElement(nav, this.SkinNameNodeName);

        // Call base class
        base.ReadCustomManifest(nav);
    }

    /// <summary>The UnInstallFile method unInstalls a single file.</summary>
    /// <param name="unInstallFile">The InstallFile to unInstall.</param>
    protected override void UnInstallFile(InstallFile unInstallFile)
    {
        // Uninstall file
        base.UnInstallFile(unInstallFile);

        if (unInstallFile.Extension == "htm" || unInstallFile.Extension == "html")
        {
            // Try to remove "processed file"
            string fileName = unInstallFile.FullName;
            fileName = fileName.Replace(Path.GetExtension(fileName), ".ascx");
            Util.DeleteFile(fileName, this.PhysicalBasePath, this.Log);
        }
    }

    /// <summary>
    /// The DeleteSkinPackage method deletes the Skin Package
    /// from the data Store.
    /// </summary>
    private void DeleteSkinPackage()
    {
        try
        {
            // Attempt to get the Authentication Service
            SkinPackageInfo skinPackage = SkinController.GetSkinByPackageID(this.Package.PackageID);
            if (skinPackage != null)
            {
                SkinController.DeleteSkinPackage(skinPackage);
            }

            this.Log.AddInfo(string.Format(Util.SKIN_UnRegistered, skinPackage.SkinName));
        }
        catch (Exception ex)
        {
            this.Log.AddFailure(ex);
        }
    }
}
