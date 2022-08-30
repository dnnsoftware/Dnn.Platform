import { Component, Element, Event, EventEmitter, Host, h, Prop, State } from '@stencil/core';
import state from '../../store/store';
import { FileDetails, ItemsClient, SaveFileDetailsRequest } from '../../services/ItemsClient';

@Component({
  tag: 'dnn-rm-edit-file',
  styleUrl: 'dnn-rm-edit-file.scss',
  shadow: true,
})
export class DnnRmEditFile {
  
  /** The ID of the folder to edit. */
  @Prop() fileId!: number;

  /**
  * Fires when there is a possibility that some folders have changed.
  * Can be used to force parts of the UI to refresh.
  */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;

  @Element() el: HTMLDnnRmEditFolderElement;
  @State() fileDetails: FileDetails;

  private itemsClient: ItemsClient;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  componentWillLoad(){
    this.itemsClient.getFileDetails(this.fileId)
      .then(fileDetails => {
        this.fileDetails = fileDetails;
      })
      .catch(reason => alert(reason));
  }

  private closeModal(): void {
    const modal = this.el.parentElement as HTMLDnnModalElement;
    modal.hide().then(() => {
      setTimeout(() => {
        document.body.removeChild(modal);
      }, 300);
    });
  }

  private handleSave(): void {
    const request: SaveFileDetailsRequest = {
      fileId: this.fileId,
      fileName: this.fileDetails.fileName,
      title: this.fileDetails.title,
      description: this.fileDetails.description,
    };
    this.itemsClient.saveFileDetails(request)
    .then(() => this.closeModal())
    .catch(reason => alert(reason));
  }

  render() {
    return (
      <Host>
        <h2>{state.localization?.Edit}</h2>
        <dnn-tabs>
          <dnn-tab tabTitle={state.localization?.General}>
            <div class="general">
              <div class="left">
                {this.fileDetails && this.fileDetails.iconUrl &&
                  <img src={this.fileDetails.iconUrl} />
                }
                {this.fileDetails &&
                  <div class="form">
                    <label>{state.localization?.FileId}</label>
                    <span>{this.fileDetails.fileId}</span>

                    <label>{state.localization?.Created}</label>
                    <span>{this.fileDetails.createdBy}</span>

                    <label>{state.localization?.CreatedOnDate}</label>
                    <span>{this.fileDetails.createdOnDate}</span>

                    <label>{state.localization?.LastModified}</label>
                    <span>{this.fileDetails.lastModifiedBy}</span>

                    <label>{state.localization?.LastModifiedOnDate}</label>
                    <span>{this.fileDetails.lastModifiedOnDate}</span>
                  </div>
                }
              </div>
              <div class="right">
                <div class="form">
                  <label>{state.localization?.Name}</label>
                  <input type="text"
                    value={this.fileDetails?.fileName}
                    onInput={e =>
                      this.fileDetails = {
                        ...this.fileDetails,
                        fileName: (e.target as HTMLInputElement).value,
                      }
                    }
                  />
                  
                  <label>{state.localization?.Title}</label>
                  <input type="text"
                    value={this.fileDetails?.title}
                    onInput={e =>
                      this.fileDetails = {
                        ...this.fileDetails,
                        title: (e.target as HTMLInputElement).value.substring(0, 255),
                      }
                    }
                  />

                  <label>{state.localization?.Description}</label>
                  <textarea
                    value={this.fileDetails?.description}
                    onInput={e =>
                      this.fileDetails = {
                        ...this.fileDetails,
                        description: (e.target as HTMLTextAreaElement).value.substring(0, 499),
                      }
                    }
                  />
                </div>
              </div>
            </div>
          </dnn-tab>
        </dnn-tabs>
        <div class="controls">
          <dnn-button
            type="primary"
            reversed
            onClick={() => this.closeModal()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            type="primary"
            onClick={() => this.handleSave()}
          >
            {state.localization.Save}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
