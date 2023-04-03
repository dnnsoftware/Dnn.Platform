# Installation
The installation of the PolyDeploy module is mostly straight forward as you would expect from other DNN modules. There is however one additional step that must be taken to enable the module to function as intended, this is a small change to the `web.config` found in the DNN website root.

## Why?
Some of the WebAPI endpoints provided by PolyDeploy make use of asynchronous methods. In its default configuration ASP.NET will dereference the `HttpContext` object after the `await` keyword is used in a method. Once the asynchronous operation completes and method execution continues the `HttpContext` object is no longer accessible. This is problematic as DNN relies on the `HttpContext` object for many of its internal functions.

## How?
The issue is easily resolved with the addition of a configuration key to the `web.config`. The following code snippet shows the configuration key along with where it should be placed in the `web.config`.

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
