import { Component, Fragment, h, Host, State } from '@stencil/core';
import store from '../../../stores/store';
import { InstallClient } from '../../../clients/install-client';
import { sessionStatus } from '../../../enums/SessionStatus';
import { InstallJob, Session, UploadStatus } from './dnn-bi-install.model';

type FileViewModel = { type: 'pending'; file: File } | { type: 'error'; file: File } | { type: 'uploaded'; job: InstallJob };

type InstallStatus = { type: 'uploading' } | { type: 'cannotInstall' } | { type: 'installing' } | { type: 'installed' };

function toFileViewModel(fileOrJob: File | InstallJob): FileViewModel {
  if (fileOrJob instanceof File) {
    return { type: 'pending', file: fileOrJob };
  } else {
    return { type: 'uploaded', job: fileOrJob };
  }
}

function getCanInstall(file: FileViewModel): boolean {
  if (file.type === 'uploaded') {
    return file.job.canInstall;
  }

  return false;
}

@Component({
  tag: 'dnn-bi-install',
  styleUrl: 'dnn-bi-install.scss',
  shadow: true,
})
export class DnnBiInstall {
  @State() private files: FileViewModel[] = [];
  @State() private session: Session | undefined;
  @State() private maxUploadFileSize: number = 0;
  @State() private installStatus: InstallStatus = { type: 'uploading' };
  @State() private apiError = false;

  private installClient: InstallClient;
  private summaryAbortController: AbortController | null = null;

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
    if (status === UploadStatus.Cancelled) {
      this.files = [...this.files.filter(f => f !== file)];
    } else if (status === UploadStatus.Error) {
      this.files = [...this.files.map((f): FileViewModel => (file.type === 'pending' ? (f !== file ? f : { type: 'error', file: file.file }) : file))];
    } else if (status === UploadStatus.Success) {
      await this.getInstallationSummary();
    }
  }

  private async getInstallationSummary() {
    const reason = 'Cancelling in-progress summary request in order to start new request';
    this.summaryAbortController?.abort(reason);
    try {
      this.summaryAbortController = new AbortController();
      const jobs = await this.installClient.summary(this.session.sessionGuid, this.summaryAbortController.signal);
      this.summaryAbortController = null;

      this.receiveInstallationSummary(jobs);
    } catch (err) {
      if (err !== reason) {
        throw err;
      }
    }
  }

  private receiveInstallationSummary(jobs: InstallJob[]) {
    const jobViewModels = jobs.map(j => toFileViewModel(j));
    const fileViewModels = this.files.filter(f => f.type !== 'uploaded' && jobs.every(j => j.name !== f.file.name));
    this.files = [...fileViewModels, ...jobViewModels];
  }

  private async reset() {
    const { session } = await this.installClient.create();
    this.session = session;
    this.files = [];
  }

  private async installPackages() {
    this.installStatus = { type: 'installing' };
    await this.getInstallationSummary();
    if (this.files.some(file => !getCanInstall(file))) {
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
              <div class="panel-heading">
                <h3 class="panel-title">
                  {this.installStatus.type === 'uploading'
                    ? store.resx.UploadInstallPackages
                    : this.installStatus.type === 'installing'
                      ? store.resx.InstallingPackages
                      : this.installStatus.type === 'installed'
                        ? store.resx.InstallationComplete
                        : store.resx.CannotInstall}
                </h3>
              </div>
              <div class="panel-body">
                {this.apiError && <h4 class="danger">{store.resx.ApiError}</h4>}
                {this.installStatus.type === 'uploading' && (
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
                )}
                {this.files.map(file => (
                  <>
                    {file.type === 'uploaded' && <dnn-bi-install-job key={file.job.name} job={file.job} />}
                    {file.type !== 'uploaded' && (
                      <dnn-bi-queued-file
                        key={file.file.name}
                        file={file.file}
                        session={this.session}
                        maxUploadFileSize={this.maxUploadFileSize}
                        onUploadCompleted={e => this.handleUploadCompleted(file, e.detail)}
                      />
                    )}
                  </>
                ))}
                {this.installStatus.type === 'uploading' && (
                  <div class="form-group">
                    <dnn-button
                      disabled={this.files.length < 1 || this.files.some(f => getCanInstall(f) === false)}
                      onClick={() => {
                        this.installPackages().catch(console.error);
                        return;
                      }}
                    >
                      {store.resx.Install}
                    </dnn-button>
                    <dnn-button appearance="tertiary" reversed onClick={() => this.reset()}>
                      {store.resx.Reset}
                    </dnn-button>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </Host>
    );
  }
}
