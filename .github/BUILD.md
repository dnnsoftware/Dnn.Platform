# Building Dnn

You do not always need to build the entire solution, you can download and install from the releases and only build the part you are working on, please read [How to Contribue](CONTRIBUTING.md) first.

If you do need to build the entire solution and the distribution packages, you need to be aware that the entire distribution is split in multiple github repositories.
* This repository - contains all the core APIs and the Admin Experience (Persona Bar)
* [CKEditorProvider](https://github.com/DNN-Connect/CKEditorProvider) - The default HTML Editor Provider
* [CDF](https://github.com/dnnsoftware/ClientDependency) - The Dnn Client Dependency Framework

Also, we currently maintain two branches, the development branch is the next major release and we also maintain a release/x.x.x branch that allows doing bug fixes on the current major version.

To prevent issues with long paths in some build scripts, fork this repository in a short named folder on the root of any drive such as `c:\dnnsrc\` if you fork to a long path such as `c:\users\username\documents\dnn\source\` you may encounter long path issues.

In order to build the whole solution and produce the install and upgrade packages, you simply need to open PowerShell and run the following command:
```
.\build.ps1
```
Please note that the version number will be automatically set depending on the git branch you are on.

In the rare situation where you would need to build from other branches of the external repositories, you can specify those branches as such:
```
.\build.ps1 -ScriptArgs '--CkBranch="development"','--CdfBranch="dnn"'
```

If you encounter any build issues, please re-run the build with more verbosity as such:
```
.\build.ps1 -Verbosity diagnostic
```
This will log much more information about the problem and allow you to open an issue with those more detailed logs.

Also, the build scripts should leave you with 0 tracked modified files in git.
If a build fails midway and you have tracked artifacts, you can simply run:
`git reset --hard` and/or `git clean -dxf` in order to come back to a clean state.

If you encounter PowerShell security issues, please read [Cake - PowerShell Security](https://cakebuild.net/docs/tutorials/powershell-security)