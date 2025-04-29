import { Component, Host, h } from "@stencil/core";

@Component({
  tag: "dnn-rm-right-pane",
  styleUrl: "dnn-rm-right-pane.scss",
  shadow: true,
})
export class DnnRmRightPane {
  render() {
    return (
      <Host>
        <dnn-rm-actions-bar></dnn-rm-actions-bar>
        <dnn-rm-files-pane></dnn-rm-files-pane>
        <dnn-rm-status-bar></dnn-rm-status-bar>
      </Host>
    );
  }
}
