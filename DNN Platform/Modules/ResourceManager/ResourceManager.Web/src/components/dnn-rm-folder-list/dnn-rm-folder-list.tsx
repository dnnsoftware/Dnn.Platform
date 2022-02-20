import { Component, Host, h, State } from '@stencil/core';
import { InternalServicesClient } from "../../services/InternalServicesClient";
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

  constructor(){
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
  }

  componentWillLoad() {
    this.internalServicesClient.getFolders()
    .then(data =>
    {
      state.rootFolders = data;
    })
    .catch(error => alert(error.Message));
  }


  render() {
    return (
      <Host>
        {state.rootFolders && state.rootFolders.Tree.children.map(item =>
            <dnn-rm-folder-list-item
              folder={item}
              expanded
            >
            </dnn-rm-folder-list-item>
        )}
      </Host>
    );
  }

}
