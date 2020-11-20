using System.Collections.Generic;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.NuGet;
using Cake.Common.Tools.NuGet.Pack;
using Cake.Frosting;

[Dependency(typeof(PreparePackaging))]
public sealed class CreateNugetPackages : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        //look for solutions and start building them
        var nuspecFiles = context.GetFiles("./Build/Tools/NuGet/*.nuspec");

        context.Information("Found {0} nuspec files.", nuspecFiles.Count);

        //basic nuget package configuration
        var nuGetPackSettings = new NuGetPackSettings
        {
            Version = context.GetBuildNumber(),
            OutputDirectory = @"./Artifacts/",
            IncludeReferencedProjects = true,
            Properties = new Dictionary<string, string> {{"Configuration", "Release"}}
        };

        //loop through each nuspec file and create the package
        foreach (var spec in nuspecFiles)
        {
            var specPath = spec.ToString();

            context.Information("Starting to pack: {0}", specPath);
            context.NuGetPack(specPath, nuGetPackSettings);
        }
    }
}
