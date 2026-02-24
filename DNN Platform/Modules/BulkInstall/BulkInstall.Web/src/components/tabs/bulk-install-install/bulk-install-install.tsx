import { Component, Fragment, h, Host, State } from '@stencil/core';
import store from '../../../stores/store';
import { InstallJob, Session, UploadStatus } from './bulk-install-install.model';
import { InstallClient } from '../../../clients/install-client';
import { sessionStatus } from '../../../enums/SessionStatus';

interface FileViewModel {
  file: File;
  job: InstallJob | undefined;
  status: UploadStatus;
}

type InstallStatus = { type: 'uploading' } | { type: 'cannotInstall' } | { type: 'installing' } | { type: 'installed' };

function toFileViewModel(file: File): FileViewModel {
  return {
    file: file,
    job: undefined,
    status: UploadStatus.InProgress,
  };
}

@Component({
  tag: 'bulk-install-install',
  styleUrl: 'bulk-install-install.scss',
  shadow: true,
})
export class BulkInstallInstall {
  @State() private files: FileViewModel[] = [];
  @State() private session: Session | undefined;
  @State() private maxUploadFileSize: number = 0;
  @State() private installStatus: InstallStatus = { type: 'uploading' };
  @State() private apiError = false;

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

  private async handleUploadCompleted(file: FileViewModel, status: UploadStatus) {
    file.status = status;
    if (status === UploadStatus.Success) {
      await this.getInstallationSummary();
    }
  }

  private async getInstallationSummary() {
    const jobs = await this.installClient.summary(this.session.sessionGuid);
    this.receiveInstallationSummary(jobs);
  }

  private receiveInstallationSummary(jobs: InstallJob[]) {
    this.files = this.files.map(file => {
      const job = jobs.find(j => j.name === file.file.name);
      return { ...file, job: job || file.job };
    });
  }

  private async installPackages() {
    this.installStatus = { type: 'installing' };
    await this.getInstallationSummary();
    if (this.files.some(({ job }) => !job.canInstall)) {
      this.installStatus = { type: 'cannotInstall' };
      return;
    }

    await this.installClient.install(this.session.sessionGuid);

    const summaryWait = 1000;
    const updateSummary = async () => {
      try {
        this.session = await this.installClient.getSession(this.session.sessionGuid);
        this.apiError = false;
        this.receiveInstallationSummary(this.session.response);
        if (this.session.status === sessionStatus.complete) {
          this.installStatus = { type: 'installed' };
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
        <div class={`row ${this.installStatus.type}`}>
          <div class="col">
            <div class="panel">
              {this.installStatus.type === 'uploading' && (
                <>
                  <div class="panel-heading">
                    <h3 class="panel-title">Upload Install Package(s)</h3>
                  </div>
                  <div class="panel-body">
                    <dnn-dropzone
                      allowedExtensions={['zip']}
                      maxFileSize={this.maxUploadFileSize}
                      onFilesSelected={e => (this.files = [...this.files, ...e.detail.map(toFileViewModel)])}
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
                    {this.files.map(file => (
                      <bulk-install-queued-file
                        key={file.file.name}
                        file={file.file}
                        session={this.session}
                        maxUploadFileSize={this.maxUploadFileSize}
                        onUploadCompleted={e => this.handleUploadCompleted(file, e.detail)}
                      />
                    ))}
                    <div class="form-group">
                      <dnn-button
                        disabled={this.files.length < 1 && this.files.every(f => f.status === UploadStatus.Success)}
                        onClick={() => {
                          this.installPackages().catch(console.error);
                          return;
                        }}
                      >
                        {store.resx.Install}
                      </dnn-button>
                      <dnn-button appearance="tertiary" reversed onClick={() => (this.files = [])}>
                        {store.resx.Reset}
                      </dnn-button>
                    </div>
                  </div>
                </>
              )}
              {this.installStatus.type === 'cannotInstall' && (
                <>
                  <div class="panel-heading">
                    <h3 class="panel-title">{store.resx.InstallingPackages}</h3>
                  </div>
                  <div class="panel-body">
                    <h4 class="danger">{store.resx.CannotInstall}</h4>
                    {this.apiError && <h4 class="danger">{store.resx.ApiError}</h4>}
                    <ol>
                      {this.files.map(({ job }) => (
                        <li class={!job.canInstall ? 'install__invalid' : 'install__valid'}>
                          <h3>{job.name}</h3>
                          {job.failures?.length > 0 && (
                            <ul>
                              {job.failures.map(failure => (
                                <li>{failure}</li>
                              ))}
                            </ul>
                          )}
                          <ul>
                            {job.packages.map(packageJob => (
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
              {this.installStatus.type === 'installing' && (
                <>
                  <div class="panel-heading">
                    <h3 class="panel-title">{store.resx.InstallingPackages}</h3>
                  </div>
                  <div class="panel-body">
                    {this.apiError && <h4 class="danger">{store.resx.ApiError}</h4>}
                    <ol>
                      {this.files
                        .filter(f => f.job !== undefined)
                        .map(({ job }) => (
                          <li class={job.success ? 'install__success' : job.attempted ? 'install__failed' : 'install__pending'}>
                            <h3>{job.name}</h3>
                            {job.failures?.length > 0 && (
                              <ul>
                                {job.failures.map(failure => (
                                  <li>{failure}</li>
                                ))}
                              </ul>
                            )}
                            <ul>
                              {job.packages.map(packageJob => (
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
              {this.installStatus.type === 'installed' && (
                <>
                  <div class="panel-heading">
                    <h3 class="panel-title">{store.resx.InstallingPackages}</h3>
                  </div>
                  <div class="panel-body">
                    <h4 class={this.files.every(f => f.job.success) ? 'success' : 'danger'}>{store.resx.InstallationComplete}</h4>
                    <ol>
                      {this.files.map(({ job }) => (
                        <li class={job.success ? 'install__success' : 'install__failed'}>
                          <h3>{job.name}</h3>
                          {job.failures?.length > 0 && (
                            <ul class="danger">
                              {job.failures.map(failure => (
                                <li>{failure}</li>
                              ))}
                            </ul>
                          )}
                          <ul>
                            {job.packages.map(packageJob => (
                              <li>
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
