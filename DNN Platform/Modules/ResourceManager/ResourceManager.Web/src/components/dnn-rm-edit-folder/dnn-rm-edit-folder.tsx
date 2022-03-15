import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'dnn-rm-edit-folder',
  styleUrl: 'dnn-rm-edit-folder.scss',
  shadow: true,
})
export class DnnRmEditFolder {

  render() {
    return (
      <Host>
        <p>Folder editor goes here</p>
      </Host>
    );
  }

}
