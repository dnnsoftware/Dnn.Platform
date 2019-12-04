# Building Dnn

You do not always need to build the entire solution, you can download and install from the releases and only build the part you are working on, please read [How to Contribue](CONTRIBUTING.md) first.

If you do need to build the entire solution and the distribution packages, you need to be aware that the entire distribution is split in multiple github repositories.
* This repository - contains all the core APIs and the Admin Experience (Persona Bar)
* [CKEditorProvider](https://github.com/DNN-Connect/CKEditorProvider) - The default HTML Editor Provider
* [CDF](https://github.com/dnnsoftware/ClientDependency) - The Dnn Client Dependency Framework

## Understanding the branches
Our default branch is called **develop**, this is the branch most pull requests should target in order to be merged into the very next release (bug fixes, minor improvements that are not breaking changes). If you know your change will be a breaking change or more risky, then you should submit it targeting the **future/xx** branch (where xx is the next major release). **release/x.x.x** branches are temporary, they get created at code-freeze to built an alpha release for the testing team, when initial testing is done, we publish one or more release candiate versions (RC1, RC2) as needed until we find the version stable for release, at which point we release that new version and close the release/x.x.x branch. The only pull requests that will be accepted for release/x.x.x branches are for regression issues (the problem was introduced in this very version) or showstopper issues (can't use Dnn with this bug in).

To prevent issues with long paths in some build scripts, fork this repository in a short named folder on the root of any drive such as `c:\dnnsrc\` if you fork to a long path such as `c:\users\username\documents\dnn\source\` you may encounter long path issues.

In order to build the whole solution and produce the install and upgrade packages, you simply need to open PowerShell and run the following command:
```
.\build.ps1
```

The version you are building is the current version on the branch you are. However there are 2 external repositories that get bundled into Dnn build:
[Dnn.Connect CKEditor provider](https://github.com/DNN-Connect/CKEditorProvider) is the default HTML editor provider and its default branch is development.	
[Dnn.ClientDependency](https://github.com/dnnsoftware/ClientDependency), the default branch is dnn
Under normal situations they are the branches used for the next release, however if you have a need to specify a different branch to pull during the build you can specify them as such:
```
.\build.ps1 -ScriptArgs '--CkBranch="branch-name"','--CdfBranch="branch-name"'
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
