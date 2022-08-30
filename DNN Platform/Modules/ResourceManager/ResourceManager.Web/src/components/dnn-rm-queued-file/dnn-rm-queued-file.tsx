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

    @Element() el: HTMLDnnRmQueuedFileElement;

    private servicesFramework: DnnServicesFramework;
    private xhr = new XMLHttpRequest();

    constructor() {
        this.servicesFramework = new DnnServicesFramework(state.moduleId);
    }

    componentDidLoad() {
        this.uploadFile()
            .then(response => {
                this.fileUploadResults = JSON.parse(response);
                if (this.fileUploadResults.message === null){
                    setTimeout(() => {
                        this.dismiss();
                    }, 2000);
                    return;
                }
            })
            .catch(err => console.log(err));
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

    private uploadFile(){
        return new Promise<string>((resolve, reject) => {
            const extension = this.file.name.split('.').pop();
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
            formData.append("isHostPortal", "false");
            formData.append("postfile", this.file);
            this.xhr.onload = () => {
                if (this.xhr.status === 200) {
                    resolve(this.xhr.response);
                } else {
                    reject(this.xhr.statusText);
                }
            }
            
            this.xhr.upload.onprogress = e => {
                if (e.lengthComputable) {
                    const percent = Math.round((e.loaded / e.total) * 100);
                    this.progress = percent;
                }
            }

            this.xhr.onabort = () => {
                this.dismiss();
            };
            
            this.xhr.open("POST", "/API/InternalServices/FileUpload/UploadFromLocal");
            headers.forEach((value, key) => {
                this.xhr.setRequestHeader(key, value);
            });
            this.xhr.send(formData);
        });
    }

    handleOvewrite(): void {
        this.overwrite = true;
        this.fileUploadResults = null;
        this.uploadFile()
        .then(response => {
            this.fileUploadResults = JSON.parse(response);
            return this.dismiss();
        })
        .catch(err => alert(err));
    }

    render() {
        return (
            <Host>
                <div class="container">
                    <div class="preview">
                        {this.fileUploadResults && this.fileUploadResults.fileIconUrl &&
                            <img src={this.fileUploadResults.fileIconUrl} />
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
                                        type="primary"
                                        onClick={() => this.handleOvewrite()}
                                    >
                                        {state.localization.Overwrite}
                                    </dnn-button>
                                    ,
                                    <dnn-button
                                        type="primary"
                                        reversed
                                        onClick={() => this.dismiss()}
                                    >
                                        {state.localization.Cancel}
                                    </dnn-button>
                                    ]
                                }
                            </div>
                        }
                    </div>
                    <div class="dismiss">
                        <button
                            title={state.localization.Cancel}
                            onClick={() => {
                                this.xhr.abort();
                                this.dismiss();
                            }}>
                            <svg xmlns="http://www.w3.org/2000/svg" height="48" width="48"><path d="m28.55 44-2.15-2.15 5.7-5.65-5.7-5.65 2.15-2.15 5.65 5.7 5.65-5.7L42 30.55l-5.7 5.65 5.7 5.65L39.85 44l-5.65-5.7ZM6 31.5v-3h15v3Zm0-8.25v-3h23.5v3ZM6 15v-3h23.5v3Z"/></svg>
                        </button>
                    </div>
                </div>
            </Host>
        );
    }
}
