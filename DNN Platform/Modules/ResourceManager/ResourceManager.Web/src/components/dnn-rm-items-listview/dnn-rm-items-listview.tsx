import { Component, Host, h } from '@stencil/core';
import state from "../../store/store";

@Component({
  tag: 'dnn-rm-items-listview',
  styleUrl: 'dnn-rm-items-listview.scss',
  shadow: true,
})
export class DnnRmItemsListview {

  render() {
    return (
      <Host>
        {state.currentItems &&
          <table>
            <thead>
              <tr>
                <td></td>
                <td>{state.localization?.Name}</td>
              </tr>
            </thead>
            <tbody>
              {state.currentItems.items?.map(item =>
                <tr>
                  <td><img src={item.iconUrl} /></td>
                  <td>{item.itemName}</td>
                </tr>  
              )}
            </tbody>
          </table>
        }
      </Host>
    );
  }

}
