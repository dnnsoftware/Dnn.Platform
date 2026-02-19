import { DnnServicesFramework, } from "@dnncommunity/dnn-elements";
import {
  InstallJob,
  PackageDependency,
  PackageJob,
  Session,
} from "../components/tabs/bulk-install-install/bulk-install-install.model";
import { SessionStatusInfo, sessionStatus, } from "../enums/SessionStatus";

export class InstallClient {
  private readonly sf: DnnServicesFramework;
  private readonly requestUrl: string;

  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = this.sf.getServiceRoot("BulkInstall") + "Session/";
  }

  public async create(): Promise<Session> {
    const response = await fetch(
      `${this.requestUrl}Create`,
      {
        method: 'POST',
        headers: this.sf.getModuleHeaders(),
      });
    const responseBody = await response.json() as { Session: SessionResponse };
    return InstallClient.toSession(responseBody.Session);
  }

  public async getSession(sessionGuid: string): Promise<Session> {
    const response = await fetch(
      `${this.requestUrl}Get?sessionGuid=${sessionGuid}`,
      {
        method: 'GET',
        headers: this.sf.getModuleHeaders(),
      });
    const responseBody = await response.json() as { Session: SessionResponse };
    return InstallClient.toSession(responseBody.Session);
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

  public async install(sessionGuid: string): Promise<void> {
    await fetch(
      `${this.requestUrl}Install?sessionGuid=${sessionGuid}`,
      {
        method: 'POST',
        headers: this.sf.getModuleHeaders(),
      });
  }

  public async summary(sessionGuid: string) : Promise<InstallJob[]> {
    const response = await fetch(
      `${this.requestUrl}Summary?sessionGuid=${sessionGuid}`,
      {
        headers: this.sf.getModuleHeaders(),
      }
    );
    const responseBody = await response.json() as { InstallJobs: InstallJobResponse[] };
    return responseBody.InstallJobs.map(ij => InstallClient.toInstallJob(ij));
  }

  private static toInstallJob(job: InstallJobResponse) : InstallJob {
    return {
      attempted: job.Attempted,
      canInstall: job.CanInstall,
      failures: job.Failures,
      success: job.Success,
      name: job.Name,
      packages: job.Packages.map(p => InstallClient.toPackageJob(p)),
    };
  }

  private static toPackageJob(job: PackageJobResponse) : PackageJob {
    return {
      canInstall: job.CanInstall,
      version: job.VersionStr,
      name: job.Name,
      dependencies: job.Dependencies.map(d => InstallClient.toPackageDependency(d)),
    };
  }

  private static toPackageDependency(dependency: PackageDependencyResponse) : PackageDependency {
    return {
      dependencyVersion: dependency.DependencyVersion,
      isPackageDependency: dependency.IsPackageDependency,
      packageName: dependency.PackageName,
    };
  }

  private static toSession(session: SessionResponse) : Session {
    return {
      lastUsed: new Date(session.LastUsed),
      response: session.Response.map(job => InstallClient.toInstallJob(job)),
      sessionGuid: session.SessionGuid,
      status: InstallClient.toSessionStatus(session.Status),
    };
  }

  private static toSessionStatus(status: SessionStatusResponse) : SessionStatusInfo {
    switch (status) {
      case SessionStatusResponse.NotStarted:
        return sessionStatus.notStarted;
      case SessionStatusResponse.InProgress:
        return sessionStatus.inProgress;
      case SessionStatusResponse.Complete:
        return sessionStatus.complete;
      default:
        throw new Error(`Unknown status: ${status as number}`);
    }
  }
}

interface SessionResponse {
  SessionGuid: string;
  Status: SessionStatusResponse;
  Response: InstallJobResponse[];
  LastUsed: string;
}

enum SessionStatusResponse {
  NotStarted = 0,
  InProgress = 1,
  Complete = 2,
}

interface InstallJobResponse {
  Name: string;
  Packages: PackageJobResponse[];
  Failures: string[];
  Attempted: boolean;
  Success: boolean;
  CanInstall: boolean;
}

interface PackageJobResponse {
  Name: string;
  Dependencies: PackageDependencyResponse[];
  VersionStr: string;
  CanInstall: boolean;
}

interface PackageDependencyResponse {
  IsPackageDependency: boolean;
  PackageName: string;
  DependencyVersion: string;
}
