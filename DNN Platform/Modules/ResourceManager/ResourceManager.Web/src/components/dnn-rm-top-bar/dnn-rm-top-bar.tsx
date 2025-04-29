import { Component, Host, h } from "@stencil/core";
import state from "../../store/store";
import { ItemsClient } from "../../services/ItemsClient";
@Component({
  tag: "dnn-rm-top-bar",
  styleUrl: "dnn-rm-top-bar.scss",
  shadow: true,
})
export class DnnRmTopBar {
  private itemsClient: ItemsClient;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private handleSearchChanged(e: CustomEvent<any>): void {
    if (e.detail != "") {
      state.itemsSearchTerm = e.detail;
      this.itemsClient
        .search(
          state.currentItems.folder.folderId,
          e.detail,
          0,
          state.pageSize,
          state.sortField,
        )
        .then((data) => {
          state.currentItems = {
            ...state.currentItems,
            totalCount: data.totalCount,
            items: data.items,
          };
          state.lastSearchRequestedPage = 1;
        })
        .catch((reason) => console.error(reason));
    } else {
      this.itemsClient
        .getFolderContent(
          state.currentItems.folder.folderId,
          0,
          state.pageSize,
          state.sortField,
        )
        .then((data) => {
          state.lastSearchRequestedPage = 1;
          state.itemsSearchTerm = undefined;
          state.currentItems = data;
        });
    }
  }

  render() {
    return (
      <Host>
        <dnn-searchbox
          onQueryChanged={(e) => this.handleSearchChanged(e)}
        ></dnn-searchbox>
      </Host>
    );
  }
}
