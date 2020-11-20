// This is the task CI will use to build release packages

using Cake.Frosting;

[Dependency(typeof(CleanArtifacts))]
[Dependency(typeof(CleanArtifacts))]
[Dependency(typeof(UpdateDnnManifests))]
[Dependency(typeof(GenerateChecksum))]
[Dependency(typeof(SetPackageVersions))]
[Dependency(typeof(CreateInstall))]
[Dependency(typeof(CreateUpgrade))]
[Dependency(typeof(CreateDeploy))]
[Dependency(typeof(CreateSymbols))]
[Dependency(typeof(CreateNugetPackages))]
public sealed class BuildAll : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        RevertSqlDataProvider(context);
    }

    private void RevertSqlDataProvider(Context context)
    {
        var fileName = context.GetTwoDigitsVersionNumber() + ".SqlDataProvider";
        var filePath = "./Dnn Platform/Website/Providers/DataProviders/SqlDataProvider/" + fileName;
        if (!context.sqlDataProviderExists && System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }
}
