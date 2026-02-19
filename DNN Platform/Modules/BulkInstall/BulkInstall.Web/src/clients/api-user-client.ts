import { DnnServicesFramework } from "@dnncommunity/dnn-elements";
import { User } from "../components/tabs/bulk-install-api-users/bulk-install-api-users.model";

export class ApiUserClient
{
    private readonly sf: DnnServicesFramework;
    private readonly requestUrl: string;

    constructor(moduleId: number) {
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = this.sf.getServiceRoot("BulkInstall") + "APIUser/";
    }

  public async getAll(): Promise<User[]>
  {
    const response = await fetch(
      `${this.requestUrl}GetAll`,
      {
        headers: this.sf.getModuleHeaders(),
      });
    const responseBody = await response.json() as { Users: ApiUser[], };
    return responseBody.Users.map(u => ApiUserClient.toUser(u));
  }

  public async create(name: string, bypassIpWhitelist: boolean): Promise<User>
  {
    const response = await fetch(
      `${this.requestUrl}Create?name=${encodeURIComponent(name)}&bypass=${bypassIpWhitelist}`,
      {
        method: 'POST',
        headers: this.sf.getModuleHeaders(),
      });
    const responseBody = await response.json() as { User: ApiUser, };
    return ApiUserClient.toUser(responseBody.User);
  }

  public async delete(id: number): Promise<void>
  {
    await fetch(
      `${this.requestUrl}Delete?id=${id}`,
      {
        method: 'DELETE',
        headers: this.sf.getModuleHeaders(),
      });
  }

  private static toUser(user: ApiUser) : User {
    return {
      id: user.APIUserId,
      name: user.Name,
      apiKey: user.APIKey,
      encryptionKey: user.EncryptionKey,
      bypassIPWhitelist: user.BypassIPWhitelist,
    };
  }
}

interface ApiUser {
  APIUserId: number;
  Name: string;
  APIKey: string;
  EncryptionKey: string;
  BypassIPWhitelist: boolean;
}
