# Building Dnn

You do not always need to build the entire solution, please read [How to Contribue](CONTRIBUTING.md) first.

If you do need to build the entire solution and the distribution packages, you need to be aware that the entire distribution is split in multiple github repositories.
* This repository - contains all the core APIs
* [CKEditorProvider](https://github.com/DNN-Connect/CKEditorProvider) - The default HTML Editor Provider
* [CDF](https://github.com/dnnsoftware/ClientDependency) - The Dnn Client Dependency Framework
* [Admin Experience](https://github.com/dnnsoftware/Dnn.AdminExperience) The default administration interface (Persona Bar)

Also, we currently maintain two branches, the development branch is the next major release and we also maintain a release/x.x.x branch that allows doing bug fixes on the current major version.

If you need to build the next major release you simply need to open PowerShell and run the following command:
```
.\build.ps1 -Target BuildAll
```

But if you need to build from one of the release branches, then you also need to reference each branch you want to pull from those repositories as such (example for the 9.4.0 release, replace the branch names as needed):
```
.\build.ps1 -Target BuildAll -ScriptArgs '--CkBranch="development"','--CdfBranch="dnn"','--CpBranch="release/3.0.x"'
```