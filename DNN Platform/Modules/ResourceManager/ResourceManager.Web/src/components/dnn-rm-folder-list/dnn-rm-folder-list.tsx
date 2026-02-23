import { Component, Element, Host, h, State, Listen, Event, EventEmitter } from '@stencil/core';
import { FolderTreeItem, InternalServicesClient } from "../../services/InternalServicesClient";
import { Item, ItemsClient } from "../../services/ItemsClient";
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
  @State() rootItem: Item;

  @Element() el!: HTMLDnnRmFolderListElement;

  private internalServicesClient: InternalServicesClient;
  private itemsClient: ItemsClient;
  private rootItemContextMenu: HTMLDnnContextMenuElement;

  constructor(){
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  @Listen("dnnRmFoldersChanged", {target: "document"})
  handleFoldersChanged(){
    void this.getFolders();
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
        this.rootItem = await this.itemsClient.getFolderItem(state.settings.HomeFolderId)
    } catch (error) {
      alert(error);
    }
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

  render() {
    return (
      <Host>
        <button
          onClick={() => this.handleRootClicked()}
          onContextMenu={e => {
            e.preventDefault();
            this.rootItemContextMenu.open(e as PointerEvent).catch(console.error);
          }}
        >
          <strong>{state.settings.HomeFolderName}</strong>
          <dnn-context-menu
            ref={el => this.rootItemContextMenu = el}
            closeOnClick
          >
            {this.rootItem && (
              <dnn-rm-folder-context-menu item={this.rootItem} />
            )}
          </dnn-context-menu>
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
