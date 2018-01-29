# PolyDeploy
Seasoned DNN developers will no doubt have experience of installing many modules at once by placing their module install files in the `[websiteroot]/Install/Module/` directory and calling `[domain]/Install.aspx?Mode=InstallResources`. It was the only efficient way to install many modules together while avoiding multiple AppPool restarts.

Unfortunately, due to a security exploit within `Install.aspx` it is now removed after the installation of DNN is complete. This leaves the installation of large numbers of modules a tedius and time consuming task.

## What is PolyDeploy?
PolyDeploy aims to bring that deployment convenience back to DNN developers so deployments can once again be quick and easy. In addition we aim to add additional convenience to allow more flexible deployment solutions including remote deployments and automated deployments that can be performed by your continuous integration solution to help steamline the development process.

## Features

### Security
PolyDeploy has been architected with security in mind from the very beginning. There are multiple mechanisms in place to prevent the unauthorised use of PolyDeploy to avoid attackers deploying modules.

#### IP Whitelisting
By default, PolyDeploy will only allow access from the host machine (127.0.0.1). Starting out with the strictest possible whitelisting and requiring that the user log in on the host machine and specify additional IP addresses to be trusted. This ensures there's no security vulnerabilty if PolyDeploy is installed and not immediately configured.

#### Host Only
The API exposed for use with PolyDeploy's Angular user interface requires a host user account in order to be accessible. If for some reason the PolyDeploy module is mistakenly placed on a page which is accisible by standard DNN users or visitors who aren't logged in, no vulnerability is created.

#### DNN Services Framework
The API available to allow management of PolyDeploy through its Angular user interface requires that the appropriate anti-forgery token is supplied in the request. The DNN Services Framework provides this security.

#### API Users
PolyDeploy requires that clients accessing its API to perform remote deployments have an API User. Each API User can be uniquely indentified by its API Key. Access to the API can be quickly and easily revoked on an individual basis in the case that an API Key is considered compromised.

#### Encryption
Modules being deployed remotely using a valid API Key must also be encrypted using the unique Encryption Key which is generated when the API User is created. This Encryption Key is never passed over the web for security. If an attacker were to acquire a clients API Key, any modules that were sent to the API would be rejected as it wouldn't be possible to decrypt them unless they also had the Encryption Key.

### Local Deployments
PolyDeploy provides an easy to use drag and drop interface that allows the user to upload multiple modules quickly and inititate a deployment.

### Remote Deployments
PolyDeploy provides the ability to perform secure module deployments over the web through the use of many security features described above.

Additionally an example of a client is provided in this repository which we currently use in our development workflow as part of our continous integration solution.

### Dependency Detection
When you upload modules to PolyDeploy, it looks through the modules dependencies and ensures that they can be met. Dependencies are checked against modules that are already installed as well as modules that are included as part of the current deployment session.

During the dependency checks, modules in the deployment session are placed in to a suitable installation order. Modules will only be installed after the modules they depend on have been installed. For example if I add Module A and Module B to a deployment session and Module B has a dependency on Module A, PolyDeploy will make sure that Module A is installed before Module B during the deployment.

#### Circular Dependencies
PolyDeply is also able to detect circular dependencies in modules and will prevent a deployment from starting if a circular dependency is found.

### Auditability
PolyDeploy logs all activity including:
  - Failed IP Whitelist Checks
  - Failed API User Checks
  - Session Creation
  - Module Upload
  - Module Installation
