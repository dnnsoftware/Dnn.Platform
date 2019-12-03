Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
	{
		NUnit3("./src/**/bin/" + configuration + "/*.Test*.dll", new NUnit3Settings {
			NoResults = false
			});
	});
