import { DnnServicesFramework } from '@dnncommunity/dnn-elements';
import { User } from '../components/tabs/bulk-install-api-users/bulk-install-api-users.model';

export class ApiUserClient {
  private readonly sf: DnnServicesFramework;
  private readonly requestUrl: string;

  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = this.sf.getServiceRoot('BulkInstall') + 'APIUser/';
  }

  public async getAll(): Promise<{ users: User[]; enabled: boolean }> {
    const response = await fetch(`${this.requestUrl}GetAll`, {
      headers: this.sf.getModuleHeaders(),
    });
    const responseBody = (await response.json()) as { Users: ApiUser[]; Enabled: boolean };
    return {
      users: responseBody.Users.map(u => ApiUserClient.toUser(u)),
      enabled: responseBody.Enabled,
    };
  }

  public async create(name: string, bypassIpWhitelist: boolean, expiresOn: Date): Promise<User> {
    const response = await fetch(`${this.requestUrl}Create?name=${encodeURIComponent(name)}&bypass=${bypassIpWhitelist}&expiresOn=${expiresOn.toISOString()}`, {
      method: 'POST',
      headers: this.sf.getModuleHeaders(),
    });
    const responseBody = (await response.json()) as { User: ApiUser };
    return ApiUserClient.toUser(responseBody.User);
  }

  public async delete(id: number): Promise<void> {
    await fetch(`${this.requestUrl}Delete?id=${id}`, {
      method: 'DELETE',
      headers: this.sf.getModuleHeaders(),
    });
  }

  private static toUser(user: ApiUser): User {
    return {
      id: user.APIUserId,
      name: user.Name,
      apiKey: user.ApiKey,
      encryptionKey: user.EncryptionKey,
      bypassIPWhitelist: user.BypassIPWhitelist,
    };
  }
}

interface ApiUser {
  APIUserId: number;
  Name: string;
  ApiKey: string;
  EncryptionKey: string;
  BypassIPWhitelist: boolean;
}
