namespace PolyDeploy.DeployClient
{
    using System;
    using System.ComponentModel;

    using Spectre.Cli;
    using ValidationResult = Spectre.Cli.ValidationResult;

    public class DeployInput : CommandSettings
    {
        public DeployInput(string targetUri, string apiKey, string encryptionKey)
        {
            this.TargetUri = targetUri;
            this.ApiKey = apiKey;
            this.EncryptionKey = encryptionKey;
        }

        [CommandOption("-u|--target-uri")]
        [Description("The URL of the site to which the packages will be deployed.")]
        public string TargetUri { get; init; }

        [CommandOption("-a|--api-key")]
        [Description("The key used to authenticate with the web server.")]
        public string ApiKey { get; init; }

        [CommandOption("-e|--encryption-key")]
        [Description("The key used to encrypt the packages before uploading.")]
        public string EncryptionKey { get; init; }

        public Uri GetTargetUri() => new Uri(this.TargetUri, UriKind.Absolute);

        public override ValidationResult Validate()
        {
            return Uri.TryCreate(this.TargetUri, UriKind.Absolute, out _)
                ? ValidationResult.Error("--target-uri must be a valid URI")
                : ValidationResult.Success();
        }
    }
}
