import { Component, Host, h, Prop, State, Element } from '@stencil/core';
import state from '../../store/store';
import { getFileSize } from '../../utilities/filesize-utilities';
import { DnnServicesFramework } from '@dnncommunity/dnn-elements';
import { UploadFromLocalResponse } from './UploadFromLocalResponse';

@Component({
    tag: 'dnn-rm-queued-file',
    styleUrl: 'dnn-rm-queued-file.scss',
    shadow: true,
})
export class DnnRmQueuedFile {

    /** The file to upload. */
    @Prop() file!: File;

    /** Whether to extract uploaded zip files. */
    @Prop() extract = false;

    /** The validation code to use for uploads. */
    @Prop() validationCode!: string;

    /** Optionally limit the file types that can be uploaded. */
    @Prop() filter!: string;

    /** The maximal allowed file upload size */
    @Prop() maxUploadFileSize!: number;

    @State() overwrite = false;
    @State() fileUploadResults: UploadFromLocalResponse;
    @State() progress: number;
    @State() successMessage: string;

    @Element() el: HTMLDnnRmQueuedFileElement;

    private servicesFramework: DnnServicesFramework;
    private xhr = new XMLHttpRequest();

    constructor() {
        this.servicesFramework = new DnnServicesFramework(state.moduleId);
    }

    async componentDidLoad() {
        try {
            this.fileUploadResults = await this.uploadFile();
            if (this.fileUploadResults.message === null) {
                this.successMessage = state.localization.FileUploadedMessage;
                setTimeout(() => {
                    void this.dismiss();
                }, 3000);
                return;
            }
        } catch (err) {
            console.log(err);
        }
    }

    private dismiss() {
        return new Promise<void>((resolve, reject) => {
            try {
                this.el.style.transition = 'all 1s ease-in-out';
                this.el.style.overflow = 'hidden';
                this.el.style.height = this.el.offsetHeight.toFixed(2) + 'px';
                requestAnimationFrame(() => {
                    this.el.style.height = "0";
                    this.el.style.opacity = "0";
                    this.el.style.border = "0";
                });
                setTimeout(() => {
                    resolve();
                }, 1000);
            }
            catch (error) {
                reject(error);
            }
        });
    }

    private async uploadFile(): Promise<UploadFromLocalResponse> {
        return new Promise<UploadFromLocalResponse>((resolve, reject) => {
            const extension = this.file.name.split('.').pop().toLowerCase();
            if (this.filter.split(',').indexOf(extension) === -1) {
                const message = `'.${extension}' ${state.localization.InvalidExtensionMessage}`;
                this.fileUploadResults = {
                    alreadyExists: false,
                    message: message,
                    fileIconUrl: undefined,
                    fileName: this.file.name,
                    fileId: -1,
                    orientation: 0,
                };
                reject(message);
                return;
            }

            if (this.file.size > this.maxUploadFileSize) {
                const message = `${this.file.name} ${state.localization.FileSizeErrorMessage} ${getFileSize(this.maxUploadFileSize)}`;
                this.fileUploadResults = {
                    alreadyExists: false,
                    message: message,
                    fileIconUrl: undefined,
                    fileName: this.file.name,
                    fileId: -1,
                    orientation: 0,
                };
                reject(message);
                return;
            }

            const headers = this.servicesFramework.getModuleHeaders();

            const formData = new FormData();
            formData.append("folder", state.currentItems.folder.folderPath);
            formData.append("filter", this.filter);
            formData.append("extract", this.extract ? "true" : "false");
            formData.append("overwrite", this.overwrite ? "true" : "false");
            formData.append("validationCode", this.validationCode);
            formData.append("isHostPortal", state.settings.IsHostPortal ? "true" : "false");
            formData.append("postfile", this.file);
            this.xhr.onload = () => {
                if (this.xhr.status === 200) {
                    try {
                        const result = JSON.parse(this.xhr.response as string) as unknown as UploadFromLocalResponse;
                        resolve(result);
                    } catch (e) {
                        reject(e);
                    }
                } else {
                    reject(this.xhr.statusText);
                }
            };

            this.xhr.upload.onprogress = e => {
                if (e.lengthComputable) {
                    const percent = Math.round((e.loaded / e.total) * 100);
                    this.progress = percent;
                }
            };

            this.xhr.onabort = () => {
                void this.dismiss();
            };

            const url = this.servicesFramework.getServiceRoot('InternalServices') + 'FileUpload/UploadFromLocal';
            this.xhr.open("POST", url);
            headers.forEach((value, key) => {
                this.xhr.setRequestHeader(key, value);
            });
            this.xhr.send(formData);
        });
    }

