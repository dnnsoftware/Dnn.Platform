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

  private async closeModal() {
    const modal = this.el.parentElement as HTMLDnnModalElement;
    await modal.hide();
    setTimeout(() => {
      document.body.removeChild(modal);
    }, 300);
  }

  private async handleSave() {
    try {
      const request: SaveFileDetailsRequest = {
        fileId: this.fileId,
        fileName: this.fileDetails.fileName,
        title: this.fileDetails.title,
        description: this.fileDetails.description,
      };
      await this.itemsClient.saveFileDetails(request);
      await this.closeModal();
      
    } catch (error) {
      alert(error);
    }
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
                  <img src={this.fileDetails.iconUrl} alt={this.fileDetails.fileName} />
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
                  <dnn-input
                    label={state.localization?.Name}
                    required
                    type="text"
                    value={this.fileDetails?.fileName}
                    onValueInput={e =>
                      this.fileDetails = {
                        ...this.fileDetails,
                        fileName: (e.detail as string),
                      }
                    }
                  />
                  <dnn-input
                    label={state.localization?.Title}
                    type="text"
                    value={this.fileDetails?.title}
                    onValueInput={e =>
                      this.fileDetails = {
                        ...this.fileDetails,
                        title: (e.detail as string).substring(0, 255),
                      }
                    }
                  />
                  <dnn-textarea
                    label={state.localization?.Description}
                    value={this.fileDetails?.description}
                    onValueInput={e =>
                      this.fileDetails = {
                        ...this.fileDetails,
                        description: (e.detail).substring(0, 499),
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
            appearance="primary"
            reversed
            onClick={() => void this.closeModal()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            appearance="primary"
            onClick={() => void this.handleSave()}
          >
            {state.localization.Save}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
