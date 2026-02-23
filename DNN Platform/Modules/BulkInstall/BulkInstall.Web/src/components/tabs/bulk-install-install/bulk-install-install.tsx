import { Component, Fragment, Host, h, State } from '@stencil/core';
import store from '../../../stores/store';
import { InstallJob, Session } from './bulk-install-install.model';
import { InstallClient } from '../../../clients/install-client';
import { sessionStatus } from '../../../enums/SessionStatus';

@Component({
  tag: 'bulk-install-install',
  styleUrl: 'bulk-install-install.scss',
  shadow: true,
})
export class BulkInstallInstall {
  @State() private selectedFiles: File[] = [];
  @State() private installationSummary: InstallJob[] = [];
  @State() private cannotInstall = false;
  @State() private installationComplete = false;
  @State() private apiError = false;
  @State() private session: Session | undefined;
  @State() private maxUploadFileSize: number = 0;

  private installClient: InstallClient;

  constructor() {
    this.installClient = new InstallClient(store.moduleId);
  }

  async componentDidLoad() {
    try {
      const { session, maxUploadFileSize } = await this.installClient.create();
      this.session = session;
      this.maxUploadFileSize = maxUploadFileSize;
    } catch (err) {
      console.error(err);
    }
  }

  private async installPackages() {
    this.installationSummary = await this.installClient.summary(this.session.sessionGuid);
    if (this.installationSummary.some(installJob => !installJob.canInstall)) {
      this.cannotInstall = true;
      return;
    }

    await this.installClient.install(this.session.sessionGuid);

    const summaryWait = 1000;
    const updateSummary = async () => {
      try {
        this.session = await this.installClient.getSession(this.session.sessionGuid);
        this.apiError = false;
        this.installationSummary = this.session.response;
        if (this.session.status === sessionStatus.complete) {
          this.installationComplete = true;
          return;
        }
      } catch (error) {
        this.apiError = true;
        console.error('Error getting install session, retrying', error);
      }
      setTimeout(() => {
        updateSummary().catch(console.error);
        return;
      }, summaryWait);
    };
    await updateSummary();
  }

  render() {
    return (
      <Host>
        <div class="row">
          <div class="col">
            <div class="panel">
              {this.installationSummary.length === 0 && (
                <>
                  <div class="panel-heading">
                    <h3 class="panel-title">Upload Install Package(s)</h3>
                  </div>
                  <div class="panel-body">
                    <dnn-dropzone
                      allowedExtensions={['zip']}
                      maxFileSize={this.maxUploadFileSize}
                      onFilesSelected={e => (this.selectedFiles = [...this.selectedFiles, ...e.detail])}
                      multiple
                      resx={{
                        dragAndDropFile: store.resx.DropZone_DragAndDropFile,
                        or: store.resx.DropZone_Or,
                        uploadFile: store.resx.DropZone_UploadFile,
                        uploadSizeTooLarge: store.resx.DropZone_UploadSizeTooLarge,
                        fileSizeLimit: store.resx.DropZone_FileSizeLimit,
                        invalidExtension: store.resx.DropZone_InvalidExtension,
                        allowedFileExtensions: store.resx.DropZone_AllowedFileExtensions,
                      }}
                    />
                    {this.selectedFiles.map(file => (
                      <bulk-install-queued-file file={file} session={this.session} maxUploadFileSize={this.maxUploadFileSize} />
                    ))}
                    <div class="form-group">
                      <dnn-button
                        disabled={this.selectedFiles.length < 1}
                        onClick={() => {
                          this.installPackages().catch(console.error);
                          return;
                        }}
                      >
                        {store.resx.Install}
                      </dnn-button>
                      <dnn-button appearance="tertiary" reversed onClick={() => (this.selectedFiles = [])}>
                        {store.resx.Reset}
                      </dnn-button>
                    </div>
                  </div>
                </>
              )}
              {this.installationSummary.length > 0 && (
                <>
                  <div class="panel-heading">
                    <h3 class="panel-title">{store.resx.InstallingPackages}</h3>
                  </div>
                  <div
                    class={
                      'panel-body ' +
                      (this.session.status === sessionStatus.complete
                        ? 'session__complete'
                        : this.session.status === sessionStatus.inProgress
                          ? 'session__in-progress'
                          : 'session__not-started')
                    }
                  >
                    {this.apiError && <h4 class="danger">{store.resx.ApiError}</h4>}
                    {this.cannotInstall && <h4 class="danger">{store.resx.CannotInstall}</h4>}
                    {this.installationComplete && <h4 class={this.installationSummary.every(j => j.success) ? 'success' : 'danger'}>{store.resx.InstallationComplete}</h4>}
                    <ol>
                      {this.installationSummary.map(installJob => (
                        <li
                          class={
                            !installJob.canInstall ? 'install__invalid' : installJob.success ? 'install__success' : installJob.attempted ? 'install__failed' : 'install__pending'
                          }
                        >
                          <h3>{installJob.name}</h3>
                          {installJob.failures?.length > 0 && (
                            <ul>
                              {installJob.failures.map(failure => (
                                <li>{failure}</li>
                              ))}
                            </ul>
                          )}
                          <ul>
                            {installJob.packages.map(packageJob => (
                              <li class={!packageJob.canInstall ? 'package__invalid' : 'package__valid'}>
                                <h4>{packageJob.name}</h4>
                                {packageJob.version}
                              </li>
                            ))}
                          </ul>
                        </li>
                      ))}
                    </ol>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      </Host>
    );
  }
}
