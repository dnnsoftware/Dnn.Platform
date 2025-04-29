// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework.Providers;

/// <summary>The ScriptInstaller installs Script Components to a DotNetNuke site.</summary>
public class ScriptInstaller : FileInstaller
{
    private readonly SortedList<Version, InstallFile> installScripts = new SortedList<Version, InstallFile>();
    private readonly SortedList<Version, InstallFile> unInstallScripts = new SortedList<Version, InstallFile>();
    private readonly List<InstallFile> preUpgradeScripts = new List<InstallFile>();
    private readonly List<InstallFile> postUpgradeScripts = new List<InstallFile>();
    private InstallFile installScript;

    /// <inheritdoc />
    public override string AllowableFiles
    {
        get
        {
            return "*dataprovider, sql";
        }
    }

    /// <summary>Gets the base Install Script (if present).</summary>
    protected InstallFile InstallScript
    {
        get
        {
            return this.installScript;
        }
    }

    /// <summary>Gets the collection of versioned Install Scripts.</summary>
    protected SortedList<Version, InstallFile> InstallScripts
    {
        get
        {
            return this.installScripts;
        }
    }

    /// <summary>Gets the collection of UnInstall Scripts.</summary>
    protected SortedList<Version, InstallFile> UnInstallScripts
    {
        get
        {
            return this.unInstallScripts;
        }
    }

    /// <summary>Gets the name of the Collection Node (<c>scripts</c>).</summary>
    protected override string CollectionNodeName
    {
        get
        {
            return "scripts";
        }
    }

    /// <summary>Gets the name of the Item Node (<c>script</c>).</summary>
    protected override string ItemNodeName
    {
        get
        {
            return "script";
        }
    }

    protected ProviderConfiguration ProviderConfiguration
    {
        get
        {
            return ProviderConfiguration.GetProviderConfiguration("data");
        }
    }

    /// <summary>Gets a list of Pre-Upgrade Scripts (if present) - these scripts will always run before any upgrade scripts but not upon initial installation.</summary>
    /// <value>A list of <see cref="InstallFile"/> instances.</value>
    protected IList<InstallFile> PreUpgradeScripts
    {
        get
        {
            return this.preUpgradeScripts;
        }
    }

    [Obsolete("Deprecated in DotNetNuke 9.9.0. This is now the first of the PostUpgrade scripts. Scheduled for removal in v11.0.0.")]
    protected InstallFile UpgradeScript => this.PostUpgradeScripts[0];

    /// <summary>Gets a list of Post-Upgrade Scripts (if present) - these scripts will always run after and versioned upgrade scripts and also after initial install.</summary>
    /// <value>A list of <see cref="InstallFile"/> instances.</value>
    protected IList<InstallFile> PostUpgradeScripts
    {
        get
        {
            return this.postUpgradeScripts;
        }
    }

    /// <inheritdoc />
    public override void Commit()
    {
        base.Commit();
    }

    /// <inheritdoc />
    public override void Install()
    {
        this.Log.AddInfo(Util.SQL_Begin);
        try
        {
            bool bSuccess = true;
            Version installedVersion = this.Package.InstalledVersion;

            // First process InstallScript
            if (installedVersion == new Version(0, 0, 0))
            {
                if (this.InstallScript != null)
                {
                    bSuccess = this.InstallScriptFile(this.InstallScript);
                    installedVersion = this.InstallScript.Version;
                }
            }
            else
            {
                // Pre upgrade script
                foreach (InstallFile file in this.PreUpgradeScripts)
                {
                    bSuccess = this.InstallScriptFile(file);
                    if (!bSuccess)
                    {
                        break;
                    }
                }
            }

            // Then process remain Install/Upgrade Scripts
            if (bSuccess)
            {
                foreach (InstallFile file in this.InstallScripts.Values)
                {
                    if (file.Version > installedVersion)
                    {
                        bSuccess = this.InstallScriptFile(file);
                        if (!bSuccess)
                        {
                            break;
                        }
                    }
                }
            }

            // Next process Post-UpgradeScripts - this script always runs if present
            foreach (InstallFile file in this.PostUpgradeScripts)
            {
                bSuccess = this.InstallScriptFile(file);
                if (file.Version != new Version(0, 0, 0))
                {
                    installedVersion = file.Version;
                }

                if (!bSuccess)
                {
                    break;
                }
            }

            // Then process uninstallScripts - these need to be copied but not executed
            if (bSuccess)
            {
                foreach (InstallFile file in this.UnInstallScripts.Values)
                {
                    bSuccess = this.InstallFile(file);
                    if (!bSuccess)
                    {
                        break;
                    }
                }
            }

            this.Completed = bSuccess;
        }
        catch (Exception ex)
        {
            this.Log.AddFailure(ex);
        }

        this.Log.AddInfo(Util.SQL_End);
    }

