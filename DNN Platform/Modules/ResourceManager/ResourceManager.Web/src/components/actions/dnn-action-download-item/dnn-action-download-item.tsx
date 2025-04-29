import { Component, Host, h, Prop } from "@stencil/core";
import { Item, ItemsClient } from "../../../services/ItemsClient";
import state from "../../../store/store";

@Component({
  tag: "dnn-action-download-item",
  styleUrl: "../dnn-action.scss",
  shadow: true,
})
export class DnnActionDownloadItem {
  @Prop() item!: Item;

  private itemsClient: ItemsClient;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  private handleClick(): void {
    this.itemsClient.download(this.item.itemId, false);
  }

  render() {
    return (
      <Host>
        <button onClick={() => this.handleClick()}>
          <svg
            xmlns="http://www.w3.org/2000/svg"
            height="24px"
            viewBox="0 0 24 24"
            width="24px"
            fill="#000000"
          >
            <path d="M0 0h24v24H0V0z" fill="none" />
            <path
              xmlns="http://www.w3.org/2000/svg"
              d="M12 16 7 11 8.4 9.55 11 12.15V4H13V12.15L15.6 9.55L17 11ZM6 20Q5.175 20 4.588 19.413Q4 18.825 4 18V15H6V18Q6 18 6 18Q6 18 6 18H18Q18 18 18 18Q18 18 18 18V15H20V18Q20 18.825 19.413 19.413Q18.825 20 18 20Z"
            />
          </svg>
          <span>{state.localization.Download}</span>
        </button>
      </Host>
    );
  }
}
