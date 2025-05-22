import { Component, Host, h } from '@stencil/core';
import { FolderTreeItem } from '../../services/InternalServicesClient';
import { ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';

@Component({
  tag: 'dnn-rm-left-pane',
  styleUrl: 'dnn-rm-left-pane.scss',
  shadow: true,
})
export class DnnRmLeftPane {

  private itemsClient: ItemsClient;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private async handleFolderClicked(e: CustomEvent<FolderTreeItem>) {
    try {
      state.selectedItems = [];
      state.currentItems = await this.itemsClient.getFolderContent(
        Number.parseInt(e.detail.data.key),
        0,
        state.pageSize,
        state.sortField,
        state.sortOrder);
      
    } catch (error) {
      alert(error);
    }
  }

  render() {
    return (
      <Host>
        <dnn-rm-folder-list
          onDnnRmFolderListFolderPicked={e => void this.handleFolderClicked(e)}
        />
      </Host>
    );
  }

}
