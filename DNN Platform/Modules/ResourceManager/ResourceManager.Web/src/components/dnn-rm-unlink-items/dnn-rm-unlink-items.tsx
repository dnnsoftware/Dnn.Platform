import { Component, Element, Event, EventEmitter, Host, h, Prop, State } from '@stencil/core';
import { Item, ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';

@Component({
  tag: 'dnn-rm-unlink-items',
  styleUrl: 'dnn-rm-unlink-items.scss',
  shadow: true,
})
export class DnnRmUnlinkItems {

  /** The list of items to delete. */
  @Prop() items!: Item[];

  /**
  * Fires when there is a possibility that some folders have changed.
  * Can be used to force parts of the UI to refresh.
  */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;
  
  @Element() el: HTMLDnnRmUnlinkItemsElement;
  
  @State() unlinking: boolean;
  @State() unlinkedCount: number = 0;
  @State() currentItemName: string;
  
  private itemsClient: ItemsClient;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private async closeModal() {
    const modal = this.el.parentElement as HTMLDnnModalElement;
    await modal.hide();
    setTimeout(() => {
      document.body.removeChild(modal);
    }, 300);
  }

  private async handleUnlink() {
    this.unlinking = true;
    for (const item of this.items) {
      try {
        await this.itemsClient.deleteFolder({
          FolderId: item.itemId,
          UnlinkAllowedStatus: true,
        })
        await this.handleItemUnlinked(item);
      } catch (error) {
        alert(error);
      }
    }
  }
  
  private async handleItemUnlinked(item: Item) {
    this.unlinkedCount++;
    this.currentItemName = item.itemName;
    if (this.unlinkedCount == this.items.length) {
      this.unlinking = false;
      this.dnnRmFoldersChanged.emit();
      await this.closeModal();
    }
  }

  render() {
    return (
      <Host>
        <h2>{state.localization?.UnlinkFolders}</h2>
        <p>{state.localization?.ConfirmUnlinkMessage}</p>
        <ul>
          {this.items.map(item => (
            <li>{item.itemName}</li>
          ))}
        </ul>
        {this.unlinking &&
          <div>
            <p>{state.localization?.UnlinkFolders} {this.unlinkedCount+1}/{state.selectedItems.length} ({this.currentItemName})</p>
            <dnn-rm-progress-bar
              max={state.selectedItems.length}
              value={this.unlinkedCount}
            />
          </div>
        }
        <div class="controls">
          <dnn-button
            appearance="primary"
            reversed
            disabled={this.unlinking}
            onClick={() => void this.closeModal()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            appearance="primary"
            onClick={() => void this.handleUnlink()}
            disabled={this.unlinking}
          >
            {state.localization?.Unlink}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
