import { Component, Event, EventEmitter, Host, h, Prop, Element } from '@stencil/core';
import { GetFolderContentResponse, Item } from '../../services/ItemsClient';
import { selectionUtilities } from '../../utilities/selection-utilities';

@Component({
  tag: 'dnn-rm-items-cardview',
  styleUrl: 'dnn-rm-items-cardview.scss',
  shadow: true,
})
export class DnnRmItemsCardview {

  /** The list of current items. */
  @Prop() currentItems!: GetFolderContentResponse;

  @Element() el: HTMLDnnRmItemsCardviewElement;

  /** Fires when a folder is double-clicked and emits the folder ID into the event.detail */
  @Event() dnnRmFolderDoubleClicked: EventEmitter<number>;

  /** Fires when a file is double-clicked and emits the file ID into the event.detail */
  @Event() dnnRmFileDoubleClicked: EventEmitter<string>;

  private contextMenu: HTMLDnnContextMenuElement;

  private handleDoubleClick(item: Item): void {
    if (item.isFolder) {
      this.dnnRmFolderDoubleClicked.emit(item.itemId);
    } else {
      this.dnnRmFileDoubleClicked.emit(item.path);
    }
  }

  render() {
    return (
      <Host>
        {this.currentItems &&
          <div class="container">
            {this.currentItems.items?.map(item =>
              <button
                class="card"
                onClick={() => selectionUtilities.toggleItemSelected(item)}
                onDblClick={() => this.handleDoubleClick(item)}
                onContextMenu={e => {
                  e.preventDefault();
                  this.contextMenu.open(e as PointerEvent).catch(console.error);
                }}
              >
                  <div class={selectionUtilities.isItemSelected(item) ? "radio selected" : "radio"}>
                    {selectionUtilities.isItemSelected(item) &&
                      <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M12 7c-2.76 0-5 2.24-5 5s2.24 5 5 5 5-2.24 5-5-2.24-5-5-5zm0-5C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z"/></svg>
                    }
                    {!selectionUtilities.isItemSelected(item) &&
                      <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z"/></svg>
                    }
                  </div>
                  <img
                    src={item.thumbnailAvailable ? item.thumbnailUrl : item.iconUrl}
                    alt={`${item.itemName} (ID: ${item.itemId})`}
                  />
                  <span class="item-name">
                    {item.itemName}
                  </span>
                  <dnn-context-menu
                    ref={el => this.contextMenu = el}
                    closeOnClick
                  >
                    {item.isFolder
                      ?
                        <dnn-rm-folder-context-menu item={item} />
                      :
                        <dnn-rm-file-context-menu item={item} />
                    }
                  </dnn-context-menu>
              </button>
            )}
          </div>
        }
      </Host>
    );
  }
}
