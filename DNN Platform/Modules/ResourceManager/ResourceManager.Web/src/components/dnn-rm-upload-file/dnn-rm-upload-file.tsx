import {
  Component,
  Host,
  h,
  Element,
  Event,
  EventEmitter,
  State,
} from "@stencil/core";
import { ItemsClient } from "../../services/ItemsClient";
import state from "../../store/store";

@Component({
  tag: "dnn-rm-upload-file",
  styleUrl: "dnn-rm-upload-file.scss",
  shadow: true,
})
export class DnnRmUploadFile {
  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;

  @State() queuedFiles: File[] = [];
  @State() alreadyExistingFilesCount = 0;
  @State() uploadingFilesCount = 0;
  @State() uploadedFilesCount = 0;
  @State() failedFilesCount = 0;

  @Element() el: HTMLDnnRmUploadFileElement;

  private itemsClient: ItemsClient;
  private allowedExtensions: string;
  private validationCode: string;
  private maxUploadFileSize: number;
  private extract = false;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  componentWillLoad() {
    this.itemsClient
      .getAllowedFileExtensions()
      .then((data) => {
        this.allowedExtensions = data.allowedExtensions;
        this.validationCode = data.validationCode;
        this.maxUploadFileSize = data.maxUploadFileSize;
      })
      .catch((err) => alert(err));
  }

  private handleFilesDropped(droppedFiles: File[]): void {
    droppedFiles.forEach((file) => {
      this.queuedFiles = [...this.queuedFiles, file as File];
    });
  }

  render() {
    return (
      <Host>
        <h2>{state.localization.Upload}</h2>
        <label>
          <dnn-checkbox onClick={() => (this.extract = !this.extract)}>
            {state.localization.ExtractUploads}
          </dnn-checkbox>
        </label>
        <dnn-dropzone
          onFilesSelected={(e) => this.handleFilesDropped(e.detail)}
        />
        {this.allowedExtensions &&
          this.validationCode &&
          this.queuedFiles.map((file) => (
            <dnn-rm-queued-file
              file={file}
              validationCode={this.validationCode}
              filter={this.allowedExtensions}
              extract={this.extract}
              maxUploadFileSize={this.maxUploadFileSize}
            />
          ))}
      </Host>
    );
  }
}
