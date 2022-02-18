import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-files-pane',
  styleUrl: 'dnn-rm-files-pane.scss',
  shadow: true,
})
export class DnnRmFilesPane {

  render() {
    return (
      <Host>
        I am the files pane
      </Host>
    );
  }

}
