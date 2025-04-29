import { Component, Host, h, Prop } from "@stencil/core";
import { Item } from "../../../services/ItemsClient";
import state from "../../../store/store";

@Component({
  tag: "dnn-rm-folder-context-menu",
  styleUrl: "../context-menu.scss",
  shadow: true,
})
export class DnnRmFolderContextMenu {
  /** The item that triggered this menu. */
  @Prop() item!: Item;

  render() {
    return (
      <Host>
        {state.currentItems?.hasAddFoldersPermission && (
          <dnn-action-create-folder parentFolderId={this.item.itemId} />
        )}
        {state.currentItems?.hasManagePermission && (
          <dnn-action-edit-item item={this.item} />
        )}
        {state.currentItems?.hasDeletePermission &&
          this.item.itemId != state.settings.HomeFolderId && [
            <dnn-action-move-items items={[this.item]} />,
            <dnn-action-delete-items items={[this.item]} />,
          ]}
        {state.currentItems.hasDeletePermission &&
          this.item.unlinkAllowedStatus != "false" && (
            <dnn-action-unlink-items items={[this.item]} />
          )}
      </Host>
    );
  }
}
