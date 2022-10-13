namespace PolyDeploy.DeployClient
{
    using System;
    using System.ComponentModel;

    using Spectre.Cli;
    using ValidationResult = Spectre.Cli.ValidationResult;

    public enum ExitCode
    {
        Success = 0,
        InstallerError = 1,
        UnexpectedError = int.MaxValue
    }

    public class DeployInput : CommandSettings
    {
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

        public Uri GetTargetUri() => new Uri(this.TargetUri, UriKind.Absolute);

        public override ValidationResult Validate()
        {
            if (!Uri.TryCreate(this.TargetUri, UriKind.Absolute, out _))
            {
                return ValidationResult.Error("--target-uri must be a valid URI");
            }

            if (this.InstallationStatusTimeout < 0)
            {
                return ValidationResult.Error("--installation-status-timeout must be non-negative");
            }

            return ValidationResult.Success();
        }
    }
}
