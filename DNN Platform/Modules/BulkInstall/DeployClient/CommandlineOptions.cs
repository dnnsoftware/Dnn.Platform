using CommandLine;
using CommandLine.Text;

namespace DeployClient
{
    internal class CommandLineOptions
    {
        [Option('s',
            "silent",
            DefaultValue = false,
            HelpText = "Whether to not print any output")]
        public bool IsSilent { get; set; }

        [Option('n',
            "no-prompt",
            DefaultValue = false,
            HelpText = "Whether to prompt the user for confirmation")]
        public bool NoPrompt { get; set; }

        [Option('u',
            "target-uri", 
            Required = false, 
            HelpText = "The URL to deploy the packages to.",
            DefaultValue = null)]
        public string TargetUri { get; set; }

        [Option('a',
            "api-key",
            Required = false,
            HelpText = "The API Key to use",
            DefaultValue = null)]
        public string APIKey { get; set; }

        [Option('e',
            "encryption-key",
            Required = false,
            HelpText = "The Encryption Key to use",
            DefaultValue = null)]
        public string EncryptionKey { get; set; }

        [Option('d',
            "packages-directory",
            Required = false,
            HelpText = "Defines the directory that contains the to install packages.",
            DefaultValue = null)]
        public string PackagesDirectoryPath { get; set; }

        [Option('t',
            "installation-status-timeout",
            Required = false,
            HelpText = "The number of seconds to ignore 404 errors when checking installation status",
            DefaultValue = 60)]
        public double InstallationStatusTimeout { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