    /// <inheritdoc />
    public override void Rollback()
    {
        base.Rollback();
    }

    /// <inheritdoc />
    public override void UnInstall()
    {
        this.Log.AddInfo(Util.SQL_BeginUnInstall);

        // Call the base method
        base.UnInstall();

        this.Log.AddInfo(Util.SQL_EndUnInstall);
    }

    /// <inheritdoc />
    protected override bool IsCorrectType(InstallFileType type)
    {
        return type == InstallFileType.Script;
    }

    /// <inheritdoc />
    protected override void ProcessFile(InstallFile file, XPathNavigator nav)
    {
        string type = nav.GetAttribute("type", string.Empty);
        if (file != null && this.IsCorrectType(file.Type) && this.IsValidScript(file.Name))
        {
            if (file.Name.StartsWith("install.", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is the initial script when installing
                this.installScript = file;
            }
            else if (type.Equals("preupgrade", StringComparison.InvariantCultureIgnoreCase))
            {
                this.preUpgradeScripts.Add(file);
            }
            else if (file.Name.StartsWith("upgrade.", StringComparison.InvariantCultureIgnoreCase)
                     || type.Equals("postupgrade", StringComparison.InvariantCultureIgnoreCase))
            {
                this.postUpgradeScripts.Add(file);
            }
            else if (type.Equals("install", StringComparison.InvariantCultureIgnoreCase))
            {
                // These are the Install/Upgrade scripts
                this.InstallScripts[file.Version] = file;
            }
            else if (file.Name.StartsWith("uninstall.", StringComparison.InvariantCultureIgnoreCase)
                     || type.Equals("uninstall", StringComparison.InvariantCultureIgnoreCase))
            {
                // These are the Uninstall scripts
                this.UnInstallScripts[file.Version] = file;
            }
            else
            {
                // we couldn't determine the file type
                this.Log.AddFailure(string.Format(Util.SQL_Manifest_BadFile, file.Name));
                return;
            }
        }
        else
        {
            // bad file
            this.Log.AddFailure(Util.SQL_Manifest_Error);
            return;
        }

        // Call base method to set up for file processing
        base.ProcessFile(file, nav);
    }

    /// <inheritdoc/>
    protected override void UnInstallFile(InstallFile scriptFile)
    {
        // Process the file if it is an UnInstall Script
        if (this.UnInstallScripts.ContainsValue(scriptFile))
        {
            if (scriptFile.Name.StartsWith("uninstall.", StringComparison.InvariantCultureIgnoreCase))
            {
                // Install Script
                this.Log.AddInfo(Util.SQL_Executing + scriptFile.Name);
                this.ExecuteSql(scriptFile);
            }
        }

        // Call base method to delete file
        base.UnInstallFile(scriptFile);
    }

    private bool ExecuteSql(InstallFile scriptFile)
    {
        bool bSuccess = true;

        this.Log.AddInfo(string.Format(Util.SQL_BeginFile, scriptFile.Name));

        // read script file for installation
        string strScript = FileSystemUtils.ReadFile(this.PhysicalBasePath + scriptFile.FullName);

        // This check needs to be included because the unicode Byte Order mark results in an extra character at the start of the file
        // The extra character - '?' - causes an error with the database.
        if (strScript.StartsWith("?"))
        {
            strScript = strScript.Substring(1);
        }

        string strSQLExceptions = DataProvider.Instance().ExecuteScript(strScript);
        if (!string.IsNullOrEmpty(strSQLExceptions))
        {
            if (this.Package.InstallerInfo.IsLegacyMode)
            {
                this.Log.AddWarning(string.Format(Util.SQL_Exceptions, Environment.NewLine, strSQLExceptions));
            }
            else
            {
                this.Log.AddFailure(string.Format(Util.SQL_Exceptions, Environment.NewLine, strSQLExceptions));
                bSuccess = false;
            }
        }

        this.Log.AddInfo(string.Format(Util.SQL_EndFile, scriptFile.Name));
        return bSuccess;
    }

    private bool IsValidScript(string fileName)
    {
        var fileExtension = Path.GetExtension(fileName.ToLowerInvariant());
        if (fileExtension != null)
        {
            fileExtension = fileExtension.Substring(1);
            return this.ProviderConfiguration.DefaultProvider.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase) || fileExtension.Equals("sql", StringComparison.InvariantCultureIgnoreCase);
        }
        else
        {
            return false;
        }
    }

    private bool InstallScriptFile(InstallFile scriptFile)
    {
        // Call base InstallFile method to copy file
        bool bSuccess = this.InstallFile(scriptFile);
        if (bSuccess)
        {
            this.Log.AddInfo(Util.SQL_Executing + scriptFile.Name);
            bSuccess = this.ExecuteSql(scriptFile);
        }

        return bSuccess;
    }
}
