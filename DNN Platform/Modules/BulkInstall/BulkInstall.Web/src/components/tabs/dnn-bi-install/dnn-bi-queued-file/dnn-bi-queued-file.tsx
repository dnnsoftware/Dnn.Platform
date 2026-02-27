import { Component, Element, Event, EventEmitter, h, Host, Prop, State } from '@stencil/core';
import state from '../../../../stores/store';
import { getFileSize } from '../../../../utilities/filesize-utilities';
import { Session, UploadStatus } from '../dnn-bi-install.model';
import { InstallClient } from '../../../../clients/install-client';

@Component({
  tag: 'dnn-bi-queued-file',
  styleUrl: 'dnn-bi-queued-file.scss',
  shadow: true,
})
export class DnnBiQueuedFile {
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

  @Element() el: HTMLDnnBiQueuedFileElement;

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
                <dnn-bi-dismiss-icon />
              </button>
            </div>
          )}
          {this.successMessage && (
            <div class="uploaded">
              <dnn-bi-checkmark-icon />
            </div>
          )}
        </div>
      </Host>
    );
  }
}
