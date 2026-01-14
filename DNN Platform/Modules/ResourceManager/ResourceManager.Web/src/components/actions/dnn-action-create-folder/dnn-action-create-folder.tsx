import { Component, Host, h, Prop } from '@stencil/core';
import { ItemsClient } from '../../../services/ItemsClient';
import state from "../../../store/store";

@Component({
  tag: 'dnn-action-create-folder',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionCreateFolder {

  /** The ID of the parent folder */
  @Prop() parentFolderId: number;

  private readonly itemsClient: ItemsClient;

  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private async handleClick() {
    if (this.parentFolderId != null){
      try {
        state.currentItems = await this.itemsClient.getFolderContent(this.parentFolderId, 0, 0);
      } catch (error) {
        alert(error);
      }
    }
    await this.showModal();
  }

  private async showModal(){
    const modal = document.createElement("dnn-modal");
    modal.preventBackdropDismiss = true;
    modal.hideCloseButton = true;
    const editor = document.createElement("dnn-rm-create-folder");
    modal.appendChild(editor);
    document.body.appendChild(modal);
    await modal.show();
  }

  render() {
    return (
      <Host>
        <button onClick={() => void this.handleClick()}>
          <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0V0z" fill="none"/><path d="M20 6h-8l-2-2H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V8c0-1.11-.89-2-2-2zm-1 8h-3v3h-2v-3h-3v-2h3V9h2v3h3v2z"/></svg>
          <span>{state.localization.AddFolder}</span>
        </button>
      </Host>
    );
  }
}
