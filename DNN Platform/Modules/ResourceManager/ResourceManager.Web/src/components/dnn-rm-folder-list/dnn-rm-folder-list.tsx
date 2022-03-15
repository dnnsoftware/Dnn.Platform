import { Component, Host, h, State, Listen } from '@stencil/core';
import { InternalServicesClient } from "../../services/InternalServicesClient";
import { ItemsClient } from "../../services/ItemsClient";
import { GetFolderContentResponse } from "../../services/ItemsClient";
import state from "../../store/store";

@Component({
  tag: 'dnn-rm-folder-list',
  styleUrl: 'dnn-rm-folder-list.scss',
  shadow: true,
})
export class DnnRmFolderList {

  @State() folderContents: GetFolderContentResponse;
  
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

  private getFolders() {
    return new Promise((resolve, reject) => {
      this.internalServicesClient.getFolders()
      .then(data => {
        state.rootFolders = data;
        resolve(data);
      })
      .catch(reason => reject(reason));
    });
  }

  componentWillLoad() {
    this.getFolders()
    .then(() => {
      this.itemsClient.getFolderContent(
        Number.parseInt(state.rootFolders.Tree.children[0].data.key),
        0,
        state.pageSize,
        state.sortField)
      .then(data => state.currentItems = data)
      .catch(error => console.error(error));
    })
    .catch(error => alert(error.Message));
  }

  render() {
    return (
      <Host>
        {state.rootFolders && state.rootFolders.Tree.children.map(item =>
            <dnn-rm-folder-list-item
              folder={item}
              parentFolderId={Number.parseInt(state.rootFolders.Tree.data.key)}
              expanded
            >
            </dnn-rm-folder-list-item>
        )}
      </Host>
    );
  }

}
