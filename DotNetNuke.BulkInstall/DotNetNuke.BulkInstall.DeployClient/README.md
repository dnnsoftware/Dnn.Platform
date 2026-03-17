# DNN Platform
## Bulk Install
### Deploy Client

Bulk Install is an extension for [DNN Platform](https://dnncommunity.org/) which supports installing many extension
packages at once. For Continuous Deployment scenarios, this Deploy Client can be used to remotely install packages via
the extension's API.

#### Examples

##### Installation

Install the Deploy Client as a global NuGet tool during deployment:
```pwsh
dotnet tool install --global DotNetNuke.BulkInstall;
```

Alternatively, install the Deploy Client as a local tool:
```pwsh
dotnet new tool-manifest;
dotnet tool install DotNetNuke.BulkInstall;
```

During deployment, restore local tools using the following command:
```pwsh
dotnet tool restore;
```

##### Usage

In order to do a deployment, the Deploy Client needs the following information:
 - `--target-uri`: The URL of the site to which the packages will be deployed
 - `--api-key`: The API key associated with the Bulk Install API user
 - `--encryption-key`: The encryption key associated with the Bulk Install API user
 - `--packages-directory`: A folder with packages. If not supplied, uses the current working directory.

A typical basic example could look something like this:
```pwsh
dotnet tool run dnn-bulkinstall --target-uri https://dnn.example.com --api-key abc123 --encryption-key zyx789;
```

An example specifying all the options could look like this:
```pwsh
dotnet tool run dnn-bulkinstall --target-uri https://dnn.example.com --api-key abc123 --encryption-key zyx789 --packages-directory ./Install/ --recurse --log-level Trace --installation-status-timeout 120;
```

Finally, the Deploy Client also supports the older PolyDeploy extension, using the `--legacy-api` argument.
