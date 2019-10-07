Task("CreateNugetPackages")
	.IsDependentOn("PreparePackaging")
	.Does(() =>
	{
		//look for solutions and start building them
		var nuspecFiles = GetFiles("./Build/Tools/NuGet/DotNetNuke.*.nuspec");
	
		Information("Found {0} nuspec files.", nuspecFiles.Count);

		//basic nuget package configuration
		var nuGetPackSettings = new NuGetPackSettings
		{
			Version = GetBuildNumber(),
			OutputDirectory = @"./Artifacts/",
			IncludeReferencedProjects = true,
			Properties = new Dictionary<string, string>
			{
				{ "Configuration", "Release" }
			}
		};
	
		//loop through each nuspec file and create the package
		foreach (var spec in nuspecFiles){
			var specPath = spec.ToString();

			Information("Starting to pack: {0}", specPath);
			NuGetPack(specPath, nuGetPackSettings);
		}
	});
