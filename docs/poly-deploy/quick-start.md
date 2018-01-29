# Quick Start Guide
The sections below should serve as quick start guides for using PolyDeploy to install modules both locally and remotely.

## Local Module Installations
The simplest way to get started using PolyDeploy is to use the user interface to install modules locally. To get started simply follow the steps below:

1. Add the `UseTaskFriendlySynchronizationContext` configuration key to the `web.config` as described in [Installation][install-link].
2. Install the PolyDeploy module using the DNN Extensions module.
3. Place the PolyDeploy module on a page.
4. You can now use the drag and drop user interface to install modules.

___Note:___ _Due to the IP Whitelisting functionality, you will only be able to access the user interface from the the host machine._

## Remote Module Installations
For more hands off module installations PolyDeploy can be used to install modules remotely. This also allows for PolyDeploy to be used for continuous integration. To get started simply follow the steps below:

1. Add the `UseTaskFriendlySynchronizationContext` configuration key to the `web.config` as described in [Installation][install-link].
2. Install the PolyDeploy module using the DNN Extensions module.
3. Place the PolyDeploy module on a page.
4. Enter edit mode.
5. Click the edit icon on the PolyDeploy module and select 'Manage' from the edit menu.
6. In the management menu, select 'API Users'.
7. Create a new API User for your remote deployments. Take note of the provided API Key and Encryption Key as they will not be shown again.
8. In the management menu, select 'IP Whitelist'.
9. Create a new Whitelist Entry for the machine that you will be using to perform remote module installations.
10. You can now use your API Key and Encryption Key to perform remote module installations from the machine you added to the IP Whitelist.

___Note:___ _Due to the IP Whitelisting functionality, you will only be able to access the management user interface from machines that are already whitelisted._

[install-link]: installation.html
