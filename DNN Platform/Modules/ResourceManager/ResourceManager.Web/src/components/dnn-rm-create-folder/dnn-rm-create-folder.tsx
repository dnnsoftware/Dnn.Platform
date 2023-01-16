import { Component, Host, h, Element, State, Event, EventEmitter } from '@stencil/core';
import { CreateNewFolderRequest, FolderMappingInfo, ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';

@Component({
  tag: 'dnn-rm-create-folder',
  styleUrl: 'dnn-rm-create-folder.scss',
  shadow: true,
})
export class DnnRmCreateFolder {

  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;
  
  @Element() el : HTMLDnnRmCreateFolderElement
  
  @State() folderMappings: FolderMappingInfo[];
  
  @State() newFolderRequest: CreateNewFolderRequest = {
    FolderMappingId: -1,
    FolderName: "",
    ParentFolderId: state.currentItems.folder.folderId,
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
        ParentFolderId: state.currentItems.folder.folderId,
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

  private canChooseFolderProvider() {
    return this.folderMappings &&
      this.folderMappings.find(f => f.FolderMappingID == state.currentItems.folder.folderMappingId).IsDefault;
  }

  render() {
    return (
      <Host>
        <h2>{state.localization.AddFolder}</h2>
        <div class="form">
          {state.currentItems.folder.folderName.length > 0 &&[
            <label>{state.localization.FolderParent}</label>,
            <span>{state.currentItems.folder.folderName}</span>
          ]}
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
          {this.canChooseFolderProvider() &&
            [
              <label>{state.localization.Type}</label>,
              <select onChange={e => this.handleTypeChanged(e)}>
                {this.folderMappings?.map((folderMapping, index) => 
                  <option
                    value={folderMapping.FolderMappingID}
                    selected={index == 0}>
                    {folderMapping.MappingName}
                  </option>
                )}
              </select>
            ]
          }
          {this.canChooseFolderProvider() && this.folderMappings.find(m => m.FolderMappingID == this.newFolderRequest.FolderMappingId).IsDefault == false &&[
            <label>{state.localization.MappedPath}</label>,
            <input 
              type="text"
              value={this.newFolderRequest.MappedName}
              onInput={e => this.newFolderRequest = {
                ...this.newFolderRequest,
                MappedName: (e.target as HTMLInputElement).value,
              }}
            />
          ]}
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
