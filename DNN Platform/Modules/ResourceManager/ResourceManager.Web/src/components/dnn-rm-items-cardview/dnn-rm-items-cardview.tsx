import { Component, Host, h } from '@stencil/core';
import state from '../../store/store';

@Component({
  tag: 'dnn-rm-items-cardview',
  styleUrl: 'dnn-rm-items-cardview.scss',
  shadow: true,
})
export class DnnRmItemsCardview {

  render() {
    return (
      <Host>
        {state.currentItems &&
          <div class="container">
            {state.currentItems.items?.map(item =>
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
