import { Component, Element, Host, h, State, Listen, Event, EventEmitter } from '@stencil/core';
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
  /** Fires when a context menu is opened for this item. Emits the folder ID. */
  @Event() dnnRmcontextMenuOpened: EventEmitter<number>;

  @State() folderContents: GetFolderContentResponse;
  @State() selectedFolder: FolderTreeItem;

  @Element() el!: HTMLDnnRmFolderListElement;
  
  private internalServicesClient: InternalServicesClient;
  private itemsClient: ItemsClient;

  constructor(){
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  @Listen("dnnRmFoldersChanged", {target: "document"})
  handleFoldersChanged(){
    void this.getFolders();
  }

  @Listen("dnnRmcontextMenuOpened", {target: "body"})
  handleDnnRmContextMenuOpened(e: CustomEvent<number>){
    if (state.settings.HomeFolderId != e.detail){
      this.dismissContextMenu();
    }
  }

  async componentWillLoad() {
    try {
      await this.getFolders();
      state.currentItems = await this.itemsClient.getFolderContent(
        state.settings.HomeFolderId,
        0,
        state.pageSize,
        state.sortField,
        state.sortOrder);  
      
    } catch (error) {
      alert(error);
    }
  }

  private dismissContextMenu() {
    const existingMenus = this.el.shadowRoot.querySelectorAll("dnn-collapsible");
    existingMenus?.forEach(contextMenu => this.el.shadowRoot.removeChild(contextMenu));
  }

  private async getFolders() {
    try {
      const data = await this.internalServicesClient.getFolders(state.settings.HomeFolderId)
      state.rootFolders = data;
    } catch (error) {
      alert(error);
    }
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

  private async handleContextMenu(e: MouseEvent) {
    e.preventDefault();
    try {
      this.dismissContextMenu();
  
      const item = await this.itemsClient.getFolderItem(state.settings.HomeFolderId)
      const collapsible = document.createElement("dnn-collapsible");
      const folderContextMenu = document.createElement("dnn-rm-folder-context-menu");
      collapsible.appendChild(folderContextMenu);
      folderContextMenu.item = item;
      const parentPosition = this.el.getBoundingClientRect();
      collapsible.style.left = `${e.clientX - parentPosition.left}px`;
      collapsible.style.top = `${e.clientY - parentPosition.top}px`;
      collapsible.style.display = "block";
      this.el.shadowRoot.appendChild(collapsible);
      setTimeout(() => {
        collapsible.expanded = true;
      }, 100);
      this.dnnRmcontextMenuOpened.emit(state.settings.HomeFolderId);
    } catch (error) {
      console.error(error);
    }
  }

  render() {
    return (
      <Host>
        <button
          onClick={() => this.handleRootClicked()}
          onContextMenu={e => void this.handleContextMenu(e)}
        >
          <strong>{state.settings.HomeFolderName}</strong>
        </button>
        {state.rootFolders && state.rootFolders.Tree.children.map(item =>
            <dnn-rm-folder-list-item
              folder={item}
              parentFolderId={Number.parseInt(state.rootFolders.Tree.data.key)}
              onDnnRmFolderListItemClicked={e => this.handleFolderPicked(e)}
              selectedFolder={this.selectedFolder}
            >
            </dnn-rm-folder-list-item>
        )}
      </Host>
    );
  }
}
