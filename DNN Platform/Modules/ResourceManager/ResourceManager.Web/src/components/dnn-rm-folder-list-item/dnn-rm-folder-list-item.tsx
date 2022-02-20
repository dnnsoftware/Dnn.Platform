import { Component, Host, h, Prop, State } from '@stencil/core';
import { FolderTreeItem } from '../../services/InternalServicesClient';
import { ItemsClient } from "../../services/ItemsClient";
import state from "../../store/store";

@Component({
  tag: 'dnn-rm-folder-list-item',
  styleUrl: 'dnn-rm-folder-list-item.scss',
  shadow: true,
})
export class DnnRmFolderListItem {

  /** The basic information about the folder */
  @Prop() folder!: FolderTreeItem;

  /** If true, this node will be expanded on load. */
  @Prop() expanded = false;

  @State() folderIconUrl: string;

  private itemsClient: ItemsClient;

  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId)
  }

  componentWillLoad() {
    this.itemsClient.getFolderIconUrl(Number.parseInt(this.folder.data.key))
    .then(data => this.folderIconUrl = data)
    .catch(error => console.error(error));
  }

  render() {
    console.log(this.folder)
    return (
      <Host>
        {this.folder &&
          <dnn-treeview-item expanded={this.expanded}>
            <button title={this.folder.data.value}>
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
              <div slot="children">
                {this.folder.children && this.folder.children.length > 0 && this.folder.children.map(child =>
                  <dnn-rm-folder-list-item folder={child}></dnn-rm-folder-list-item>
                )}
              </div>
            }
          </dnn-treeview-item>
        }
      </Host>
    );
  }

}
