import { Component, Host, h } from '@stencil/core';
import state from '../../store/store';
@Component({
  tag: 'dnn-rm-files-pane',
  styleUrl: 'dnn-rm-files-pane.scss',
  shadow: true,
})
export class DnnRmFilesPane {

  render() {
    return (
      <Host>
        {state.layout == "list" &&
          <dnn-rm-items-listview></dnn-rm-items-listview>
        }
        {state.layout == "card" &&
          <dnn-rm-items-cardview></dnn-rm-items-cardview>
        }
      </Host>
    );
  }

}
