# DeployClient
The DeployClient provides a basic implementation of a client designed to interact with the WebAPI endpoints exposed by PolyDeploy to facilitate remote deployment of DNN modules. It demonstrates how the API key should be passed in the request and how after initiating an installation remotely the status of the install operation can be polled for updates.

## Setup
Before using the DeployClient you must configure it.

1. Open `DeployClient.exe.config`, found in the same directory as `DeployClient.exe`.
2. Find the `DeployClient.Properties.Settings` node.
3. Set the value of the setting named `TargetUri` as the website URI of the DNN install you wish to target.
4. Set the value of the setting named `APIKey` as the API Key of your API User.
5. Set the value of the setting named `EncryptionKey` as the Encryption Key of your API User.

Here is an example of a filled in `DeployClient.Properites.Settings` node. API and Encryption Keys have been redacted.

```xml
...
<DeployClient.Properties.Settings>
    <setting name="TargetUri" serializeAs="String">
        <value>http://mydevsite.dnndev.me/</value>
    </setting>
    <setting name="APIKey" serializeAs="String">
        <value>XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX</value>
    </setting>
    <setting name="EncryptionKey" serializeAs="String">
        <value>XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX</value>
    </setting>
</DeployClient.Properties.Settings>
...
```

## Usage
Out of the box the DeployClient will identify any .zip files in the directory that it is being run from.

1. Place your module .zip files in the same directory as `DeployClient.exe`.
2. Open `DeployClient.exe` and follow the prompts.

### Options
The DeployClient features a small number of options that can tailor it to different situations, these are detailed below.

- __--silent__ Don't print any output to stdout, don't prompt.
- __--no-prompt__ Don't prompt.

These options can be used on the command line like so `DeployClient.exe --silent`.
