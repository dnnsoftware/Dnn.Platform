# PolyDeploy

In order to use this module, you must enable the `UseTaskFriendlySynchronizationContext` setting in the `web.config`. This setting prevents the HttpContext being lost after using the `await` keyword. Without this setting all module installations will fail.

```xml
<configuration>
	...
	<appSettings>
		...
		<add key="aspnet:UseTaskFriendlySynchronizationContext" value="true" />
	</appSettings>
	...
</configuration>
```

## Temporary Quick Start Guide
This guide serves as a quick start guide, it's only temporary until the UI for the module is implemented after which it will be re-written.

1. Add `UseTaskFriendlySynchronizationContext` to the `web.config`.
2. Install the PolyDeploy module.
3. Visit `[SITEURI]/DesktopModules/PolyDeploy/API/APIUsers/CreateUser?name=[APIUSERNAME]` while replacing the tokens denoted by square brackets with suitable values, take a note of the APIKey and EncryptionKey returned.
4. Place `DeployClient.exe` and `DeployClient.exe.config` in the directory where your built modules are located.
5. Open `DeployClient.exe.config` and update the values of settings `TargetUri`, `APIKey` and `EncryptionKey` with the target website URI (e.g. http://mylocaldev.dnndev.me/) and the keys retrieved in step 3.
6. On the host machine visit `[SITEURI]/DesktopModules/PolyDeploy/API/Whitelist/Create?ip=[IPADDRESSOFCLIENT]` while replacing the tokens denoted by square brackets with suitable values. __Please note:__ The ip address must match the ip of the machine where the `DeployClient` will be running.
7. Run `DeployClient.exe` and follow the prompts. Alternatively for CI run command `DeployClient.exe --no-prompt`.
8. Have a coffee.
