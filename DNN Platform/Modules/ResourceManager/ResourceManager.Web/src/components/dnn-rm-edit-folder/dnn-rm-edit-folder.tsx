import { Component, Host, h, Element, State, Prop, Event, EventEmitter } from '@stencil/core';
import { CreateNewFolderRequest, FolderMappingInfo, ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';

@Component({
  tag: 'dnn-rm-edit-folder',
  styleUrl: 'dnn-rm-edit-folder.scss',
  shadow: true,
})
export class DnnRmEditFolder {
  /** The ID of the parent folder of the one being edited. */
  @Prop() parentFolderId!: number;

  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;
  
  @Element() el : HTMLDnnRmEditFolderElement
  
  @State() folderMappings: FolderMappingInfo[];
  
  @State() newFolderRequest: CreateNewFolderRequest = {
    FolderMappingId: -1,
    FolderName: "",
    ParentFolderId: this.parentFolderId,
  }

  private readonly itemsClient: ItemsClient;
  private nameField: HTMLInputElement;

  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  componentWillLoad() {
    this.itemsClient.getFolderMappings()
    .then(data => {
      this.folderMappings = data.sort((a, b) => a.FolderMappingID - b.FolderMappingID);
      this.newFolderRequest = {
        ...this.newFolderRequest,
        FolderMappingId: this.folderMappings[0].FolderMappingID,
        ParentFolderId: this.parentFolderId,
      };
    })
    .catch(reason => console.error(reason));
  }

  componentDidLoad() {
    setTimeout(() => {
      this.nameField.focus();
    }, 350);
  }

  private handleCancel(): void {
    const modal = this.el.parentElement as HTMLDnnModalElement;
    modal.hide().then(() => {
      setTimeout(() => {
        document.body.removeChild(modal);
      }, 300);
    });
  }

  private handleTypeChanged(e: Event): void {
    const select = e.target as HTMLSelectElement;
    const option = select.options[select.selectedIndex] as HTMLOptionElement;
    const newValue = Number.parseInt(option.value);
    this.newFolderRequest = {
      ...this.newFolderRequest,
      FolderMappingId: newValue,
    };
  }

  private handleSave(): void {
    this.itemsClient.createNewFolder(this.newFolderRequest)
    .then(() => {
      this.dnnRmFoldersChanged.emit();
      state.currentItems = {
        ...state.currentItems,
        items: [],
      };
      const modal = this.el.parentElement as HTMLDnnModalElement;
      modal.hide().then(() => {
        setTimeout(() => {
          document.body.removeChild(modal);
        }, 300);
      });
    })
    .catch(error => alert(error));
  }

  render() {
    return (
      <Host>
        <h2>{state.localization.AddFolder}</h2>
        <div class="form">
          <label>{state.localization.Name}</label>
          <input
            type="text" required
            ref={el => this.nameField = el}
            value={this.newFolderRequest.FolderName}
            onInput={e => this.newFolderRequest = {
              ...this.newFolderRequest,
              FolderName: (e.target as HTMLInputElement).value,
            }}
          />
          {!this.newFolderRequest.FolderName &&
            <span>{state.localization.FolderNameRequiredMessage}</span>
          }
          <label>{state.localization.Type}</label>
          <select onChange={e => this.handleTypeChanged(e)}>
            {this.folderMappings?.map((folderMapping, index) => 
              <option
                value={folderMapping.FolderMappingID}
                selected={index == 0}>
                {folderMapping.MappingName}
              </option>
            )}
          </select>
        </div>
        <div class="controls">
          <dnn-button
            type="primary"
            reversed
            onClick={() => this.handleCancel()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            type="primary"
            disabled={
              this.newFolderRequest.FolderMappingId < 0 ||
              this.newFolderRequest.FolderName.length < 1
            }
            onClick={() => this.handleSave()}
          >
            {state.localization.Save}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
