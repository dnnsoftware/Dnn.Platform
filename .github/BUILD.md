# Building Dnn

You do not always need to build the entire solution, you can download and install from the releases and only build the part you are working on, please read [How to Contribue](CONTRIBUTING.md) first.

If you do need to build the entire solution and the distribution packages, you need to be aware that the entire distribution is split in multiple github repositories.
* This repository - contains all the core APIs
* [CKEditorProvider](https://github.com/DNN-Connect/CKEditorProvider) - The default HTML Editor Provider
* [CDF](https://github.com/dnnsoftware/ClientDependency) - The Dnn Client Dependency Framework
* [Admin Experience](https://github.com/dnnsoftware/Dnn.AdminExperience) The default administration interface (Persona Bar)

Also, we currently maintain two branches, the development branch is the next major release and we also maintain a release/x.x.x branch that allows doing bug fixes on the current major version.

To prevent issues with long paths in some build scripts, fork this repository in a short named folder on the root of any drive such as `c:\dnnsrc\` if you fork to a long path such as `c:\users\username\documents\dnn\source\` you may encounter long path issues.

If you need to build the next major release you simply need to open PowerShell and run the following command:
```
.\build.ps1 -Target BuildAll
```

But if you need to build from one of the release branches, then you also need to reference each branch you want to pull from those repositories as such (example for the 9.4.0 release, replace the branch names as needed):
```
.\build.ps1 -Target BuildAll -ScriptArgs '--CkBranch="development"','--CdfBranch="dnn"','--CpBranch="release/3.0.x"'
```

If you encounter any build issues, please re-run the build with more verbosity as such:
```
.\build.ps1 -Target BuildAll -Verbosity diagnostic
```
This will log much more information about the problem and allow you to open an issue with those more detailed logs.

If you encounter PowerShell security issues, please read [Cake - PowerShell Security](https://cakebuild.net/docs/tutorials/powershell-security)