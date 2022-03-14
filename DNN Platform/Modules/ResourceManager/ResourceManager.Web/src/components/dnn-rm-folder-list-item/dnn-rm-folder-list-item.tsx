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

  /** Fires when a context menu is opened for this item. Emits the folder ID. */
  @Event() dnnRmcontextMenuOpened: EventEmitter<number>;

  @Listen("dnnRmcontextMenuOpened", {target: "body"})
  handleDnnRmContextMenuOpened(e: CustomEvent<number>){
    if (Number.parseInt(this.folder.data.key) != e.detail){
      this.dismissContextMenu();
    }
  }

  private dismissContextMenu() {
    if (this.contextMenu && this.contextMenu.expanded){
      this.contextMenu.expanded = false;
      requestAnimationFrame(() => {
        this.contextMenu.style.display = "none";
      });
    }
  }

  @State() folderIconUrl: string;

  @Element() el!: HTMLDnnRmFolderListItemElement;

  private itemsClient: ItemsClient;
  private internalServicesClient: InternalServicesClient;
  private contextMenu: HTMLDnnCollapsibleElement;
  
  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId);
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
  }
  
  componentWillLoad() {
    this.itemsClient.getFolderIconUrl(Number.parseInt(this.folder.data.key))
    .then(data => this.folderIconUrl = data)
    .catch(error => console.error(error));
    document.addEventListener("click", this.dismissContextMenu.bind(this));
  }

  disconnectedCallback(){
    document.removeEventListener("click", this.dismissContextMenu.bind(this));
  }
  
  private handleUserExpanded() {
    const children = Array.from(this.el.shadowRoot.querySelectorAll('dnn-rm-folder-list-item'));
    children.forEach(element => {
      element.shadowRoot.querySelectorAll("dnn-treeview-item")
        .forEach(t => t.removeAttribute("expanded"));
    });
    this.internalServicesClient.getFolderDescendants(this.folder.data.key)
    .then(data => {
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
    })
    .catch(error => console.error(error));
  };

  private handleFolderClicked(): void {
    this.itemsClient.getFolderContent(
      Number.parseInt(this.folder.data.key),
      0,
      state.pageSize,
      state.sortField)
    .then(data => state.currentItems = data)
    .catch(error => console.error(error));
  }

  private handleContextMenu(e: MouseEvent): void {
    e.preventDefault();
    this.contextMenu.style.display = "block";
    this.contextMenu.style.left = `${e.pageX}px`;
    this.contextMenu.style.top = `${e.pageY}px`;
    this.contextMenu.expanded = true;
    this.dnnRmcontextMenuOpened.emit(Number.parseInt(this.folder.data.key));
  }

  render() {
    return (
      <Host>
        <dnn-treeview-item
          expanded={this.expanded}
          onUserExpanded={() => this.handleUserExpanded()}
        >
          <button
            title={`${this.folder.data.value} (ID: ${this.folder.data.key})`}
            onClick={() => this.handleFolderClicked()}
            onContextMenu={e => this.handleContextMenu(e)}
          >
            {this.folderIconUrl
            ?
              <img src={this.folderIconUrl} alt={this.folder.data.value} />
            :
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"></path><path d="M10 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z"></path></svg>
            }
            <span>
              {this.folder.data.value}
            </span>
          </button>
          {this.folder.data.hasChildren &&
            [
              <div slot="children">
              </div>
            ,
              this.folder.children && this.folder.children.length > 0 && this.folder.children.map(child =>
              <dnn-rm-folder-list-item slot="children" folder={child}></dnn-rm-folder-list-item>
              )
            ]}
        </dnn-treeview-item>
        <dnn-collapsible ref={el => this.contextMenu = el}>
          <dnn-rm-folder-context-menu clickedFolderId={Number.parseInt(this.folder.data.key)} />
        </dnn-collapsible>
      </Host>
    );
  }
}
