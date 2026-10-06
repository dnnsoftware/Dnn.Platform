import { User } from '../components/tabs/dnn-bi-api-users/dnn-bi-api-users.model';
import { PersonaBarServicesFramework } from './persona-bar-services-framework';

export class ApiUserClient {
  private readonly sf: PersonaBarServicesFramework;
  private readonly requestUrl: string;

  constructor() {
    this.sf = new PersonaBarServicesFramework();
    this.requestUrl = this.sf.getServiceRoot() + 'APIUser/';
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