    private async handleOvewrite(): Promise<void> {
        this.overwrite = true;
        this.fileUploadResults = null;
        try {
            this.fileUploadResults = await this.uploadFile();
            await this.dismiss();
        } catch (err) {
            alert(err);
        }
    }

    render() {
        return (
            <Host>
                <div class="container">
                    <div class="preview">
                        {this.fileUploadResults && this.fileUploadResults.fileIconUrl &&
                            <img src={this.fileUploadResults.fileIconUrl} alt={this.fileUploadResults.fileName} />
                        }
                    </div>
                    <div class="file">
                        <span>{this.file.name} ({getFileSize(this.file.size)})</span>
                        {this.progress > 0 &&
                            <div class="progress">
                                <div class="progress-bar" style={{ width: `${this.progress}%` }}></div>
                            </div>
                        }
                        {this.fileUploadResults && this.fileUploadResults.message &&
                            <div class="warning">
                                <span>{this.fileUploadResults.message}</span>
                                {this.fileUploadResults.alreadyExists &&
                                    [
                                    <dnn-button
                                        appearance="primary"
                                        onClick={() => void this.handleOvewrite()}
                                    >
                                        {state.localization.Overwrite}
                                    </dnn-button>
                                    ,
                                    <dnn-button
                                        appearance="primary"
                                        reversed
                                        onClick={() => void this.dismiss()}
                                    >
                                        {state.localization.Cancel}
                                    </dnn-button>
                                    ]
                                }
                            </div>
                        }
                        {this.successMessage &&
                            <div class="success">
                                {this.successMessage}
                            </div>
                        }
                    </div>
                        {this.successMessage === undefined &&
                            <div class="dismiss">
                                <button
                                    title={state.localization.Cancel}
                                    onClick={() => {
                                        this.xhr.abort();
                                        void this.dismiss();
                                    }}>
                                    <svg xmlns="http://www.w3.org/2000/svg" height="48" width="48"><path d="m28.55 44-2.15-2.15 5.7-5.65-5.7-5.65 2.15-2.15 5.65 5.7 5.65-5.7L42 30.55l-5.7 5.65 5.7 5.65L39.85 44l-5.65-5.7ZM6 31.5v-3h15v3Zm0-8.25v-3h23.5v3ZM6 15v-3h23.5v3Z"/></svg>
                                </button>
                            </div>
                        }
                        {this.successMessage &&
                            <div class="uploaded">
                                <svg xmlns="http://www.w3.org/2000/svg" height="48" width="48" class="success">
                                    <path d="M21.05 33.1 35.2 18.95l-2.3-2.25-11.85 11.85-6-6-2.25 2.25ZM24 44q-4.1 0-7.75-1.575-3.65-1.575-6.375-4.3-2.725-2.725-4.3-6.375Q4 28.1 4 24q0-4.15 1.575-7.8 1.575-3.65 4.3-6.35 2.725-2.7 6.375-4.275Q19.9 4 24 4q4.15 0 7.8 1.575 3.65 1.575 6.35 4.275 2.7 2.7 4.275 6.35Q44 19.85 44 24q0 4.1-1.575 7.75-1.575 3.65-4.275 6.375t-6.35 4.3Q28.15 44 24 44Zm0-3q7.1 0 12.05-4.975Q41 31.05 41 24q0-7.1-4.95-12.05Q31.1 7 24 7q-7.05 0-12.025 4.95Q7 16.9 7 24q0 7.05 4.975 12.025Q16.95 41 24 41Zm0-17Z"/>
                                </svg>
                            </div>
                        }
                </div>
            </Host>
        );
    }
}
