// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Build
{
    using Cake.Common;
    using Cake.Common.Diagnostics;
    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Core.IO;
    using Cake.Frosting;

    /// <inheritdoc/>
    public sealed class Lifetime : FrostingLifetime<Context>
    {
        private static readonly string[] CorepackToolNames = ["corepack", "corepack.cmd",];

        /// <inheritdoc/>
        public override void Setup(Context context, ISetupContext setupContext)
        {
            context.IsRunningInCI = context.HasEnvironmentVariable("TF_BUILD");
            context.Information("Is Running in CI : {0}", context.IsRunningInCI);
            if (context.Settings.Version == "auto" && !context.IsRunningInCI)
            {
                // Temporarily commit all changes to prevent checking in scripted changes like versioning.
                Git(context, "add .");
                Git(context, "commit --allow-empty -m 'backup'");
            }

            if (context.Tools.Resolve(CorepackToolNames) is null)
            {
                throw new CakeException("Could not find corepack, Node.js 18 or later must be installed.");
            }
        }

        /// <inheritdoc/>
        public override void Teardown(Context context, ITeardownContext info)
        {
            if (context.Settings.Version == "auto" && !context.IsRunningInCI)
            {
                // Undoes the script changes to all tracked files.
                Git(context, "reset --hard");

                // Undoes the setup commit keeping file states as before this build script ran.
                Git(context, "reset HEAD^");
            }
        }

        private static void Git(ICakeContext context, string arguments)
        {
            context.Information($"git ${arguments}");
            using (var process = context.StartAndReturnProcess("git", new ProcessSettings { Arguments = arguments, }))
            {
                process.WaitForExit();
            }
        }
    }
}
