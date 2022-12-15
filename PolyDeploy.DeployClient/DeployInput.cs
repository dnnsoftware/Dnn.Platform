using System.IO;

namespace PolyDeploy.DeployClient
{
    using System;
    using System.ComponentModel;

    using Spectre.Console.Cli;

    public enum ExitCode
    {
        Success = 0,
        InstallerError = 1,
        PackageError = 2,
        UnexpectedError = int.MaxValue
    }

    public class DeployInput : CommandSettings
    {
        private string packagesDirectoryPath = string.Empty;

        [CommandOption("-u|--target-uri")]
        [Description("The URL of the site to which the packages will be deployed.")]
        public string TargetUri { get; set; } = string.Empty;

        [CommandOption("-a|--api-key")]
        [Description("The key used to authenticate with the web server.")]
        public string ApiKey { get; set; } = string.Empty;

        [CommandOption("-e|--encryption-key")]
        [Description("The key used to encrypt the packages before uploading.")]
        public string EncryptionKey { get; set; } = string.Empty;

        [CommandOption("-t|--installation-status-timeout")]
        [Description("The number of seconds to ignore 404 errors when checking installation status.")]
        [DefaultValue(60)]
        public int InstallationStatusTimeout { get; set; }

        [CommandOption("-d|--packages-directory")]
        [Description("Defines the directory that contains the to install packages.")]
        public string PackagesDirectoryPath
        {
            get => ValidOrCurrentDirectory(this.packagesDirectoryPath);
            set => this.packagesDirectoryPath = ValidOrCurrentDirectory(value);
        }

        public Uri GetTargetUri() => new Uri(this.TargetUri, UriKind.Absolute);

        private static string ValidOrCurrentDirectory(string path)
        {
            return string.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : path;
        }
    }
}
