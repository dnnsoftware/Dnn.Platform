import { Component, Element, Event, EventEmitter, h, Host, Prop, State } from '@stencil/core';
import state from '../../../../stores/store';
import { getFileSize } from '../../../../utilities/filesize-utilities';
import { Session, UploadStatus } from '../bulk-install-install.model';
import { InstallClient } from '../../../../clients/install-client';

@Component({
  tag: 'bulk-install-queued-file',
  styleUrl: 'bulk-install-queued-file.scss',
  shadow: true,
})
export class BulkInstallQueuedFile {
  /** The file to upload. */
  @Prop() file!: File;

  /** The current session. */
  @Prop() session!: Session;

  /** The maximal allowed file upload size */
  @Prop() maxUploadFileSize!: number;

  @Event() uploadCompleted: EventEmitter<UploadStatus>;

  @State() overwrite = false;
  @State() progress: number;
  @State() successMessage: string;
  @State() dismissed: boolean;

  @Element() el: HTMLBulkInstallQueuedFileElement;

  private installClient: InstallClient;
  private abortController: AbortController;

  constructor() {
    this.installClient = new InstallClient(state.moduleId);
  }

  async componentDidLoad() {
    try {
      this.abortController = new AbortController();
      await this.installClient.addPackage(this.session.sessionGuid, this.file, this.abortController.signal, ev => this.onProgress(ev));
      this.uploadCompleted.emit(UploadStatus.Success);
      this.successMessage = state.resx.FileUploadedMessage;
    } catch (err) {
      if (this.dismissed) {
        this.uploadCompleted.emit(UploadStatus.Cancelled);
      } else {
        this.uploadCompleted.emit(UploadStatus.Error);
      }
      console.log(err);
    }
  }

  private onProgress(ev: ProgressEvent) {
    if (ev.lengthComputable) {
      const percent = Math.round((ev.loaded / ev.total) * 100);
      this.progress = percent;
    }
  }

  private dismiss() {
    this.dismissed = true;
    return new Promise<void>((resolve, reject) => {
      try {
        this.el.style.transition = 'all 1s ease-in-out';
        this.el.style.overflow = 'hidden';
        this.el.style.height = this.el.offsetHeight.toFixed(2) + 'px';
        requestAnimationFrame(() => {
          this.el.style.height = '0';
          this.el.style.opacity = '0';
          this.el.style.border = '0';
        });
        setTimeout(() => {
          this.el.style.display = 'none';
          resolve();
        }, 1000);
      } catch (error) {
        reject(error);
      }
    });
  }

  render() {
    return (
      <Host>
        <div class="container">
          <div class="preview">
            <img src="/Icons/Sigma/ExtZip_32X32_Standard.png" alt={this.file.name} />
          </div>
          <div class="file">
            <span>
              {this.file.name} ({getFileSize(this.file.size)})
            </span>
            {this.progress > 0 && (
              <div class="progress">
                <div class="progress-bar" style={{ width: `${this.progress}%` }}></div>
              </div>
            )}
            {this.successMessage && <div class="success">{this.successMessage}</div>}
          </div>
          {this.successMessage === undefined && (
            <div class="dismiss">
              <button
                title={state.resx.Cancel}
                onClick={() => {
                  this.abortController.abort();
                  this.uploadCompleted.emit(UploadStatus.Cancelled);
                  this.dismiss().catch(console.error);
                }}
              >
                <svg xmlns="http://www.w3.org/2000/svg" height="48" width="48">
                  <path d="m28.55 44-2.15-2.15 5.7-5.65-5.7-5.65 2.15-2.15 5.65 5.7 5.65-5.7L42 30.55l-5.7 5.65 5.7 5.65L39.85 44l-5.65-5.7ZM6 31.5v-3h15v3Zm0-8.25v-3h23.5v3ZM6 15v-3h23.5v3Z" />
                </svg>
              </button>
            </div>
          )}
          {this.successMessage && (
            <div class="uploaded">
              <svg xmlns="http://www.w3.org/2000/svg" height="48" width="48" class="success">
                <path d="M21.05 33.1 35.2 18.95l-2.3-2.25-11.85 11.85-6-6-2.25 2.25ZM24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 24q0-4.15 1.575-7.8 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24 4q4.15 0 7.8 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Zm0-3q7.1 0 12.05-4.975Q41 31.05 41 24q0-7.1-4.95-12.05Q31.1 7 24 7q-7.05 0-12.025 4.95Q7 16.9 7 24q0 7.05 4.975 12.025Q16.95 41 24 41Zm0-17Z" />
              </svg>
            </div>
          )}
        </div>
      </Host>
    );
  }
}
