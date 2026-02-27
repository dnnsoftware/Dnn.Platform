import { DnnServicesFramework } from '@dnncommunity/dnn-elements';
import { Ip } from '../components/tabs/dnn-bi-ip-safelist/dnn-bi-ip-safelist.model';

export class IpSafelistClient {
  private readonly sf: DnnServicesFramework;
  private readonly requestUrl: string;

  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = this.sf.getServiceRoot('BulkInstall') + 'IPSpec/';
  }

  public async getAll(): Promise<Ip[]> {
    const response = await fetch(`${this.requestUrl}GetAll`, {
      headers: this.sf.getModuleHeaders(),
    });
    const responseBody = (await response.json()) as { Safelist: IpSpec[] };
    return responseBody.Safelist.map(ip => IpSafelistClient.toIp(ip));
  }

  public async create(name: string, ipAddress: string): Promise<Ip> {
    const response = await fetch(`${this.requestUrl}Create?name=${encodeURIComponent(name)}&ip=${ipAddress}`, {
      method: 'POST',
      headers: this.sf.getModuleHeaders(),
    });
    const responseBody = (await response.json()) as { Ip: IpSpec };
    return IpSafelistClient.toIp(responseBody.Ip);
  }

  public async delete(id: number): Promise<void> {
    await fetch(`${this.requestUrl}Delete?id=${id}`, {
      method: 'DELETE',
      headers: this.sf.getModuleHeaders(),
    });
  }

  public async getIpSafelistConfiguration(): Promise<boolean> {
    const response = await fetch(`${this.requestUrl}GetIpSafelistConfiguration`, {
      method: 'GET',
      headers: this.sf.getModuleHeaders(),
    });
    const responseBody = (await response.json()) as { Enabled: boolean };
    return responseBody.Enabled;
  }

  public async saveIpSafelistConfiguration(enabled: boolean): Promise<void> {
    await fetch(`${this.requestUrl}SaveIpSafelistConfiguration?enabled=${enabled}`, {
      method: 'POST',
      headers: this.sf.getModuleHeaders(),
    });
  }

  private static toIp(ipSpec: IpSpec): Ip {
    return {
      id: ipSpec.IPSpecId,
      name: ipSpec.Name,
      ipAddress: ipSpec.Address,
    };
  }
}

interface IpSpec {
  IPSpecId: number;
  Name: string;
  Address: string;
}
