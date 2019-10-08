// Packaging of 3rd party stuff that is not in our repository

Task("ExternalExtensions")
.IsDependentOn("CleanTemp")
.IsDependentOn("CKEP")
    .Does(() =>
	{
	});

Task("CKEP")
    .Does(() =>
	{
		var ckepFolder = tempFolder + "CKEP/";
		var ckepPackageFolder = "./Website/Install/Provider/";
		var buildDir = Directory(ckepFolder);
		var buildDirFullPath = System.IO.Path.GetFullPath(ckepFolder) + "\\";
		CreateDirectory(buildDir);
		CreateDirectory(ckepPackageFolder);
        Information("CK:'{0}'", targetBranchCk);
		Information("Downloading External Extensions to {0}", buildDirFullPath);

		//ck
		DownloadFile("https://github.com/DNN-Connect/CKEditorProvider/archive/" + targetBranchCk + ".zip", buildDirFullPath + "ckeditor.zip");
		Information("Decompressing: {0}", "CK Editor");
		Unzip(buildDirFullPath + "ckeditor.zip", buildDirFullPath + "Providers/");

		//look for solutions and start building them
		var externalSolutions = GetFiles(ckepFolder + "**/*.sln");
		Information("Found {0} solutions.", externalSolutions.Count);
		foreach (var solution in externalSolutions){
			var solutionPath = solution.ToString();
			Information("Processing Solution File: {0}", solutionPath);
			Information("Starting NuGetRestore: {0}", solutionPath);
			NuGetRestore(solutionPath);
			Information("Starting to Build: {0}", solutionPath);
			MSBuild(solutionPath, settings => settings.SetConfiguration(configuration));
		}

		//grab all install zips and copy to staging directory
		var fileCounter = 0;
		fileCounter = GetFiles(ckepFolder + "**/*_Install.zip").Count;
		Information("Copying {1} Artifacts from {0}", "CK Editor Provider", fileCounter);
		CopyFiles(ckepFolder + "**/*_Install.zip", ckepPackageFolder);
	});
