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
import { FolderTreeItem } from "../../services/InternalServicesClient";
import { Item, ItemsClient } from "../../services/ItemsClient";
import state from "../../store/store";
// import { ItemsClient } from '../../services/ItemsClient';

@Component({
  tag: "dnn-rm-move-items",
  styleUrl: "dnn-rm-move-items.scss",
  shadow: true,
})
export class DnnRmMoveItems {
  /** The list of items to delete. */
  @Prop() items!: Item[];

  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;

  @State() selectedFolder: FolderTreeItem;

  @Element() el: HTMLDnnRmMoveItemsElement;

  @State() moving: boolean;
  @State() movedCount: number = 0;
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

  private handleMove(): void {
    this.moving = true;
    this.items.forEach((item) => {
      if (item.isFolder) {
        this.itemsClient
          .moveFolder({
            SourceFolderId: item.itemId,
            DestinationFolderId: Number.parseInt(this.selectedFolder.data.key),
          })
          .then(() => {
            this.handleItemMoved(item);
          })
          .catch((reason) => alert(reason));
      } else {
        this.itemsClient
          .moveFile({
            SourceFileId: item.itemId,
            DestinationFolderId: Number.parseInt(this.selectedFolder.data.key),
          })
          .then(() => {
            this.handleItemMoved(item);
          })
          .catch((reason) => alert(reason));
      }
    });
  }

  handleItemMoved(item: Item) {
    this.movedCount++;
    this.currentItemName = item.itemName;
    if (this.movedCount == this.items.length) {
      this.moving = false;
      this.dnnRmFoldersChanged.emit();
      this.closeModal();
    }
  }

  private getMoveButtonText() {
    return this.selectedFolder == undefined
      ? state.localization?.NoFolderSelected
      : state.localization?.Move;
  }

  render() {
    return (
      <Host>
        <h2>{state.localization?.MoveItems}</h2>
        <p>{state.localization?.SelectDestinationFolder}</p>
        <dnn-rm-folder-list
          onDnnRmFolderListFolderPicked={(e) =>
            (this.selectedFolder = e.detail)
          }
        />
        {this.moving && (
          <div>
            <p>
              {state.localization?.MovingItems} {this.movedCount + 1}/
              {state.selectedItems.length} ({this.currentItemName})
            </p>
            <dnn-rm-progress-bar
              max={state.selectedItems.length}
              value={this.movedCount}
            />
          </div>
        )}
        <div class="controls">
          <dnn-button
            appearance="primary"
            reversed
            disabled={this.moving}
            onClick={() => this.closeModal()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            appearance="primary"
            onConfirmed={() => this.handleMove()}
            confirm
            confirmMessage={state.localization?.ConfirmMoveMessage}
            confirmYesText={state.localization?.Yes}
            confirmNoText={state.localization?.No}
            disabled={this.moving}
          >
            {this.getMoveButtonText()}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
