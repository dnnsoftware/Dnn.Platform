import { Component, Host, h, Prop } from "@stencil/core";
import { Item } from "../../../services/ItemsClient";
import state from "../../../store/store";

@Component({
  tag: "dnn-rm-file-context-menu",
  styleUrl: "../context-menu.scss",
  shadow: true,
})
export class DnnRmFileContextMenu {
  /** The item that triggered this menu. */
  @Prop() item!: Item;

  render() {
    return (
      <Host>
        {state.currentItems?.hasAddFilesPermission &&
          state.currentItems.hasDeletePermission && (
            <dnn-action-edit-item item={this.item} />
          )}
        {state.currentItems?.hasDeletePermission && [
          <dnn-action-move-items items={[this.item]} />,
          <dnn-action-delete-items items={[this.item]} />,
          <dnn-action-open-file item={this.item} />,
          <dnn-action-download-item item={this.item} />,
        ]}
        {location.protocol == "https:" && (
          <dnn-action-copy-url items={[this.item]} />
        )}
      </Host>
    );
  }
}
