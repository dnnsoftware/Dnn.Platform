import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-files-pane',
  styleUrl: 'dnn-rm-files-pane.css',
  shadow: true,
})
export class DnnRmFilesPane {

  render() {
    return (
      <Host>
        <slot></slot>
      </Host>
    );
  }

}
