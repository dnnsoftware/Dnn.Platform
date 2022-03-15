import { Component, Host, h, Prop } from '@stencil/core';
import { Item, ItemsClient } from '../../../services/ItemsClient';
import state from "../../../store/store";

@Component({
  tag: 'dnn-action-edit-item',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionEditItem {

  @Prop() item!: Item;

  private readonly itemsClient: ItemsClient;

  constructor(){
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private handleClick(): void {
    console.log(this.itemsClient);
    
    // if (this.parentFolderId){
    //   // Here we need the item details (folder or file) and react accordingly.
    //   // this.itemsClient.getFolderContent(this.parentFolderId, 0, 0)
    //   // .then(data => {
    //   //   state.currentItems = data;
    //   //   this.showModal();
    //   // })
    //   // .catch(error => alert(error));
    //   // return;
    // }

    this.showModal();
  }

  private showModal(){
    const modal = document.createElement("dnn-modal");
    modal.backdropDismiss = false;
    modal.showCloseButton = false;
    // We also need to show a different model depending if we have a file or folder
    const editor = document.createElement("dnn-rm-create-folder");
    modal.appendChild(editor);
    document.body.appendChild(modal);
    modal.show();
  }

  render() {
    return (
      <Host>
        <button onClick={() => this.handleClick()}>
          <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0V0z" fill="none"/><path d="M14.06 9.02l.92.92L5.92 19H5v-.92l9.06-9.06M17.66 3c-.25 0-.51.1-.7.29l-1.83 1.83 3.75 3.75 1.83-1.83c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.2-.2-.45-.29-.71-.29zm-3.6 3.19L3 17.25V21h3.75L17.81 9.94l-3.75-3.75z"/></svg>
          <span>{state.localization.Edit}</span>
        </button>
      </Host>
    );
  }
}
