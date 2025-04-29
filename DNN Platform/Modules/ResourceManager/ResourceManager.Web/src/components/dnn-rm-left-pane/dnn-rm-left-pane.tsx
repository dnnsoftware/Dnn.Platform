import { Component, Host, h } from "@stencil/core";
import { FolderTreeItem } from "../../services/InternalServicesClient";
import { ItemsClient } from "../../services/ItemsClient";
import state from "../../store/store";

@Component({
  tag: "dnn-rm-left-pane",
  styleUrl: "dnn-rm-left-pane.scss",
  shadow: true,
})
export class DnnRmLeftPane {
  private itemsClient: ItemsClient;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private handleFolderClicked(e: CustomEvent<FolderTreeItem>): void {
    state.selectedItems = [];
    this.itemsClient
      .getFolderContent(
        Number.parseInt(e.detail.data.key),
        0,
        state.pageSize,
        state.sortField,
      )
      .then((data) => (state.currentItems = data))
      .catch((error) => console.error(error));
  }

  render() {
    return (
      <Host>
        <dnn-rm-folder-list
          onDnnRmFolderListFolderPicked={(e) => this.handleFolderClicked(e)}
        />
      </Host>
    );
  }
}
