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
        <dnn-action-create-folder parentFolderId={this.folderId}></dnn-action-create-folder>
      </Host>
    );
  }

}
