# Building Dnn

Important: **Dnn.Platform does not support "Hit F5 and see your website come up".** I.e. you can't run DNN by hitting F5 on the source code in Visual Studio.

There are three supported build scenarios:

1. **Build to create a platform distribution package**. You'd only use this to test a complete package (how it installs, works, etc). This build process is used by our Continuous Integration system and creates the release packages everyone uses to install the platform.
2. **Build to create a local DNN development website**. You'd typically not do this all the time, but only when you wish to set up a new development site or revert your development website to the current DNN repository state.
3. **Debug build**. You'd use this when changing code and testing your changes on your (previously created) development site. Note you can also "rebuild" just a part of the platform and not the entire solution for this which will speed things up for you.

When contributing to DNN, you'd typically go through steps 2 and 3 at least and maybe 1 if you wish to run more encompassing tests. But before you delve into code, please familiarize yourself with [How to Contribute](/CONTRIBUTING.md) first.

## Build/Develop Prerequisites

- Visual Studio 2022 (or .NET SDK 6.x) or later
- Node.js 18.x or later
  - This project uses [Yarn](https://yarnpkg.com/), enable its local usage by running `corepack enable`

## External sources

There is one project not included in this repository that are distributed with DNN:

- [CDF](https://github.com/dnnsoftware/ClientDependency) - The Dnn Client Dependency Framework

If you wish to make changes to that project, please keep this in mind.

## Used Build Technologies

DNN uses the following technologies to create a working build:

1. MSBuild. This is Microsoft Visual Studio's built in mechanism to compile C#. It can also run auxiliary tasks (like packaging the included modules). These tasks are specified in `.build` and `.targets` files and can leverage .net assemblies to do its magic. Almost all central MSBuild code is in the `Build/BuildScripts` folder. Main folder location settings can be found in the `DNN_Platform.build` file in the root of the repository which can be overridden using a `DNN_Platform.local.build` file at the same location.
2. Webpack. The "Admin Experience" (which is the project that contains the UI for managing DNN) contains a number of client-side Javascript projects (mostly React projects). These are built using Webpack. Webpack is triggered in the main build process in the `Build/BuildScripts/AEModule.build` script. But it can be run on individual projects if you need to.
3. [Cake Build](https://cakebuild.net/). This uses C# code to run build tasks. We use Cake for orchestrating the entire build process (e.g. packaging of the platform) and for auxiliary tasks like creating a dev site. All Cake scripts are found in the `Build/Cake` folder. After Cake first runs it bootstraps itself and creates the `tools` folder where the various assemblies can be found. Note the scripts use the [DNN Cake Utils](https://github.com/DNNCommunity/Dnn.CakeUtils) assembly to do the heavy lifting.

## Build to create packages

This process uses Cake. Open Powershell at the root of the repository folder and enter:

```
.\build.ps1
```

This will trigger the build and packaging logic. The packages are created in the `artifacts` folder.

Note that (unless a build version has been specified, see below) this process will retrieve the latest version from Github and use that to version dlls and manifests. This creates a bunch of changed `.dnn` files and you'll need to make sure you don't include those in any Pull Requests when contributing.

## Build to create/update your development site

This process also uses Cake and follows the same logic as above, with the sole difference that the output is not a distribution zip file but rather this process pumps contents out to a directory you specify. Also you need to tell this process about your SQL server so that it can reset the database. When complete you should get the same experience as if you've built the platform and unpacked it on a server.

### Development Site Prerequisites

You'll need to be running IIS and SQL server (Express) locally for this to work. Create a folder on your hard disk and set it up in IIS as a web application. Then create or edit the local settings file.

### Local Settings file

The build process uses a local settings file which is excluded from source control so you won't accidentally upload this to Github. First open up Powershell at the root of this repository and run the following:

```
.\Build.ps1 --target=CreateSettings
```

This will create a file called `settings.local.json` at the root with the following content:

```json
{
  "WebsitePath": "",
  "WebsiteUrl": "",
  "SaConnectionString": "server=(local);Trusted_Connection=True;",
  "DnnConnectionString": "",
  "DbOwner": "dbo",
  "ObjectQualifier": "",
  "DnnDatabaseName": "Dnn_Platform",
  "DnnSqlUsername": "",
  "DatabasePath": "",
  "Version": "auto"
}
```

The settings are as follows:

| Name                | Description                                                                                                                                                  |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| WebsitePath         | Full physical path to the folder of your (dev) website                                                                                                       |
| WebsiteUrl          | Url for that website (unused for now)                                                                                                                        |
| SaConnectionString  | SQL connection string with admin privileges. This allows the scripts to drop and recreate your database.                                                     |
| DnnConnectionString | Connection string used in the web.config of your dev site to connect to the SQL database                                                                     |
| DbOwner             | If you wish other than the default "dbo", please specify here.                                                                                               |
| ObjectQualifier     | The DNN database Object Qualifier. This is optional.                                                                                                         |
| DnnDatabaseName     | Name to use for your DNN database. This is used in the drop and create scripts.                                                                              |
| DnnSqlUsername      | User name for the account that has ownership of the database. This setting is used in the create scripts to ensure the account has the proper access rights. |
| DatabasePath        | Physical path to where you wish to create the database. Note this is just the folder, not the filename of the database.                                      |
| Version             | You can force the build process to build for a specific version. E.g. 9.4.4.5. That will be used to build the correct versioned dlls and modules.            |

Once you've set up the above, run the following in Powershell:

```
.\Build.ps1 --target=ResetDevSite
```

This will attempt to delete all content in `WebsitePath` and will build DNN to that location.

## Build Debug

Note: **you need to have gone through the steps for setting up a dev site (see above) for this to work.**

To build the .net projects to the right location, you'll need to create your override of the core build variables in the `DNN_Platform.local.build` file:

```xml
<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <WebsitePath Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">C:\Path\to\my\DNN\DevSite</WebsitePath>
  </PropertyGroup>
</Project>
```

Once you've created this file every time you click "rebuild" in Visual Studio on a project (or the solution) you'll see the content change in your dev site.
**Note**: You may have to restart Visual Studio for this new build file to take effect.

For the Webpack projects it is set up to read from the `settings.local.json` file and use the `WebsitePath` to copy generated js files to their right place.

## Build React Projects

The solution includes a number of React projects. Notably for the PersonaBar (`Dnn.AdminExperience/ClientSide/*`). To build these to your development site you
need to use Yarn and scope to the project you're working on. Go to the `package.json` file for the project and find the identifier (name) and use that. So if you're
working on the Site Settings PersonaBar project, you'll find that file here: `Dnn.AdminExperience/ClientSide/SiteSettings.Web/package.json`. Open it up
and you'll see something like this:

```json
{
  "name": "site_settings",
  "version": "9.8.1",
  "private": true,
  ...
```

The "key" for this project is "site_settings". Use the following command at the root of the project to build this:

```
yarn watch --scope site_settings
```

and the React project will be built to the dev site and yarn will watch for changes you make and rebuild continuously.

## Gotchas

1. Always check your version in the settings.local.json file if something is not quite right. It is important it is set correctly!
2. After a build to reset your dev site you may find a large number of changed files in Git. You can safely remove all of those changes while you're working on the solution but you must leave the SolutionInfo.cs file intact as it holds the version nr when you press build in Visual Studio!
3. If you're building a React project make sure to disable the caching in your browser so your changed file is loaded!

## Tips and tricks

### Long paths

To prevent issues with long paths in some build scripts, fork this repository in a short named folder on the root of any drive such as `c:\dnnsrc\` if you fork to a long path such as `c:\users\username\documents\dnn\source\` you may encounter long path issues.

### Tracked files

The build scripts should leave you with 0 tracked modified files in git.
If a build fails midway and you have tracked artifacts, you can simply run:
`git reset --hard` and/or `git clean -dxf` in order to come back to a clean state.

### Troubleshooting

If you encounter PowerShell security issues, please read [Cake - PowerShell Security](http://cakebuildbotdevelop.azurewebsites.net/docs/tutorials/powershell-security).

### Git branching strategy

Our default branch is called **develop**, this is the branch most pull requests should target in order to be merged into the very next release (bug fixes, minor improvements that are not breaking changes). If you know your change will be a breaking change or more risky, then you should submit it targeting the **future/xx** branch (where xx is the next major release). **release/x.x.x** branches are temporary, they get created at code-freeze to built an alpha release for the testing team, when initial testing is done, we publish one or more release candiate versions (RC1, RC2) as needed until we find the version stable for release, at which point we release that new version and close the release/x.x.x branch. The only pull requests that will be accepted for release/x.x.x branches are for regression issues (the problem was introduced in this very version) or showstopper issues (can't use Dnn with this bug in).
