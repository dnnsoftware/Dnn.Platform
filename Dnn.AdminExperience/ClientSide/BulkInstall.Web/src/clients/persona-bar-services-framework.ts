interface PersonaBarServiceFramework {
  antiForgeryToken: string;
  moduleRoot: string;
  controller: string;
  getServiceRoot: () => string;
}

interface PersonaBarUtility {
  sf: PersonaBarServiceFramework;
  getResx: (moduleName: string, key: string) => string;
}

interface BulkInstallInitResult {
  utility: PersonaBarUtility;
}

export class PersonaBarServicesFramework {
  private readonly serviceRoot: string;
  private readonly antiForgeryToken: string;
  private readonly utility: PersonaBarUtility;

  constructor() {
    const dnnWindow = window as unknown as {
      dnn?: {
        initBulkInstall?: () => BulkInstallInitResult;
      };
    };

    const initBulkInstall = dnnWindow.dnn?.initBulkInstall;
    if (initBulkInstall === undefined) {
      throw new Error('window.dnn.initBulkInstall() is not available.');
    }

    const result = initBulkInstall();
    const sf = result.utility.sf;
    sf.moduleRoot = 'PersonaBar';
    sf.controller = '';

    this.serviceRoot = sf.getServiceRoot();
    this.antiForgeryToken = sf.antiForgeryToken;
    this.utility = result.utility;
  }

  getServiceRoot(): string {
    return this.serviceRoot;
  }

  getResx(key: string): string {
    return this.utility.getResx('BulkInstall', key);
  }

  getModuleHeaders(): Headers {
    const headers = new Headers();
    headers.append('RequestVerificationToken', this.antiForgeryToken);
    return headers;
  }
}
