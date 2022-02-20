import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-left-pane',
  styleUrl: 'dnn-rm-left-pane.scss',
  shadow: true,
})
export class DnnRmLeftPane {

  render() {
    return (
      <Host>
        <dnn-rm-folder-list></dnn-rm-folder-list>
      </Host>
    );
  }

}
