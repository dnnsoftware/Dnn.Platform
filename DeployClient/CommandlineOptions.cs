using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeployClient
{
    internal class CommandLineOptions
    {
        [Option("silent",
            DefaultValue = false,
            HelpText = "Whether to not print any output")]
        public bool IsSilent { get; set; }

        [Option("no-prompt",
            DefaultValue = false,
            HelpText = "Whether to prompt the user for confirmation")]
        public bool NoPrompt { get; set; }

        [Option("target-uri", 
            Required = false, 
            HelpText = "The URL to deploy the packages to.",
            DefaultValue = null)]
        public string TargetUri { get; set; }

        [Option("api-key",
            Required = false,
            HelpText = "The API Key to use",
            DefaultValue = null)]
        public string APIKey { get; set; }

        [Option("encryption-key",
            Required = false,
            HelpText = "The Encryption Key to use",
            DefaultValue = null)]
        public string EncryptionKey { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
