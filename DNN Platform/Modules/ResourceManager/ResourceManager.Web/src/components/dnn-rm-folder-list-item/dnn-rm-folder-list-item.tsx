import { Component, Host, h, Prop, State } from '@stencil/core';
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

  @State() folderIconUrl: string;

  private itemsClient: ItemsClient;
  private internalServicesClient: InternalServicesClient;
  
  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId);
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
  }
  
  componentWillLoad() {
    this.itemsClient.getFolderIconUrl(Number.parseInt(this.folder.data.key))
    .then(data => this.folderIconUrl = data)
    .catch(error => console.error(error));
  }
  
  private handleUserExpanded() {
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
    this.itemsClient.getFolderContent(Number.parseInt(this.folder.data.key))
    .then(data => state.currentItems = data)
    .catch(error => console.error(error));
  }

  render() {
    return (
      <Host>
        {this.folder &&
          <dnn-treeview-item
            expanded={this.expanded}
            onUserExpanded={() => this.handleUserExpanded()}>
            <button
              title={`${this.folder.data.value} (ID: ${this.folder.data.key})`}
              onClick={() => this.handleFolderClicked()}
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
        }
      </Host>
    );
  }
}
