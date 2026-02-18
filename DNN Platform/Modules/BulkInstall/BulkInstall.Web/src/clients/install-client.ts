import { DnnServicesFramework, } from "@dnncommunity/dnn-elements";

export class InstallClient {
  private readonly sf: DnnServicesFramework;
  private readonly requestUrl: string;

  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = this.sf.getServiceRoot("BulkInstall") + "Session/";
  }

  public async create(): Promise<string> {
    const response = await fetch(
      `${this.requestUrl}Create`,
      {
        method: 'POST',
        headers: this.sf.getModuleHeaders(),
      });
    const responseBody = await response.json() as { Session: Session };
    return responseBody.Session.SessionGuid;
  }

  public async addPackages(sessionGuid: string, files: File[]): Promise<void> {
    for (const file of files) {
      const requestBody = new FormData();
      requestBody.append(file.name, file);

      await fetch(
        `${this.requestUrl}AddPackages?sessionGuid=${sessionGuid}`,
        {
          method: 'POST',
          body: requestBody,
          headers: this.sf.getModuleHeaders(),
        });
    }
  }
}

interface Session {
  SessionGuid: string;
}
