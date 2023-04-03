# API Users
PolyDeploy makes use of API and Encryption Key pairs to authenticate requests to ensure that remote deployment of modules is only performed from appropraite clients.

___Note:___ _A new API User should be created for every client that uses PolyDeploy. This improves security and audability. It also means that in the event that an API and Encryption Key pair is compromised the minimum number of clients need to be updated with a replacement key pair._

## Management
You can manage API Users through the Management Utility. It is possible to create new API Users, view the existing API Users (although you are not able to see their existing API and Encryption Key pairs, this is intentional for security) and delete existing API Users.

___Note:___ _When you first create an API User a new API and Encryption Key pair will be generated and displayed to you. This will only happen once, subsequent visits to the Management Utility will not show the complete API and Encryption Key pairs._
