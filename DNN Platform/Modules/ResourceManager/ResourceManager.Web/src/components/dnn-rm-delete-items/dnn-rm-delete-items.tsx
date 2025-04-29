import {
  Component,
  Element,
  Event,
  EventEmitter,
  Host,
  h,
  Prop,
  State,
} from "@stencil/core";
import { Item, ItemsClient } from "../../services/ItemsClient";
import state from "../../store/store";

@Component({
  tag: "dnn-rm-delete-items",
  styleUrl: "dnn-rm-delete-items.scss",
  shadow: true,
})
export class DnnRmDeleteItems {
  /** The list of items to delete. */
  @Prop() items!: Item[];

  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;

  @Element() el: HTMLDnnRmDeleteItemsElement;

  @State() deleting: boolean;
  @State() deletedCount: number = 0;
  @State() currentItemName: string;

  private itemsClient: ItemsClient;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private closeModal(): void {
    const modal = this.el.parentElement as HTMLDnnModalElement;
    modal.hide().then(() => {
      setTimeout(() => {
        document.body.removeChild(modal);
      }, 300);
    });
  }

  private handleDelete(): void {
    this.deleting = true;
    this.items.forEach((item) => {
      if (item.isFolder) {
        this.itemsClient
          .deleteFolder({
            FolderId: item.itemId,
            UnlinkAllowedStatus: false,
          })
          .then(() => {
            this.handleItemDeleted(item);
          })
          .catch((reason) => alert(reason));
      } else {
        this.itemsClient
          .deleteFile({
            FileId: item.itemId,
          })
          .then(() => {
            this.handleItemDeleted(item);
          })
          .catch((reason) => alert(reason));
      }
    });
  }

  handleItemDeleted(item: Item) {
    this.deletedCount++;
    this.currentItemName = item.itemName;
    if (this.deletedCount == this.items.length) {
      this.deleting = false;
      this.dnnRmFoldersChanged.emit();
      this.closeModal();
    }
  }

  render() {
    return (
      <Host>
        <h2>{state.localization?.DeleteItems}</h2>
        <p>{state.localization?.ConfirmDeleteMessage}</p>
        <ul>
          {this.items.map((item) => (
            <li>{item.itemName}</li>
          ))}
        </ul>
        {this.deleting && (
          <div>
            <p>
              {state.localization?.DeletingItems} {this.deletedCount + 1}/
              {state.selectedItems.length} ({this.currentItemName})
            </p>
            <dnn-rm-progress-bar
              max={state.selectedItems.length}
              value={this.deletedCount}
            />
          </div>
        )}
        <div class="controls">
          <dnn-button
            appearance="primary"
            reversed
            disabled={this.deleting}
            onClick={() => this.closeModal()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            appearance="primary"
            onClick={() => this.handleDelete()}
            disabled={this.deleting}
          >
            {state.localization?.Delete}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
