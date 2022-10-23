import { Component, Host, h, State, Listen, Event, EventEmitter } from '@stencil/core';
import { FolderTreeItem, InternalServicesClient } from "../../services/InternalServicesClient";
import { ItemsClient } from "../../services/ItemsClient";
import { GetFolderContentResponse } from "../../services/ItemsClient";
import state from "../../store/store";

@Component({
  tag: 'dnn-rm-folder-list',
  styleUrl: 'dnn-rm-folder-list.scss',
  shadow: true,
})
export class DnnRmFolderList {

  /** Fires when a folder is picked. */
  @Event() dnnRmFolderListFolderPicked: EventEmitter<FolderTreeItem>;

  @State() folderContents: GetFolderContentResponse;
  @State() selectedFolder: FolderTreeItem;
  
  private internalServicesClient: InternalServicesClient;
  private itemsClient: ItemsClient;

  constructor(){
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  @Listen("dnnRmFoldersChanged", {target: "document"})
  handleFoldersChanged(){
    this.getFolders();
  }

  componentWillLoad() {
    this.getFolders()
    .then(() => {
      console.log(state.rootFolders);
      this.itemsClient.getFolderContent(
        state.settings.HomeFolderId,
        0,
        state.pageSize,
        state.sortField)
      .then(data => state.currentItems = data)
      .catch(error => console.error(error));
    })
    .catch(error => {
      console.error(error);
      if (error.Message){
        alert(error.Message);
      }
    });
      
  }

  private getFolders() {
    return new Promise((resolve, reject) => {
      this.internalServicesClient.getFolders(state.settings.HomeFolderId)
      .then(data => {
        state.rootFolders = data;
        resolve(data);
      })
      .catch(reason => reject(reason));
    });
  }

  private handleFolderPicked(e: CustomEvent<FolderTreeItem>): void {
    this.selectedFolder = e.detail;
    this.dnnRmFolderListFolderPicked.emit(e.detail)
  }

  private handleRootClicked(){
    const item: FolderTreeItem = {
      data: {
        hasChildren: false,
        key: state.settings.HomeFolderId.toString(),
        selectable: true,
        value: state.settings.HomeFolderName,
      },
    };
    this.selectedFolder = item;
    this.dnnRmFolderListFolderPicked.emit(item);
  }

  render() {
    return (
      <Host>
        <button
          onClick={() => this.handleRootClicked()}
        >
          <strong>{state.settings.HomeFolderName}</strong>
        </button>
        {state.rootFolders && state.rootFolders.Tree.children.map(item =>
            <dnn-rm-folder-list-item
              folder={item}
              parentFolderId={Number.parseInt(state.rootFolders.Tree.data.key)}
              expanded
              onDnnRmFolderListItemClicked={e => this.handleFolderPicked(e)}
              selectedFolder={this.selectedFolder}
            >
            </dnn-rm-folder-list-item>
        )}
      </Host>
    );
  }
}
