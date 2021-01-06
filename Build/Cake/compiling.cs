// This tasks kicks off MS Build (just as in Visual Studio)

using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.NuGet;
using Cake.Frosting;

[Dependency(typeof(CleanWebsite))]
[Dependency(typeof(RestoreNuGetPackages))]
public sealed class Build : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var buildSettings = new MSBuildSettings()
            .SetConfiguration(context.configuration)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .WithTarget("Rebuild")
            .SetMaxCpuCount(4);
        context.MSBuild(context.dnnSolutionPath, settings => settings.WithTarget("Clean"));
        context.MSBuild(context.dnnSolutionPath, buildSettings);
    }
}

public sealed class RestoreNuGetPackages : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.NuGetRestore(context.dnnSolutionPath);

    }
}
