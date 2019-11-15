Task("Run-Unit-Tests")
    .IsDependentOn("CompileSource")
    .Does(() =>
	{
		NUnit3("./src/**/bin/" + configuration + "/*.Test*.dll", new NUnit3Settings {
			NoResults = false
			});
	});
