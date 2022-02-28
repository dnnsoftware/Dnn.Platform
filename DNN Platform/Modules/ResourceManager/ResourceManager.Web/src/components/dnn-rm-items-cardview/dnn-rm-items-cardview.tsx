import { Component, Host, h, Prop } from '@stencil/core';
import { GetFolderContentResponse } from '../../services/ItemsClient';

@Component({
  tag: 'dnn-rm-items-cardview',
  styleUrl: 'dnn-rm-items-cardview.scss',
  shadow: true,
})
export class DnnRmItemsCardview {

  @Prop() currentItems!: GetFolderContentResponse;

  render() {
    return (
      <Host>
        {this.currentItems &&
          <div class="container">
            {this.currentItems.items?.map(item =>
              <button class="card">
                  <img
                    src={item.thumbnailAvailable ? item.thumbnailUrl : item.iconUrl}
                    alt={`${item.itemName} (ID: ${item.itemId})`}
                  />
                  <span>
                    {item.itemName}
                  </span>
              </button>
            )}
          </div>
        }
      </Host>
    );
  }
}
