import { Component, Host, h, Prop } from '@stencil/core';
import { Item } from '../../../services/ItemsClient';
import state from "../../../store/store";

@Component({
  tag: 'dnn-action-delete-items',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionDeleteItems {

  /** The list of items to delete. */
  @Prop() items!: Item[];

  private async handleClick() {
    await this.showModal();
  }

  private async showModal(){
    const modal = document.createElement("dnn-modal");
    modal.preventBackdropDismiss = true;
    modal.hideCloseButton = true;
    const editor = document.createElement("dnn-rm-delete-items");
    editor.items = this.items;
    modal.appendChild(editor);
    document.body.appendChild(modal);
    await modal.show();
  }

  render() {
    return (
      <Host>
        <button onClick={() => void this.handleClick()}>
          <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0V0z" fill="none"/><path d="M16 9v10H8V9h8m-1.5-6h-5l-1 1H5v2h14V4h-3.5l-1-1zM18 7H6v12c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7z"/></svg>
          <span>{state.localization.Delete}</span>
        </button>
      </Host>
    );
  }
}
