import { Component, Host, h, Prop } from "@stencil/core";
import { Item } from "../../../services/ItemsClient";
import state from "../../../store/store";

@Component({
  tag: "dnn-action-copy-url",
  styleUrl: "../dnn-action.scss",
  shadow: true,
})
export class DnnActionCopyUrl {
  @Prop() items!: Item[];

  private handleClick(): void {
    let t;
    if (this.items[0].path.includes(":")) {
      t = this.items[0].path;
    } else {
      t = `${window.location.protocol}//${window.location.host}${this.items[0].path}`;
    }
    navigator.clipboard.writeText(t);
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
            <path d="M17 7h-4v2h4c1.65 0 3 1.35 3 3s-1.35 3-3 3h-4v2h4c2.76 0 5-2.24 5-5s-2.24-5-5-5zm-6 8H7c-1.65 0-3-1.35-3-3s1.35-3 3-3h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-2zm-3-4h8v2H8z" />
          </svg>
          <span>{state.localization.CopyUrl}</span>
        </button>
      </Host>
    );
  }
}
