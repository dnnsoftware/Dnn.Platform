# API Reference
This document aims to serve as an API reference, detailing the API methods made available by PolyDeploy for the remote deployment of modules.

## IP Whitelist
Before any of the following API methods will work, you must ensure that the IP address you are accessing the API from has been added to the IP Whitelist. [Read more][ip-whitelist].

## API Authentication
Before getting started with any of the following API methods, ensure you have created a new API User so that you can interact with the API. You'll need the API and Encryption Key pair that was generated when you created the API User. [Read more][api-users].

All requests sent to the api should contain your API Key in the `x-api-key` header of the request.

### CSharp
```csharp
HttpClient client = new HttpClient();

client.DefaultRequestHeaders.Add("x-api-key", "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
```

### curl
```curl
curl -H "x-api-key: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
```

## Methods

### GET CreateSession
`/API/Remote/CreateSession`

Allows the creation of a new deployment session. A valid deployment session is required for all subsequent actions you'll take when performing a remote deployment.

_Returns:_ __string__ Guid representing the session.

### GET GetSession
`/API/Remote/GetSession?sessionGuid=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX`

Retrieves the `Session` object corresponding to the passed `sessionGuid`.

_Returns:_ __Session__ Object representing the session in the database.

### POST AddPackages
`/API/Remote/AddPackages?sessionGuid=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX`

Adds packages to the session corresponding to the passed `sessionGuid`.

___Note:___ _Packages must be encrypted with the Encryption Key associated with the API Key being used to access the API._

### GET Install
`/API/Remote/Install?sessionGuid=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX`

Starts the installation of the modules added to the session corresponding to the passed `sessionGuid`.

[ip-whitelist]: ip-whitelist.html
[api-users]: api-users.html
