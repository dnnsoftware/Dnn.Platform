import { Component, Host, h } from '@stencil/core';
import state from '../../store/store';
@Component({
  tag: 'dnn-rm-status-bar',
  styleUrl: 'dnn-rm-status-bar.scss',
  shadow: true,
})
export class DnnRmStatusBar {

  render() {
    return (
      <Host>
        <div class="status-bar">
          Showing {state.currentItems?.items.length} of {state.currentItems?.totalCount} items
        </div>
      </Host>
    );
  }

}
