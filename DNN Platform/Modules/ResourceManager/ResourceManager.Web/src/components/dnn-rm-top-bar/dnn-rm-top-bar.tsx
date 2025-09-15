import { Component, Host, h, } from '@stencil/core';
import state from '../../store/store';
import { ItemsClient } from "../../services/ItemsClient";
@Component({
  tag: 'dnn-rm-top-bar',
  styleUrl: 'dnn-rm-top-bar.scss',
  shadow: true,
})
export class DnnRmTopBar {

  private itemsClient: ItemsClient;

  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private async handleSearchChanged(e: CustomEvent<string>) {
    if (e.detail != ""){
      try {
        state.itemsSearchTerm = e.detail;
        const data = await this.itemsClient.search(
          state.currentItems.folder.folderId, e.detail,
          0,
          state.pageSize,
          state.sortField);
        state.currentItems = {
          ...state.currentItems,
          totalCount: data.totalCount,
          items: data.items,
        }
        state.lastSearchRequestedPage = 1;
      } catch (error) {
        alert(error);
      }
    }
    else
    {
      const data = await this.itemsClient.getFolderContent(
        state.currentItems.folder.folderId,
        0,
        state.pageSize,
        state.sortField,
        state.sortOrder);
      state.lastSearchRequestedPage = 1;
      state.itemsSearchTerm = undefined;
      state.currentItems = data;
    }
  }

  render() {
    return (
      <Host>
        <dnn-searchbox onQueryChanged={e => void this.handleSearchChanged(e)}></dnn-searchbox>
      </Host>
    );
  }
}
