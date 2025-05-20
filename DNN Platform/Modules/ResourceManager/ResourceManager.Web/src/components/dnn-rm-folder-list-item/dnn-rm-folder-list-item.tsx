import { Component, Host, h, Event, EventEmitter, Prop, State, Element, Listen } from '@stencil/core';
import { InternalServicesClient, FolderTreeItem } from '../../services/InternalServicesClient';
import { ItemsClient } from "../../services/ItemsClient";
import state from "../../store/store";

@Component({
  tag: 'dnn-rm-folder-list-item',
  styleUrl: 'dnn-rm-folder-list-item.scss',
  shadow: true,
})
export class DnnRmFolderListItem {

  /** The basic information about the folder */
  @Prop({mutable: true}) folder!: FolderTreeItem;

  /** If true, this node will be expanded on load. */
  @Prop({mutable: true}) expanded = false;
  
  /** The ID of the parent folder. */
  @Prop() parentFolderId!: number;

  /** Indicates if this item is the currently selected one.*/
  @Prop() selectedFolder: FolderTreeItem;

  /** Fires when a context menu is opened for this item. Emits the folder ID. */
  @Event() dnnRmcontextMenuOpened: EventEmitter<number>;

  @Listen("dnnRmcontextMenuOpened", {target: "body"})
  handleDnnRmContextMenuOpened(e: CustomEvent<number>){
    if (Number.parseInt(this.folder.data.key) != e.detail){
      this.dismissContextMenu();
    }
  }

  @Listen("dnnRmFolderDoubleClicked", {target: "document"})
  handleFolderDoubleClicked(e: CustomEvent<number>) {
    if (e.detail == Number.parseInt(this.folder.data.key)) {
      this.dnnRmFolderListItemClicked.emit(this.folder);
      void this.handleUserExpanded();
      this.expanded = true;
    }
  }

  @State() folderIconUrl: string;

  @Element() el!: HTMLDnnRmFolderListItemElement;

  /** Fires when a folder is clicked. */
  @Event() dnnRmFolderListItemClicked: EventEmitter<FolderTreeItem>;

  private itemsClient: ItemsClient;
  private internalServicesClient: InternalServicesClient;
  
  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId);
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
  }
  
  async componentWillLoad() {
    try {
      this.folderIconUrl = await this.itemsClient.getFolderIconUrl(Number.parseInt(this.folder.data.key));
    } catch (error) {
      console.error(error);
    }
    document.addEventListener("click", void this.dismissContextMenu.bind(this));
  }

  disconnectedCallback(){
    document.removeEventListener("click", void this.dismissContextMenu.bind(this));
  }
  
  private async handleUserExpanded() {
    const children = Array.from(this.el.shadowRoot.querySelectorAll('dnn-rm-folder-list-item'));
    children.forEach(element => {
      element.shadowRoot.querySelectorAll("dnn-treeview-item")
        .forEach(t => t.removeAttribute("expanded"));
    });
    try {
      const data = await this.internalServicesClient.getFolderDescendants(this.folder.data.key);
      this.folder = {
        ...this.folder,
        children: data.Items.map(item => {
          return {
            data: {
              hasChildren : item.hasChildren,
              key: item.key,
              selectable: item.selectable,
              value: item.value
            },
          };
        }),
      }; 
    } catch (error) {
      alert(error);
    }
  };

  private async handleContextMenu(e: MouseEvent) {
    e.preventDefault();
    this.dismissContextMenu();

    try {
      const item = await this.itemsClient.getFolderItem(Number.parseInt(this.folder.data.key));
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
      this.dnnRmcontextMenuOpened.emit(Number.parseInt(this.folder.data.key));
    } catch (error) {
      alert(error);
    }
  }

  private dismissContextMenu() {
    const existingMenus = this.el.shadowRoot.querySelectorAll("dnn-collapsible");
    existingMenus?.forEach(contextMenu => this.el.shadowRoot.removeChild(contextMenu));
  }

  private getItemClasses(): string {
    if (this.selectedFolder?.data?.key == this.folder.data.key) {
      return "selected";
    }

    return "";
  }

  render() {
    return (
      <Host class={this.getItemClasses()}>
        <dnn-treeview-item
          expanded={this.expanded}
          onUserExpanded={() => void this.handleUserExpanded()}
        >
          <button
            title={`${this.folder.data.value} (ID: ${this.folder.data.key})`}
            onClick={() => this.dnnRmFolderListItemClicked.emit(this.folder)}
            onContextMenu={e => void this.handleContextMenu(e)}
          >
            {this.folderIconUrl != null && this.folderIconUrl.length > 0
            ?
              <img src={this.folderIconUrl} alt={this.folder.data.value} />
            :
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"></path><path d="M10 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z"></path></svg>
            }
            <span class="item-name">
              {this.folder.data.value}
            </span>
          </button>
          {this.folder.data.hasChildren &&
            [
              <div slot="children">
              </div>
            ,
              this.folder.children && this.folder.children.length > 0 && this.folder.children.map(child =>
              <dnn-rm-folder-list-item
                slot="children"
                parentFolderId={Number.parseInt(this.folder.data.key)}
                folder={child}
                selectedFolder={this.selectedFolder}>
              </dnn-rm-folder-list-item>
              )
            ]}
        </dnn-treeview-item>
      </Host>
    );
  }
}
