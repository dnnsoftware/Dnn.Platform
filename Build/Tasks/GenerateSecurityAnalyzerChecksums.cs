// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using Cake.Common.Diagnostics;
    using Cake.Frosting;

    /// <summary>A cake task to generate the <c>Default.aspx</c> checksum for the Security Analyzer.</summary>
    [IsDependentOn(typeof(SetVersion))]
    public sealed class GenerateSecurityAnalyzerChecksums : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            context.Information("Generating default.aspx checksum…");
            const string sourceFile = "./Dnn Platform/Website/Default.aspx";
            const string destFile = "./Dnn.AdminExperience/Dnn.PersonaBar.Extensions/Components/Security/Resources/sums.resources";
            var hash = CalculateSha(sourceFile);
            var content = $"""
                           <checksums>
                             <sum name="Default.aspx" version="{context.Version.MajorMinorPatch}" type="Platform" sum="{hash}" />
                           </checksums>
                           """;
            File.WriteAllText(destFile, content);
        }

        private static string CalculateSha(string filename)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filename);
            var hash = sha.ComputeHash(stream);
            return Convert.ToHexStringLower(hash);
        }
    }
}
