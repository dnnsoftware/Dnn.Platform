// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers;

using System;
using System.IO;
using System.Xml;

using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.UI.Skins;

/// <summary>The SkinPackageWriter class.</summary>
public class SkinPackageWriter : PackageWriterBase
{
    private readonly SkinPackageInfo skinPackage;
    private readonly string subFolder;

    /// <summary>Initializes a new instance of the <see cref="SkinPackageWriter"/> class.</summary>
    /// <param name="package"></param>
    public SkinPackageWriter(PackageInfo package)
        : base(package)
    {
        this.skinPackage = SkinController.GetSkinByPackageID(package.PackageID);
        this.SetBasePath();
    }

    /// <summary>Initializes a new instance of the <see cref="SkinPackageWriter"/> class.</summary>
    /// <param name="skinPackage"></param>
    /// <param name="package"></param>
    public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package)
        : base(package)
    {
        this.skinPackage = skinPackage;
        this.SetBasePath();
    }

    /// <summary>Initializes a new instance of the <see cref="SkinPackageWriter"/> class.</summary>
    /// <param name="skinPackage"></param>
    /// <param name="package"></param>
    /// <param name="basePath"></param>
    public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package, string basePath)
        : base(package)
    {
        this.skinPackage = skinPackage;
        this.BasePath = basePath;
    }

    /// <summary>Initializes a new instance of the <see cref="SkinPackageWriter"/> class.</summary>
    /// <param name="skinPackage"></param>
    /// <param name="package"></param>
    /// <param name="basePath"></param>
    /// <param name="subFolder"></param>
    public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package, string basePath, string subFolder)
        : base(package)
    {
        this.skinPackage = skinPackage;
        this.subFolder = subFolder;
        this.BasePath = Path.Combine(basePath, subFolder);
    }

    /// <inheritdoc/>
    public override bool IncludeAssemblies
    {
        get
        {
            return false;
        }
    }

    protected SkinPackageInfo SkinPackage
    {
        get
        {
            return this.skinPackage;
        }
    }

    public void SetBasePath()
    {
        if (this.skinPackage.SkinType == "Skin")
        {
            this.BasePath = Path.Combine("Portals\\_default\\Skins", this.SkinPackage.SkinName);
        }
        else
        {
            this.BasePath = Path.Combine("Portals\\_default\\Containers", this.SkinPackage.SkinName);
        }
    }

    /// <inheritdoc/>
    protected override void GetFiles(bool includeSource, bool includeAppCode)
    {
        // Call base class method with includeAppCode = false
        base.GetFiles(includeSource, false);
    }

    /// <inheritdoc/>
    protected override void ParseFiles(DirectoryInfo folder, string rootPath)
    {
        // Add the Files in the Folder
        FileInfo[] files = folder.GetFiles();
        foreach (FileInfo file in files)
        {
            string filePath = folder.FullName.Replace(rootPath, string.Empty);
            if (filePath.StartsWith("\\"))
            {
                filePath = filePath.Substring(1);
            }

            if (!file.Extension.Equals(".dnn", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(this.subFolder))
                {
                    this.AddFile(Path.Combine(filePath, file.Name));
                }
                else
                {
                    filePath = Path.Combine(filePath, file.Name);
                    this.AddFile(filePath, Path.Combine(this.subFolder, filePath));
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override void WriteFilesToManifest(XmlWriter writer)
    {
        var skinFileWriter = new SkinComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
        if (this.SkinPackage.SkinType == "Skin")
        {
            skinFileWriter = new SkinComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
        }
        else
        {
            skinFileWriter = new ContainerComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
        }

        skinFileWriter.WriteManifest(writer);
    }
}
