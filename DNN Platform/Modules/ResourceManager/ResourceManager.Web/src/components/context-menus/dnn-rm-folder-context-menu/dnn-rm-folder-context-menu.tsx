import { Component, Host, h, Prop } from '@stencil/core';

@Component({
  tag: 'dnn-rm-folder-context-menu',
  styleUrl: '../context-menu.scss',
  shadow: true,
})
export class DnnRmFolderContextMenu {
  
  /** The ID of the folder onto which the context menu was triggered on. */
  @Prop() clickedFolderId!: number;

  render() {
    return (
      <Host>
        <dnn-action-create-folder parentFolderId={this.clickedFolderId} />
      </Host>
    );
  }

}
