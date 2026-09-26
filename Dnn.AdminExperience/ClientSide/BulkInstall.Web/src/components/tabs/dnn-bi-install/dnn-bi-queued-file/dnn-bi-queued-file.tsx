import { Component, Element, Event, EventEmitter, h, Host, Prop, State } from '@stencil/core';
import store from '../../../../stores/store';
import { getFileSize } from '../../../../utilities/filesize-utilities';
import { InstallClient } from '../../../../clients/install-client';
import { Session, UploadStatus } from '../dnn-bi-install.model';

@Component({
  tag: 'dnn-bi-queued-file',
  styleUrl: 'dnn-bi-queued-file.scss',
  shadow: true,
})
export class DnnBiQueuedFile {
  private static readonly maxConcurrentUploads = 3;
  private static activeUploads = 0;
  private static uploadQueue: Array<() => void> = [];

  /** The file to upload. */
  @Prop() file!: File;

  /** The current session. */
  @Prop() session!: Session;

  /** The maximal allowed file upload size. */
  @Prop() maxUploadFileSize!: number;

  /** Fires when the upload is completed. */
  @Event() uploadCompleted!: EventEmitter<UploadStatus>;

  @State() overwrite = false;
  @State() progress = 0;
  @State() successMessage?: string;
  @State() dismissed = false;
  @State() uploadStarted = false;

  @Element() el!: HTMLDnnBiQueuedFileElement;

  private installClient: InstallClient;
  private abortController?: AbortController;
  private hasUploadSlot = false;

  constructor() {
    this.installClient = new InstallClient();
  }

  async componentDidLoad() {
    try {
      this.abortController = new AbortController();
      await this.acquireUploadSlot(this.abortController.signal);
      this.hasUploadSlot = true;
      this.uploadStarted = true;
      await this.installClient.addPackage(this.session.sessionGuid, this.file, this.abortController.signal, ev => this.onProgress(ev));
      this.uploadCompleted.emit(UploadStatus.Success);
      this.successMessage = store.resx.FileUploadedMessage;
    } catch (err) {
      if (this.dismissed) {
        this.uploadCompleted.emit(UploadStatus.Cancelled);
      } else {
        this.uploadCompleted.emit(UploadStatus.Error);
      }
      console.log(err);
    } finally {
      if (this.hasUploadSlot) {
        this.releaseUploadSlot();
        this.hasUploadSlot = false;
      }
    }
  }

  private acquireUploadSlot(signal: AbortSignal): Promise<void> {
    if (signal.aborted) {
      return Promise.reject(new Error('Upload cancelled'));
    }

    if (DnnBiQueuedFile.activeUploads < DnnBiQueuedFile.maxConcurrentUploads) {
      DnnBiQueuedFile.activeUploads++;
      return Promise.resolve();
    }

    return new Promise<void>((resolve, reject) => {
      const resumeUpload = () => {
        signal.removeEventListener('abort', onAbort);
        DnnBiQueuedFile.activeUploads++;
        resolve();
      };

      const onAbort = () => {
        const queueIndex = DnnBiQueuedFile.uploadQueue.indexOf(resumeUpload);
        if (queueIndex >= 0) {
          DnnBiQueuedFile.uploadQueue.splice(queueIndex, 1);
        }

        reject(new Error('Upload cancelled'));
      };

      signal.addEventListener('abort', onAbort, { once: true });
      DnnBiQueuedFile.uploadQueue.push(resumeUpload);
    });
  }

  private releaseUploadSlot() {
    if (DnnBiQueuedFile.activeUploads > 0) {
      DnnBiQueuedFile.activeUploads--;
    }

    const nextUpload = DnnBiQueuedFile.uploadQueue.shift();
    nextUpload?.();
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

  private getUploadStatusTooltip() {
    return this.uploadStarted ? store.resx.BulkInstallStatus_Uploading : store.resx.BulkInstallStatus_QueuedForUpload;
  }

  render() {
    const uploadStatusTooltip = this.getUploadStatusTooltip();

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
                aria-label={store.resx.Cancel}
                title={`${uploadStatusTooltip} (${store.resx.Cancel})`}
                onClick={() => {
                  this.abortController?.abort();
                  this.uploadCompleted.emit(UploadStatus.Cancelled);
                  this.dismiss().catch(console.error);
                }}
              >
                <dnn-bi-dismiss-icon />
              </button>
            </div>
          )}
          {this.successMessage && (
            <div class="uploaded" title={store.resx.BulkInstallStatus_Uploaded}>
              <dnn-bi-checkmark-icon />
            </div>
          )}
        </div>
      </Host>
    );
  }
}
