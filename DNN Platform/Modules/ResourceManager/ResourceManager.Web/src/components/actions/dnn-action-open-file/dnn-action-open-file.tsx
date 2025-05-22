import { Component, Host, h, Prop } from '@stencil/core';
import { Item } from '../../../services/ItemsClient';
import state from "../../../store/store";

@Component({
  tag: 'dnn-action-open-file',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionOpenFile {

  /** The item to open. */
  @Prop() item!: Item;

  private handleClick(): void {
    window.open(this.item.path, "_blank");
  }

  render() {
    return (
      <Host>
        <button onClick={() => this.handleClick()}>
        <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M200-120q-33 0-56.5-23.5T120-200v-560q0-33 23.5-56.5T200-840h280v80H200v560h560v-280h80v280q0 33-23.5 56.5T760-120H200Zm188-212-56-56 372-372H560v-80h280v280h-80v-144L388-332Z"/></svg>
          <span>{state.localization.OpenFile}</span>
        </button>
      </Host>
    );
  }
}
