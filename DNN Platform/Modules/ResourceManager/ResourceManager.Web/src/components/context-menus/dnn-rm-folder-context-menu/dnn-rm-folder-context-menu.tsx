import { Component, Host, h, Prop } from '@stencil/core';

@Component({
  tag: 'dnn-rm-folder-context-menu',
  styleUrl: '../context-menu.scss',
  shadow: true,
})
export class DnnRmFolderContextMenu {
  @Prop() folderId!: number;

  render() {
    return (
      <Host>
        <p>I am the folders context menu.</p>
        <p>Bla bla bla {this.folderId}</p>
      </Host>
    );
  }

}
